using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Entities;

namespace ProjectLogistiXX.Infrastructure.Persistence.Configurations
{
    public class IdempotentRequestConfiguration : IEntityTypeConfiguration<IdempotentRequest>
    {
        public void Configure(EntityTypeBuilder<IdempotentRequest> builder)
        {
            builder.ToTable("IdempotentRequests");

            builder.HasKey(x => new { x.Id, x.UserId })
                .IsClustered(false);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasConversion(
                    id => id.Value,
                    value => new IdempotencyKeyId(value));

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasConversion(
                    userId => userId.Value,
                    value => new UserId(value));

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(rsp => rsp.Response)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(sts => sts.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasIndex(x => x.CreatedAtUtc)
                .IsClustered()
                .HasDatabaseName("IX_IdempotentRequests_CreatedAtUtc");

            //builder.Property<byte[]>("RowVersion")
            //    .IsRowVersion();
        }
    }
}
