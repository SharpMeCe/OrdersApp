using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersApp.Application.DTOs
{
    public class MailMessageDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<byte[]> Attachments { get; set; } = new();
    }
}
