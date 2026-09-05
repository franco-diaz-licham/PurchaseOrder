using Hangfire;
using Hangfire.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Infrastructure;
using PurchaseOrderApp.Infrastructure.Background;
using PurchaseOrderApp.Infrastructure.Repositories;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Infrastructure;

[NonParallelizable]
public sealed class OutboxHangfireTests : DatabaseFixture
{
    private IAuditJobQueue Queue => new HangfireAuditJobQueue(new BackgroundJobClient(Storage));
    private OutboxProcessor Relay(DatabaseContext db, IAuditJobQueue? queue = null) =>
        new(db, queue ?? Queue, NullLogger<OutboxProcessor>.Instance);

    [TestCase(true)]
    [TestCase(false)]
    public async Task BusinessTransaction_ShouldCommitOrRollbackOutboxBeforeRelay(bool commit)
    {
        var uow = new UnitOfWork(Db, new DomainEventDispatcher(Db));
        await AddReservationAsync(uow);
        await uow.SaveChangesAsync(CancellationToken.None);
        await using (var observer = CreateDatabaseContext()) {
            (await observer.StockReservations.CountAsync()).ShouldBe(0);
            (await observer.OutboxMessages.CountAsync()).ShouldBe(0);
            await Relay(observer).ProcessPendingAsync(CancellationToken.None);
        }
        Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(0);

        if (commit) await uow.CommitTransactionAsync();
        else await uow.RollbackTransactionAsync();

        await using var verification = CreateDatabaseContext();
        (await verification.StockReservations.CountAsync()).ShouldBe(commit ? 1 : 0);
        (await verification.OutboxMessages.CountAsync()).ShouldBe(commit ? 1 : 0);
        await Relay(verification).ProcessPendingAsync(CancellationToken.None);
        Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(commit ? 1 : 0);

        if (commit) {
            var message = await verification.OutboxMessages.AsNoTracking().SingleAsync();
            message.PublishedUtc.ShouldNotBeNull();
            message.HangfireJobId.ShouldNotBeNullOrWhiteSpace();
            await Relay(verification).ProcessPendingAsync(CancellationToken.None);
            Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(1);
            (await verification.AuditLogEntries.CountAsync()).ShouldBe(0);
            await RunWorkerAsync(1);
            (await verification.AuditLogEntries.CountAsync()).ShouldBe(1);
            (await verification.OutboxMessages.AsNoTracking().SingleAsync()).PublishedUtc.ShouldNotBeNull();
        }
    }

    [Test]
    public async Task QueueOutage_ShouldRetainCommittedIntentForLaterDelivery()
    {
        await CommitMessageAsync();
        await Relay(Db, new UnavailableQueue()).ProcessPendingAsync(CancellationToken.None);
        var failed = await Db.OutboxMessages.AsNoTracking().SingleAsync();
        failed.PublishedUtc.ShouldBeNull();
        failed.HangfireJobId.ShouldBeNull();
        Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(0);
        (await Db.StockReservations.CountAsync()).ShouldBe(1);
        await Relay(Db).ProcessPendingAsync(CancellationToken.None);
        Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(1);
    }

