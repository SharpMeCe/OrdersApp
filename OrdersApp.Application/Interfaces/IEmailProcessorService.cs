using System.Collections.Generic;
using System.Threading.Tasks;
using MimeKit;
using OrdersApp.Application.DTOs;

namespace OrdersApp.Application.Interfaces
{
    public interface IEmailProcessorService
    {
        Task<List<MimeMessage>> FetchRecentEmailsAsync(string host, int port, bool useSsl, string username, string password);
        Task<List<OrderItemDto>> ExtractOrderItemsFromEmailAsync(MimeMessage email);
        Task ProcessEmailsAsync();
    }
}
