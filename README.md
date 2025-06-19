# OrdersApp

OrdersApp to aplikacja konsolowa/serwis (lub usługa w tle), której głównym zadaniem jest automatyczne importowanie zamówień z wiadomości e-mail. Wykorzystuje sztuczną inteligencję (Large Language Model - LLM) do ekstrakcji szczegółów zamówień z treści maili oraz załączników (w tym plików .eml), a następnie zapisuje je w bazie danych.

## Spis Treści

- [Opis Projektu](#opis-projektu)
- [Funkcjonalności](#funkcjonalności)
- [Technologie](#technologie)
- [Wymagania](#wymagania)
- [Konfiguracja](#konfiguracja)
- [Uruchamianie](#uruchamianie)
- [Struktura Projektu](#struktura-projektu)
- [Autor](#autor)

## Opis Projektu

Głównym celem OrdersApp jest usprawnienie procesu przyjmowania zamówień poprzez automatyzację ekstrakcji danych z wiadomości e-mail. Aplikacja łączy się ze skrzynką pocztową (obecnie Gmail IMAP), wyszukuje maile z określonymi frazami w temacie (np. "zamowienie"), a następnie analizuje ich treść oraz załączniki (także zagnieżdżone maile w formacie .eml), aby wyodrębnić informacje o produktach, ilościach i cenach. Wyekstrahowane dane są następnie zapisywane w lokalnej bazie danych.

## Funkcjonalności

* **Pobieranie E-maili:** Łączenie się z serwerem IMAP (np. Gmail) w celu pobierania najnowszych wiadomości.
* **Filtrowanie Wiadomości:** Wyszukiwanie maili zawierających specyficzne frazy w temacie ("zamowienie").
* **Ekstrakcja Treści:**
    * Pobieranie treści głównej wiadomości (tekstowej i HTML).
    * Wykrywanie i odczytywanie tekstowych załączników (np. `.txt`, `.csv`).
    * Wykrywanie i odczytywanie treści ze zagnieżdżonych wiadomości e-mail (załączniki `.eml`).
* **Analiza za pomocą LLM:** Wykorzystanie zewnętrznego modelu językowego (LLM) do parsowania i strukturyzowania danych zamówienia (nazwa produktu, ilość, cena).
* **Zapis do Bazy Danych:** Persistowanie wyodrębnionych pozycji zamówienia do bazy danych (np. MySQL).

## Technologie

* **Język:** C# (.NET)
* **Pobieranie E-maili:** [MailKit](https://github.com/jstedfast/MailKit) (Biblioteka do IMAP/POP3/SMTP)
* **Obsługa Danych:** [MimeKit](https://github.com/jstedfast/MimeKit) (Część MailKit, do parsowania MIME)
* **Baza Danych:** Entity Framework Core (OR/M), prawdopodobnie z providerem dla MySQL (np. Pomelo.EntityFrameworkCore.MySql, wnioskując po logach SQL)
* **Integracja LLM:** Dedykowany serwis do komunikacji z modelem LLM (np. OpenAI GPT, Google Gemini, itp.)
* **Zarządzanie Wersjami:** Git

## Wymagania

* .NET SDK (najnowsza wersja LTS, np. .NET 8)
* Dostęp do konta e-mail IMAP (np. Gmail) z włączoną opcją dostępu dla mniej bezpiecznych aplikacji lub hasłem do aplikacji (jeśli używasz uwierzytelniania dwuskładnikowego).
* Klucz API dla używanego modelu LLM (szczegóły konfiguracji w `appsettings.json` lub zmiennych środowiskowych).
* Działająca instancja bazy danych MySQL (lub innej, jeśli zmieniono konfigurację EF Core).

## Konfiguracja

1.  **Sklonuj Repozytorium:**
    ```bash
    git clone <URL_TWOJEGO_REPOZYTORIUM>
    cd OrdersApp
    ```
2.  **Konfiguracja Połączenia z Pocztą:**
    Upewnij się, że plik konfiguracyjny (np. `appsettings.json` lub zmienne środowiskowe, w zależności od implementacji) zawiera dane logowania do skrzynki e-mail.
    ```json
    // Przykład w appsettings.json (nie zalecane dla danych wrażliwych w produkcji)
    {
      "EmailSettings": {
        "ImapHost": "imap.gmail.com",
        "ImapPort": 993,
        "UseSsl": true,
        "EmailAddress": "twoj_email@gmail.com",
        "EmailPassword": "twoje_haslo_lub_haslo_do_aplikacji"
      },
      "LlmService": {
        "ApiKey": "twoj_klucz_api_llm"
      }
    }
    ```
    *Dla danych wrażliwych (hasła, klucze API) zaleca się użycie Menedżera Tajnych Danych (Secret Manager Tool) podczas developmentu oraz zmiennych środowiskowych lub bezpiecznego systemu konfiguracji w produkcji.*

3.  **Konfiguracja Bazy Danych:**
    Skonfiguruj połączenie do bazy danych w `appsettings.json` lub w kodzie w `Program.cs`/`Startup.cs`. Upewnij się, że masz zainstalowany odpowiedni pakiet NuGet dla swojego providera bazy danych (np. `Pomelo.EntityFrameworkCore.MySql` dla MySQL).
    ```csharp
    // Przykład konfiguracji w Startup.cs/Program.cs
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 21)))); // Przykładowa wersja MySQL
    ```
    Uruchom migracje bazy danych, aby utworzyć schemat:
    ```bash
    dotnet ef database update
    ```

4.  **Konfiguracja LLM:**
    Upewnij się, że `ILlmService` jest prawidłowo skonfigurowany i ma dostęp do klucza API dla wybranego modelu LLM.

## Uruchamianie

Aby uruchomić aplikację:

1.  Przejdź do katalogu głównego projektu `OrdersApp.WebUI` (lub odpowiedniego, zawierającego plik `.csproj` aplikacji uruchomieniowej).
2.  Uruchom aplikację:
    ```bash
    dotnet run
    ```
    Aplikacja rozpocznie proces pobierania i przetwarzania maili zgodnie z wewnętrzną logiką.

## Struktura Projektu

* `OrdersApp.Application/`: Definicje interfejsów, logiki biznesowej i DTO.
* `OrdersApp.Domain/`: Definicje encji (np. `OrderItem`).
* `OrdersApp.Infrastructure/`: Implementacje interfejsów (np. `EmailOrderImporterService`, `OrderRepository`), integracje z zewnętrznymi usługami (LLM, baza danych, MailKit).
* `OrdersApp.WebUI/`: Główna aplikacja uruchomieniowa (np. serwis hostujący `EmailOrderImporterService`).

## Autor

Krystian Gugala

