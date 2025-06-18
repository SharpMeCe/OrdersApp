using OrdersApp.Application.DTOs;
using OrdersApp.Domain.Entities;

namespace OrdersApp.Application.Mappers;

public static class OrderItemMapper
{
    public static OrderItem ToEntity(this OrderItemDto dto)
    {
        return new OrderItem
        {
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            Price = dto.Price
        };
    }
}
