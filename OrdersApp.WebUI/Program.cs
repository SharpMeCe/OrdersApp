using Microsoft.EntityFrameworkCore;
using OrdersApp.Application.Interfaces;
using OrdersApp.Infrastructure.Persistence;
using OrdersApp.Infrastructure.Persistence.Repositories;
using OrdersApp.Infrastructure.Services.Email;
using OrdersApp.Infrastructure.Services.Llm;
using OrdersApp.WebUI.Components;
using OrdersApp.Infrastructure.Config;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Konfiguracja OpenAI
builder.Services.Configure<OpenAiSettings>(
    builder.Configuration.GetSection("OpenAI"));

// 🔌 Baza danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 📦 Rejestracja serwisów
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddScoped<EmailOrderImporterService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ✅ Uruchom TYLKO importer maili
using (var scope = app.Services.CreateScope())
{
    var importer = scope.ServiceProvider.GetRequiredService<EmailOrderImporterService>();
    await importer.FetchAndImportOrdersAsync("putEmailAddress", "PutEmailKey");
}

await app.RunAsync();
