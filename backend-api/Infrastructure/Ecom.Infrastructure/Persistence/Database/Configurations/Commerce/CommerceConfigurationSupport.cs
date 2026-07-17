using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

internal static class CommerceConfigurationSupport
{
    internal const string ActiveRowFilter = "\"IsDeleted\" = false";

    internal static IndexBuilder Unique<TEntity>(EntityTypeBuilder<TEntity> builder, params string[] properties)
        where TEntity : BaseEntity => builder.HasIndex(properties).IsUnique().HasFilter(ActiveRowFilter);

    internal static IndexBuilder UniqueWhere<TEntity>(EntityTypeBuilder<TEntity> builder, string filter, params string[] properties)
        where TEntity : BaseEntity => builder.HasIndex(properties).IsUnique().HasFilter(filter);

    internal static PropertyBuilder<decimal> Money(PropertyBuilder<decimal> property) =>
        property.HasPrecision(CommerceConstants.MoneyPrecision, CommerceConstants.MoneyScale);

    internal static PropertyBuilder<decimal?> Money(PropertyBuilder<decimal?> property) =>
        property.HasPrecision(CommerceConstants.MoneyPrecision, CommerceConstants.MoneyScale);

    internal static PropertyBuilder<decimal> Quantity(PropertyBuilder<decimal> property) =>
        property.HasPrecision(CommerceConstants.QuantityPrecision, CommerceConstants.QuantityScale);

    internal static PropertyBuilder<decimal?> Quantity(PropertyBuilder<decimal?> property) =>
        property.HasPrecision(CommerceConstants.QuantityPrecision, CommerceConstants.QuantityScale);
}
