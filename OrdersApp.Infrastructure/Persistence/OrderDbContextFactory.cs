﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrdersApp.Infrastructure.Persistence
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseMySql(
                "server=localhost;port=3306;user=root;password=root;database=ordersdb - YOU NUST SET IT FOR YOUR LOCAL SETTINGS",
                ServerVersion.AutoDetect("server=localhost;port=3306;user=root;password=root;database=ordersdb - YOU NUST SET IT FOR YOUR LOCAL SETTINGS"));

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
