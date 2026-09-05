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
        builder.Property(message => message.HangfireJobId).HasMaxLength(100).IsRequired(false);
        builder.Property(message => message.PublishedUtc).IsRequired(false);
        builder.Property(message => message.CreatedUtc).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(message => message.CreatedUtc).HasFilter("published_utc IS NULL");
        builder.HasIndex(message => message.CorrelationId);
        builder.HasIndex(message => new { message.EntityType, message.EntityId });
        builder.HasIndex(message => message.IdempotencyKey)
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");

        builder.ToTable(table => {
            table.HasCheckConstraint("ck_outbox_message_message_type", "length(trim(message_type)) > 0");
            table.HasCheckConstraint("ck_outbox_message_entity_type", "length(trim(entity_type)) > 0");
            table.HasCheckConstraint("ck_outbox_message_correlation_id", "length(trim(correlation_id)) > 0");
        });
    }

}
