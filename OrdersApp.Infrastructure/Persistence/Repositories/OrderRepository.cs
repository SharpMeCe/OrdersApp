using Microsoft.EntityFrameworkCore;
using OrdersApp.Application.Interfaces;
using OrdersApp.Domain.Entities;

namespace OrdersApp.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task SaveOrderItemsAsync(List<OrderItem> items)
    {
        _context.OrderItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderItem>> GetAllOrderItemsAsync()
    {
        return await _context.OrderItems.OrderByDescending(x => x.OrderDate).ToListAsync();
    }

    public async Task AddOrderItemAsync(OrderItem item)
    {
        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();
    }
}
