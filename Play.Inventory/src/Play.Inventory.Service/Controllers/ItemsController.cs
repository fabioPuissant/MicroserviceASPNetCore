using System;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ItemsController : ControllerBase
	{
		private readonly IRepository<InventoryItem> _itemRepository;

		public ItemsController(IRepository<InventoryItem> repository)
		{
			_itemRepository = repository;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
		{
			if (userId == Guid.Empty)
			{
				return BadRequest();
			}

			var items = (await _itemRepository.GetAllAsync(item => item.UserId == userId))
												.Select(items => items.AsDto());

			return Ok(items);
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

