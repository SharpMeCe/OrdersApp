using OrdersApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OrdersApp.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task AddOrderItemAsync(OrderItem item);
        Task SaveOrderItemsAsync(List<OrderItem> items);
        Task<List<OrderItem>> GetAllOrderItemsAsync();
    }
}