    [Test]
    public async Task LostEnqueueAcknowledgement_ShouldAllowDuplicateJobsButOnlyOneAuditEntry()
    {
        await CommitMessageAsync();
        await Relay(Db, new LostAcknowledgementQueue(Queue)).ProcessPendingAsync(CancellationToken.None);
        Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(1);
        (await Db.OutboxMessages.AsNoTracking().SingleAsync()).PublishedUtc.ShouldBeNull();
        await Relay(Db).ProcessPendingAsync(CancellationToken.None);
        Storage.GetMonitoringApi().EnqueuedCount("audit").ShouldBe(2);
        await RunWorkerAsync(2, workerCount: 2);
        (await Db.AuditLogEntries.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task ConcurrentPublishers_ShouldNotDuplicateAudit()
    {
        await CommitMessageAsync();
        await using var first = CreateDatabaseContext();
        await using var second = CreateDatabaseContext();
        await Task.WhenAll(
            Relay(first).ProcessPendingAsync(CancellationToken.None),
            Relay(second).ProcessPendingAsync(CancellationToken.None));
        var jobs = (int)Storage.GetMonitoringApi().EnqueuedCount("audit");
        jobs.ShouldBeInRange(1, 2);
        await RunWorkerAsync(jobs, workerCount: 2);
        (await Db.AuditLogEntries.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task TransientHandlerFailure_ShouldUseHangfireRetryAndHistory()
    {
        await CommitMessageAsync();
        await Relay(Db).ProcessPendingAsync(CancellationToken.None);
        var message = await Db.OutboxMessages.AsNoTracking().SingleAsync();
        var interceptor = new FailFirstAuditSave();
        await RunWorkerAsync(1, interceptor);
        interceptor.Attempts.ShouldBe(2);
        using var connection = Storage.GetConnection();
        connection.GetJobParameter(message.HangfireJobId!, "RetryCount").ShouldBe("1");
        Storage.GetMonitoringApi().JobDetails(message.HangfireJobId!).History
            .ShouldContain(state => state.StateName == "Scheduled");
        (await Db.AuditLogEntries.CountAsync()).ShouldBe(1);
    }

    private async Task CommitMessageAsync()
    {
        var uow = new UnitOfWork(Db, new DomainEventDispatcher(Db));
        await AddReservationAsync(uow);
        await uow.SaveChangesAsync(CancellationToken.None);
        await uow.CommitTransactionAsync();
        Db.ChangeTracker.Clear();
    }

    private async Task AddReservationAsync(UnitOfWork uow)
    {
        var seed = await PurchaseOrderScenarioSeeder.SeedApprovedLineAsync(Db);
        await uow.BeginTransactionAsync();
        var order = await Db.PurchaseOrders.Include(order => order.Lines).SingleAsync(order => order.Id == seed.PurchaseOrderId);
        var item = await Db.InventoryItems.SingleAsync(item => item.Id == seed.InventoryItemId);
        var stock = await new WarehouseStockRepository(Db).GetForUpdateAsync(seed.WarehouseId, seed.InventoryItemId, CancellationToken.None);
        Db.StockReservations.Add(StockReservationDomainService.Reserve(order, seed.PurchaseOrderLineId,
            stock!, item, Quantity.Zero, new Quantity(5), TestData.User, TestData.OccurredAt));
    }


    private async Task RunWorkerAsync(int expected, SaveChangesInterceptor? interceptor = null, int workerCount = 1)
    {
        var services = new ServiceCollection();
        services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention();
            if (interceptor is not null) options.AddInterceptors(interceptor);
        });
        services.AddTransient<StockAuditJob>();
        await using var provider = services.BuildServiceProvider();
        using var server = new BackgroundJobServer(new BackgroundJobServerOptions {
            Queues = ["audit"], WorkerCount = workerCount,
            SchedulePollingInterval = TimeSpan.FromMilliseconds(100),
            Activator = new AspNetCoreJobActivator(provider.GetRequiredService<IServiceScopeFactory>())
        }, Storage);
        await HangfireTestWorker.WaitForSuccessAsync(Storage, expected);
    }

    private sealed class UnavailableQueue : IAuditJobQueue
    {
        public string Enqueue(Guid messageId) => throw new InvalidOperationException("Queue storage unavailable.");
    }

    private sealed class LostAcknowledgementQueue(IAuditJobQueue inner) : IAuditJobQueue
    {
        public string Enqueue(Guid messageId)
        {
            inner.Enqueue(messageId);
            throw new InvalidOperationException("Enqueue succeeded but the acknowledgement was lost.");
        }
    }

    private sealed class FailFirstAuditSave : SaveChangesInterceptor
    {
        private int _attempts;
        public int Attempts => _attempts;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _attempts) == 1) throw new TimeoutException("Injected audit failure.");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}