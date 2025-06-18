using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using OrdersApp.Domain.Entities;
using OrdersApp.Application.Interfaces;
using System.Text.Json;
using System.IO;
using System.Linq;

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

        Console.WriteLine($"Wybrano {filteredUids.Count} maili z frazą 'zamowienie'");

        foreach (var uid in filteredUids)
        {
            var message = await inbox.GetMessageAsync(uid);
            Console.WriteLine($"✉️ START ekstrakcji zamówienia z maila: {message.Subject}");

            var contentsToProcess = new List<string>();

            // 1. Dodaj treść główną maila (jeśli istnieje)
            var body = message.TextBody ?? message.HtmlBody ?? "";
            if (!string.IsNullOrWhiteSpace(body))
            {
                contentsToProcess.Add(body);
                Console.WriteLine("Dodano treść maila do przetworzenia.");
            }

            // 2. Przetwórz załączniki
            foreach (var attachment in message.Attachments)
            {
                if (attachment is MessagePart messagePart)
                {
                    Console.WriteLine($"Wykryto załącznik .eml (zagnieżdżona wiadomość) - przetwarzam jego treść.");
                    var nestedBody = messagePart.Message?.TextBody ?? messagePart.Message?.HtmlBody ?? "";
                    if (!string.IsNullOrWhiteSpace(nestedBody))
                    {
                        contentsToProcess.Add(nestedBody);
                        Console.WriteLine($"Dodano treść z załącznika .eml do przetworzenia.");
                    }
                }
                else if (attachment is MimePart mimePart) 
                {
                    using (var stream = new MemoryStream())
                    {
                        await mimePart.Content.DecodeToAsync(stream);
                        stream.Position = 0;

                        try
                        {
                            using (var reader = new StreamReader(stream, mimePart.ContentType.Charset != null ? System.Text.Encoding.GetEncoding(mimePart.ContentType.Charset) : System.Text.Encoding.UTF8))
                            {
                                var attachmentText = await reader.ReadToEndAsync();
                                if (!string.IsNullOrWhiteSpace(attachmentText))
                                {
                                    contentsToProcess.Add(attachmentText);
                                    Console.WriteLine($"Dodano załącznik '{mimePart.FileName}' do przetworzenia.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Nie udało się odczytać załącznika '{mimePart.FileName}' jako tekst: {ex.Message}");
                        }
                    }
                }
            }

            if (!contentsToProcess.Any())
            {
                Console.WriteLine("Brak treści ani tekstowych załączników do przetworzenia – pomijam.");
                continue;
            }

            var combinedContent = string.Join("\n\n--- KONIEC SEKCJI ---\n\n", contentsToProcess);

            try
            {
                var gptResponse = await _llmService.ExtractOrderItemsAsync(combinedContent);

                if (gptResponse == null || !gptResponse.Any())
                {
                    Console.WriteLine($"Brak danych do zapisania dla: {message.Subject}");
                    continue;
                }

                var entities = gptResponse.Select(item => new OrderItem
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    OrderDate = DateTime.Now,
                    SourceEmail = message.From.Mailboxes.FirstOrDefault()?.Address ?? email
                }).ToList();

                await _orderRepository.SaveOrderItemsAsync(entities);
                Console.WriteLine($"Zapisano {entities.Count} pozycji z: {message.Subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd LLM lub zapisu: {ex.Message}");
            }
        }

        await client.DisconnectAsync(true);
        Console.WriteLine("Rozłączono z Gmailem.");
    }
}