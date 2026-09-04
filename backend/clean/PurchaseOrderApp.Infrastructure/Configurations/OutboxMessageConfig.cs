using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApp.Infrastructure.Background;

namespace PurchaseOrderApp.Infrastructure.Configurations;

public sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_message", "background");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.MessageType).HasMaxLength(200).IsRequired();
        builder.Property(message => message.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(message => message.EntityId).IsRequired();
        builder.Property(message => message.Payload)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();
        builder.Property(message => message.OccurredUtc).IsRequired();
        builder.Property(message => message.CorrelationId).HasMaxLength(200).IsRequired();
        builder.Property(message => message.ActorUserId).IsRequired(false);
        builder.Property(message => message.IdempotencyKey).HasMaxLength(300).IsRequired(false);
        builder.Property(message => message.Status)
            .HasConversion(
                status => ToProviderValue(status),
                value => FromProviderValue(value))
            .HasMaxLength(40)
            .HasDefaultValue(OutboxMessageStatus.Pending)
            .HasSentinel(OutboxMessageStatus.Unspecified)
            .IsRequired();
        builder.Property(message => message.AttemptCount).HasDefaultValue(0).IsRequired();
        builder.Property(message => message.NextAttemptUtc).HasDefaultValueSql("now()").IsRequired();
        builder.Property(message => message.LockedBy).HasMaxLength(200).IsRequired(false);
        builder.Property(message => message.LockedUntilUtc).IsRequired(false);
        builder.Property(message => message.ProcessedUtc).IsRequired(false);
        builder.Property(message => message.LastError).HasMaxLength(1000).IsRequired(false);
        builder.Property(message => message.CreatedUtc).HasDefaultValueSql("now()").IsRequired();
        builder.Property(message => message.UpdatedUtc).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(message => new { message.Status, message.NextAttemptUtc });
        builder.HasIndex(message => message.LockedUntilUtc);
        builder.HasIndex(message => message.CorrelationId);
        builder.HasIndex(message => new { message.EntityType, message.EntityId });
        builder.HasIndex(message => message.IdempotencyKey)
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");

        builder.ToTable(table => {
            table.HasCheckConstraint("ck_outbox_message_attempt_count", "attempt_count >= 0");
            table.HasCheckConstraint("ck_outbox_message_message_type", "length(trim(message_type)) > 0");
            table.HasCheckConstraint("ck_outbox_message_entity_type", "length(trim(entity_type)) > 0");
            table.HasCheckConstraint("ck_outbox_message_correlation_id", "length(trim(correlation_id)) > 0");
            table.HasCheckConstraint(
                "ck_outbox_message_status",
                "status IN ('pending', 'processing', 'processed', 'failed', 'dead_lettered')");
        });
    }

    private static string ToProviderValue(OutboxMessageStatus status)
    {
        return status switch {
            OutboxMessageStatus.Pending => "pending",
            OutboxMessageStatus.Processing => "processing",
            OutboxMessageStatus.Processed => "processed",
            OutboxMessageStatus.Failed => "failed",
            OutboxMessageStatus.DeadLettered => "dead_lettered",
            _ => throw new InvalidOperationException($"Unknown outbox message status: {status}.")
        };
    }

    private static OutboxMessageStatus FromProviderValue(string value)
    {
        return value switch {
            "pending" => OutboxMessageStatus.Pending,
            "processing" => OutboxMessageStatus.Processing,
            "processed" => OutboxMessageStatus.Processed,
            "failed" => OutboxMessageStatus.Failed,
            "dead_lettered" => OutboxMessageStatus.DeadLettered,
            _ => throw new InvalidOperationException($"Unknown outbox message status value: {value}.")
        };
    }
}
