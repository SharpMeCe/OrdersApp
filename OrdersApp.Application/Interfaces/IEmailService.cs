using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrdersApp.Application.DTOs;

namespace OrdersApp.Application.Interfaces
{
    public interface IEmailService
    {
        Task<List<MailMessageDto>> FetchOrderEmailsAsync();
    }
}
