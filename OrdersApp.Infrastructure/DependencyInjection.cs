using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrdersApp.Application.Interfaces;
using OrdersApp.Infrastructure.Persistence;
using OrdersApp.Infrastructure.Persistence.Repositories;
using OrdersApp.Infrastructure.Services.Email;
using OrdersApp.Infrastructure.Services.Llm;

namespace OrdersApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<ILlmService, LlmService>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
