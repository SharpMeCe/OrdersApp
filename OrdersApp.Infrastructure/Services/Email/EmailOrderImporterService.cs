using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using OrdersApp.Domain.Entities;
using OrdersApp.Application.Interfaces;
using System.Text.Json;

namespace OrdersApp.Infrastructure.Services.Email;

public class EmailOrderImporterService
{
    private readonly ILlmService _llmService;
    private readonly IOrderRepository _orderRepository;

    public EmailOrderImporterService(ILlmService llmService, IOrderRepository orderRepository)
    {
        _llmService = llmService;
        _orderRepository = orderRepository;
    }

    public async Task FetchAndImportOrdersAsync(string email, string password)
    {
        using var client = new ImapClient();
        await client.ConnectAsync("imap.gmail.com", 993, true);
        await client.AuthenticateAsync(email, password);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly);

        var allUids = await inbox.SearchAsync(SearchQuery.All);
        var last50Uids = allUids.Reverse().Take(50).ToList();

        var filteredUids = new List<UniqueId>();

        foreach (var uid in last50Uids)
        {
            var message = await inbox.GetMessageAsync(uid);

            if (message.Subject != null &&
                message.Subject.ToLower().Contains("zamowienie"))
            {
                filteredUids.Add(uid);

                if (filteredUids.Count >= 5)
                    break;
            }
        }

        Console.WriteLine($"📬 Wybrano {filteredUids.Count} maili z frazą 'zamowienie'");

        foreach (var uid in filteredUids)
        {
            var message = await inbox.GetMessageAsync(uid);
            Console.WriteLine($"✉️ START ekstrakcji zamówienia z maila: {message.Subject}");

            var body = message.TextBody ?? message.HtmlBody ?? "";

            if (string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("⚠️ Brak treści – pomijam.");
                continue;
            }

            try
            {
                var gptResponse = await _llmService.ExtractOrderItemsAsync(body);

                if (gptResponse == null || gptResponse.Count == 0)
                {
                    Console.WriteLine($"⚠️ Brak danych do zapisania dla: {message.Subject}");
                    continue;
                }

                var entities = gptResponse.Select(item => new OrderItem
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    OrderDate = DateTime.Now,
                    SourceEmail = message.From.Mailboxes.FirstOrDefault()?.Address
                }).ToList();

                await _orderRepository.SaveOrderItemsAsync(entities);
                Console.WriteLine($"✅ Zapisano {entities.Count} pozycji z: {message.Subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Błąd LLM: {ex.Message}");
            }
        }

        await client.DisconnectAsync(true);
        Console.WriteLine("🔌 Rozłączono z Gmailem.");
    }
}
