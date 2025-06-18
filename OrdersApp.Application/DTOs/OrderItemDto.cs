using System.Text.Json.Serialization; 

namespace OrdersApp.Application.DTOs
{
    public class OrderItemDto
    {
        [JsonPropertyName("productName")] 
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")] 
        public int Quantity { get; set; }

        [JsonPropertyName("price")] 
        public decimal Price { get; set; }
    }
}