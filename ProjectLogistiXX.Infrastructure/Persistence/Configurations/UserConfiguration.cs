using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Entities;
using ProjectLogistiXX.Domain.ValueObjects;

namespace ProjectLogistiXX.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value));

        builder.Property(user => user.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasConversion(
                email => email!.Value,
                value => new Email(value))
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.Username)
            .IsUnique();

        builder.Property(user => user.PhoneNumber)
            .HasConversion(
                phoneNumber => phoneNumber!.ToDatabaseFormat(),
                value => new PhoneNumber(value))
            .HasMaxLength(15)
            .IsRequired();

        builder.HasIndex(user => user.PhoneNumber)
            .IsUnique();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.UpdatedAt);

        builder.Property(user => user.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(user => !user.IsDeleted);

        builder.OwnsOne(user => user.Address, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(150)
                .HasColumnName("AddressStreet")
                .IsRequired();

            address.Property(a => a.BuildingDetails)
                .HasMaxLength(100)
                .HasColumnName("AddressBuildingDetails");

            address.Property(a => a.City)
                .HasMaxLength(100)
                .HasColumnName("AddressCity")
                .IsRequired();

            address.Property(a => a.County)
                .HasMaxLength(100)
                .HasColumnName("AddressCounty")
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("AddressPostalCode");

            address.Property(a => a.Country)
                .HasMaxLength(100)
                .HasColumnName("AddressCountry")
                .IsRequired();
        });

        builder.Navigation(user => user.Address).IsRequired();

        builder.Ignore(user => user.DomainEvents);
    }
}
