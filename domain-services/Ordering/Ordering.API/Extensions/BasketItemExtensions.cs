using Ordering.API.Application.Commands;
using Ordering.API.Application.Models;

namespace Ordering.API.Extensions;

public static class BasketItemExtensions
{
    public static IEnumerable<CreateOrderCommand.OrderItemDTO> ToOrderItemsDTO(this IEnumerable<BasketItem> basketItems)
    {
        foreach (var item in basketItems)
        {
            yield return item.ToOrderItemDTO();
        }
    }

    public static CreateOrderCommand.OrderItemDTO ToOrderItemDTO(this BasketItem item)
    {
        return new CreateOrderCommand.OrderItemDTO()
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            PictureUrl = item.PictureUrl,
            UnitPrice = item.UnitPrice,
            Units = item.Quantity
        };
    }
}