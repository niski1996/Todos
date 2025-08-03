# 📝 Lista zadań - Aplikacja webowa

Prosta aplikacja do zarządzania listą zadań na każdy dzień z historią wykonań.

## 🚀 Funkcjonalności

- ✅ Lista zadań powtarzalnych na każdy dzień
- ➕ Dodawanie, edytowanie i usuwanie zadań
- 🔄 Automatyczne resetowanie statusów o północy
- 📊 Historia dziennych wykonań z statystykami
- 📱 Responsywny interfejs (desktop i mobile)
- 🎨 Minimalistyczny design

## 🏗️ Architektura

### Backend (ASP.NET Core Web API)
- **Port**: 7001 (HTTPS), 5001 (HTTP)
- **Technologia**: .NET 8.0
- **Przechowywanie**: Pliki CSV
- **API**: RESTful endpoints

### Frontend (Blazor Server)
- **Port**: 7000 (HTTPS), 5000 (HTTP)  
- **Technologia**: Blazor Server + Bootstrap 5
- **Komunikacja**: HttpClient → Backend API

## 📁 Struktura projektu

```
Todos/
├── src/
│   ├── Backend/TodosApi/          # ASP.NET Core Web API
│   │   ├── Controllers/           # API Controllers
│   │   ├── Models/               # Modele danych
│   │   ├── Services/             # Logika biznesowa
│   │   └── Data/                 # Warstwa dostępu do CSV
│   │
│   ├── Frontend/                 # Blazor Server App
│   │   ├── Components/Pages/     # Strony Blazor
│   │   ├── Models/              # Modele frontendu
│   │   ├── Services/            # Komunikacja z API
│   │   └── wwwroot/             # Pliki statyczne
│   │
│   └── Data/                     # Pliki CSV z danymi
│       ├── tasks.csv            # Lista zadań
│       └── history.csv          # Historia wykonań
├── .vscode/                     # Konfiguracja VS Code
└── README.md                    # Ten plik
```

## 🛠️ Instalacja i uruchomienie

### Wymagania
- .NET 8.0 SDK
- Visual Studio Code (zalecane)

### Kroki instalacji

1. **Sklonuj repozytorium**
   ```bash
   git clone <repository-url>
   cd Todos
   ```

2. **Przywróć zależności**
   ```bash
   dotnet restore
   ```

3. **Uruchom aplikację**

   **Opcja A: Automatyczne uruchomienie ze Swaggerem (VS Code)**
   - Otwórz projekt w VS Code
   - Naciśnij `Ctrl+Shift+P` → `Tasks: Run Task` → `start-all`
   - Swagger UI otworzy się automatycznie w przeglądarce
   
   **Opcja B: Ręczne uruchomienie**
   ```bash
   # Terminal 1 - Backend
   cd src/Backend/TodosApi
   dotnet run
   
   # Terminal 2 - Frontend  
   cd src/Frontend
   dotnet run
   ```

4. **Otwórz aplikację**
   - Frontend: https://localhost:7000
   - Backend API + Swagger: https://localhost:7001 (automatycznie otwiera się Swagger UI)
   - Swagger dokumentacja: https://localhost:7001/swagger

## 📋 API Endpoints

Backend dostarcza RESTful API z pełną dokumentacją Swagger UI.

### 🔍 Swagger UI
- **URL**: https://localhost:7001 (główna strona)
- **Funkcje**: Interaktywna dokumentacja, testowanie endpointów, przykłady
- **Auto-open**: Otwiera się automatycznie przy uruchomieniu backendu

### Zadania
- `GET /api/tasks` - Pobierz wszystkie zadania
- `POST /api/tasks` - Dodaj nowe zadanie
- `PUT /api/tasks/{id}` - Edytuj zadanie
- `DELETE /api/tasks/{id}` - Usuń zadanie
- `PUT /api/tasks/{id}/toggle` - Zmień status zadania

### Historia
- `GET /api/history` - Pobierz historię wykonań
- `GET /api/history/today` - Pobierz dzisiejsze statystyki
- `GET /api/history/statistics` - Pobierz ogólne statystyki

## 🎨 Interfejs użytkownika

### Strona główna (/)
- Lista zadań z checkboxami
- Formularz dodawania nowych zadań
- Pasek postępu wykonania
- Podgląd ostatnich dni

### Historia (/history)
- Tabela z historią dzienną
- Statystyki ogólne
- Wykres trendów (ostatnie 7 dni)

## ⚙️ Konfiguracja

### Backend (appsettings.json)
```json
{
  "DataPath": "../../../Data",
  "Logging": { ... }
}
```

### Frontend (appsettings.json)
```json
{
  "ApiBaseUrl": "https://localhost:7001/",
  "Logging": { ... }
}
```

## 🔄 Automatyczne funkcje

### Reset o północy
- Serwis `DailyResetService` automatycznie resetuje statusy zadań
- Zapisuje statystyki dnia do pliku history.csv
- Działa w tle jako HostedService

### Przechowywanie danych
- **tasks.csv**: Id, Name, CreatedDate, IsCompleted
- **history.csv**: Date, TotalTasks, CompletedTasks

## 🧪 Testowanie

```bash
# Kompilacja całego solution
dotnet build

# Testy jednostkowe (jeśli dodane)
dotnet test
```

## 📱 Responsywność

Aplikacja jest w pełni responsywna i działa na:
- 💻 Desktop (wszystkie główne przeglądarki)
- 📱 Mobile (iOS Safari, Android Chrome)
- 📟 Tablet

## 🚀 Deployment

### Produkcja
1. Skonfiguruj serwer z .NET 8.0
2. Ustaw zmienne środowiskowe
3. Skompiluj w trybie Release
4. Wdróż oba projekty

### Docker (opcjonalnie)
```bash
# Będzie dodane w przyszłości
docker-compose up
```

## 🛠️ Rozwój

### Planowane funkcjonalności
- [ ] Eksport danych do PDF/Excel
- [ ] Motyw ciemny/jasny
- [ ] Kategoryzacja zadań
- [ ] Powiadomienia webowe
- [ ] Synchronizacja między urządzeniami

### Zgłaszanie błędów
Zgłaszaj problemy przez Issues w repozytorium.

---

**Autor**: [Twoje imię]  
**Licencja**: MIT  
**Wersja**: 1.0.0
daily TODO list checker
