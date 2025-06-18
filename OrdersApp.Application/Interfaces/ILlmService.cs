using OrdersApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersApp.Application.Interfaces
{
    public interface ILlmService
    {
        Task<List<OrderItemDto>> ExtractOrderItemsAsync(string mailBody);
    }
}
