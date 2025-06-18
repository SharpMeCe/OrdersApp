using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using OrdersApp.Application.Interfaces;
using OrdersApp.Domain.Entities;
using OrdersApp.Infrastructure.Persistence;

namespace OrdersApp.WebUI.DebugTools
{
    public class EmailTestConsole
    {
        private readonly IServiceProvider _serviceProvider;

        public EmailTestConsole(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task TestConnection()
        {
            Console.WriteLine("📨 Start testu połączenia z Gmailem...");

            List<MimeMessage> emails = [];

            try
            {
                using var client = new ImapClient();
                await client.ConnectAsync("imap.gmail.com", 993, true);
                await client.AuthenticateAsync("krystiangugala888@gmail.com", "qpxrrnfwprwywina");

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);
                Console.WriteLine($"✅ Zalogowano! W skrzynce: {inbox.Count} wiadomości.");

                var uids = await inbox.SearchAsync(SearchQuery.All);
                foreach (var uid in uids.Reverse().Take(10))
                {
                    var message = await inbox.GetMessageAsync(uid);
                    Console.WriteLine($"✔️ Mail: {message.Subject}");
                    emails.Add(message);
                }

                await client.DisconnectAsync(true);
                Console.WriteLine("🔌 Rozłączono z Gmailem.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Błąd IMAP: {ex.Message}");
            }

            // === PRZETWARZANIE MAILI PRZEZ GPT I ZAPIS DO BAZY ===
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IEmailProcessorService>();

                foreach (var email in emails)
                {
                    var items = await processor.ExtractOrderItemsFromEmailAsync(email);
                    Console.WriteLine($"📥 {email.Subject} => {items.Count} pozycji zapisanych do bazy.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Błąd LLM/zapisu do bazy: {ex.Message}");
            }

            // === WYPISANIE ZAWARTOŚCI TABELI ===
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                var orders = await db.OrderItems.ToListAsync();

                Console.WriteLine($"📦 Znaleziono {orders.Count} zamówień w bazie:");
                foreach (var item in orders)
                {
                    Console.WriteLine($"🧾 {item.ProductName} | Ilość: {item.Quantity} | Cena: {item.Price} | Data: {item.OrderDate}");
                }

                if (orders.Count == 0)
                    Console.WriteLine("⚠️ Brak zamówień w tabeli `order_items`.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Błąd przy odczycie z bazy: {ex.Message}");
            }
        }
    }
}
