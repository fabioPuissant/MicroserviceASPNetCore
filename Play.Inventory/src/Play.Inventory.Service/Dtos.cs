using System;
namespace Play.Inventory.Service
{
	public record GrantItemDto(Guid UserId, Guid CatalogItemId, int Quantity);

	public record InventoryItemDto(Guid CatalogItemId, int Quality, DateTimeOffset AcquiredDate);
}

