using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersApp.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; } 

        [Column("product_name")] 
        public string ProductName { get; set; } = string.Empty;

        [Column("quantity")] 
        public int Quantity { get; set; }

        [Column("price")] 
        public decimal Price { get; set; }

        [Column("order_date")] 
        public DateTime OrderDate { get; set; }

        [Column("source_email")] 
        public string SourceEmail { get; set; } = string.Empty;
    }
}