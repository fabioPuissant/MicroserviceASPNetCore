using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service;
using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Play.Inventory.Service.Clients;

namespace Play.Inventory.Service.Controllers
{
	[ApiController]
	[Route("items")]
	public class ItemsController : ControllerBase
	{
		private readonly IRepository<InventoryItem> _inventoryItemRepository;
		private readonly IRepository<CatalogItem> _catalogItemRepository;
		// before rabbitMQ consumer// private readonly CatalogClient _catalogClient;

		public ItemsController(IRepository<InventoryItem> repository, IRepository<CatalogItem> catalogItemRepository)
		{
			_inventoryItemRepository = repository;
			// before rabbitMQ consumer//_catalogClient = catalogClient;
			_catalogItemRepository = catalogItemRepository;
        }

        [HttpGet]
		public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync([FromQuery] Guid userId)
		{
			if (userId == Guid.Empty)
			{
				return BadRequest();
			}
            #region before RabbitMQ consumer (code with client)
            // before rabbitMQ consumer// var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            // before rabbitMQ consumer//var inventoryItemEntities = await _inventoryItemRepository.GetAllAsync(i => i.UserId == userId);

            // before rabbitMQ consumer// var inventoryItemDtos = inventoryItemEntities.Select(invItem => {
            // before rabbitMQ consumer// var catalogItem = catalogItems.Single(ci => ci.Id == invItem.CatalogItemId);
            // before rabbitMQ consumer// return invItem.AsDto(catalogItem.Name, catalogItem.Description);					
            // before rabbitMQ consumer//});

            // before rabbitMQ consumer//return Ok(inventoryItemDtos);

            #endregion
            var inventoryItemEntities = await _inventoryItemRepository.GetAllAsync(i => i.UserId == userId);
			var itemIds = inventoryItemEntities.Select(item => item.Id);
			var catalogItemEntities = await _catalogItemRepository.GetAllAsync(item => itemIds.Contains(item.Id));

			var inventoryItemDtos = inventoryItemEntities.Select(invItem => {
				var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == invItem.CatalogItemId);
                return invItem.AsDto(catalogItem.Name, catalogItem.Description);					
                });
            }


            [HttpPost]
		public async Task<ActionResult> PostAsync(GrantItemDto grantItemDto)
		{
			var inventoryItem = await _inventoryItemRepository.GetAsync(item => item.UserId == grantItemDto.UserId
									&& item.CatalogItemId == grantItemDto.CatalogItemId);
			if (inventoryItem == null)
            {
				inventoryItem = new InventoryItem
				{
					CatalogItemId = grantItemDto.CatalogItemId,
					UserId = grantItemDto.UserId,
					Quantity = grantItemDto.Quantity,
					AcquiredDate = DateTimeOffset.UtcNow
				};

				await _inventoryItemRepository.CreateAsync(inventoryItem);

            }

			else
            {
				inventoryItem.Quantity += grantItemDto.Quantity;
				await _inventoryItemRepository.UpdateAsync(inventoryItem);
            }

			return Ok();
	}
	}
}

