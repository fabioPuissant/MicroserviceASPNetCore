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
		private readonly IRepository<InventoryItem> _itemRepository;
		private readonly CatalogClient _catalogClient;

		public ItemsController(IRepository<InventoryItem> repository, CatalogClient catalogClient)
		{
			_itemRepository = repository;
			_catalogClient = catalogClient;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync([FromQuery] Guid userId)
		{
			if (userId == Guid.Empty)
			{
				return BadRequest();
			}

			var catalogItems = await _catalogClient.GetCatalogItemsAsync();
			var inventoryItemEntities = await _itemRepository.GetAllAsync(i => i.UserId == userId);

			var inventoryItemDtos = inventoryItemEntities.Select(invItem => {
				var catalogItem = catalogItems.Single(ci => ci.Id == invItem.CatalogItemId);
				return invItem.AsDto(catalogItem.Name, catalogItem.Description);					
			});

			return Ok(inventoryItemDtos);
		}


		[HttpPost]
		public async Task<ActionResult> PostAsync(GrantItemDto grantItemDto)
		{
			var inventoryItem = await _itemRepository.GetAsync(item => item.UserId == grantItemDto.UserId
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

				await _itemRepository.CreateAsync(inventoryItem);

            }

			else
            {
				inventoryItem.Quantity += grantItemDto.Quantity;
				await _itemRepository.UpdateAsync(inventoryItem);
            }

			return Ok();
	}
	}
}

