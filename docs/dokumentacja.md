# Dokumentacja systemu — Cyfrowe Archiwum Społecznościowe

## Spis treści

1. [Opis projektu](#1-opis-projektu)
2. [Uzasadnienie przyjętych rozwiązań technicznych](#2-uzasadnienie-przyjętych-rozwiązań-technicznych)
3. [Architektura systemu](#3-architektura-systemu)
4. [Architektura informacji](#4-architektura-informacji)
5. [Model danych](#5-model-danych)
6. [Struktura metadanych](#6-struktura-metadanych)
7. [Dokumentacja API](#7-dokumentacja-api)
8. [Analiza dostępności informacji (WCAG 2.1 AA)](#8-analiza-dostępności-informacji-wcag-21-aa)

---

## 1. Opis projektu

Cyfrowe Archiwum Społecznościowe to aplikacja internetowa umożliwiająca lokalnym mieszkańcom i pasjonatom historii gromadzenie, opisywanie i udostępnianie zdjęć dokumentujących historię i ewolucję danego obszaru geograficznego.

Główne założenia systemu:

- **Dostępność** — materiały historyczne mają charakter społecznościowy; interfejs musi być prosty i intuicyjny, by mogli z niego korzystać również starsi użytkownicy
- **Wyszukiwanie kontekstualne** — możliwość wyszukiwania według lokalizacji geograficznej, okresu historycznego oraz hierarchicznej struktury obszarów
- **Trójpoziomowy model ról** — zróżnicowane uprawnienia dla Przeglądającego, Twórcy i Administratora
- **Publiczne API** — otwarte wyszukiwanie bez logowania, prywatne API dla zarządzania treścią

---

## 2. Uzasadnienie przyjętych rozwiązań technicznych

### 2.1 Podział na backend i frontend

System został zbudowany w architekturze SPA (Single Page Application) z oddzielnym backendem REST API. Takie podejście umożliwia:

- niezależne skalowanie warstwy prezentacji i warstwy danych
- udostępnienie tego samego API innym klientom (np. aplikacja mobilna)
- wyraźny kontrakt między warstwami przez typy TypeScript odpowiadające DTOs backendu

### 2.2 ASP.NET Core 8 (backend)

Wybór podyktowany dojrzałością ekosystemu .NET w zakresie:

- **Entity Framework Core** — migracje, silnie typowane zapytania LINQ, automatyczna obsługa relacji wiele-do-wielu (Photo ↔ Tag)
- **JWT Bearer** — bezstanowa autoryzacja, kompatybilna z zewnętrznymi dostawcami tożsamości
- **Npgsql / PostgreSQL** — natywna obsługa tablic, zapytania pełnotekstowe, dojrzały driver dla .NET
- **Swagger/OpenAPI** — automatyczna dokumentacja API dostępna pod `/swagger`

### 2.3 Vue 3 + Vite (frontend)

- **Composition API** — lepsza organizacja logiki przez composables (`usePhotoSearch`, `useAnnouncer`, `usePagination`), niż Options API
- **Pinia** — lekki store z pełnym wsparciem TypeScript; jeden store na domenę (photos, categories, auth, theme, fontsize, toast)
- **Vue Router** — meta-pola `requiresAuth` / `requiresAdmin` obsługiwane przez guards, eliminując konieczność powtarzania walidacji w każdym komponencie
- **Vite** — natychmiastowy HMR, zoptymalizowany build z tree-shakingiem
- **ky** — minimalistyczny klient HTTP oparty na Fetch API, bez zbędnych zależności (vs axios)

### 2.4 PostgreSQL

Wybór relacyjnej bazy danych uzasadniony przez:

- złożone relacje (User → Photo → Category → Tag → Comment)
- konieczność zachowania integralności referencyjnej (DELETE na kategorii z ograniczeniem RESTRICT, DELETE Photo → Cascade na Comments)
- pełnotekstowe wyszukiwanie po polach tekstowych bez dodatkowego silnika
- hierarchiczna struktura kategorii jako samoreferencja w jednej tabeli (self-referencing tree)

### 2.5 Autoryzacja JWT + OAuth

Zrezygnowano z tradycyjnej rejestracji e-mail/hasło na rzecz wyłącznie OAuth (Google, Facebook). Uzasadnienie:

- eliminacja konieczności przechowywania haseł i zarządzania ich bezpieczeństwem (hashing, resetowanie)
- niższy próg wejścia dla użytkownika — jedno kliknięcie zamiast wypełniania formularza
- weryfikacja adresu e-mail jest po stronie dostawcy tożsamości

Przy pierwszym logowaniu przez OAuth system automatycznie tworzy konto z rolą `Creator`.

### 2.6 Zmienna precyzja dat

Materiały historyczne często posiadają nieprecyzyjną datację (wiadomo tylko że zdjęcie pochodzi z lat 60., lub z konkretnego miesiąca). Wprowadzono pole `DatePrecision` z wartościami `Year`, `Month`, `Day`:

- data przechowywana jako string w formacie `YYYY`, `YYYY-MM` lub `YYYY-MM-DD`
- filtrowanie zakresu dat inteligentnie dopełnia datę końcową (np. `"1960"` → `"1960-12-31"`) by objąć cały rok
- interfejs użytkownika (`DatePrecisionPicker`) pozwala wybrać granularność przed wprowadzeniem daty

### 2.7 Trzy motywy wizualne (light / dark / high-contrast)

Wymaganie WCAG 2.1 AA oraz dostępności dla osób słabowidzących zrealizowano przez:

- system CSS design tokens oparty na atrybucie `data-theme` na elemencie `<html>`
- automatyczne wykrywanie preferencji systemowych przez `prefers-color-scheme` i `prefers-contrast`
- persystencja wyboru w `localStorage`
- motyw `high-contrast` używa czarnego tła z akcentem cyjanowym, zapewniając kontrast powyżej 7:1

---

## 3. Architektura systemu

### 3.1 Diagram warstw

```
┌─────────────────────────────────────────────┐
│               Przeglądarka                  │
│  Vue 3 SPA (Vite, Pinia, Vue Router)        │
│  Port: 5173 (dev) / pliki statyczne (prod)  │
└─────────────────────┬───────────────────────┘
                      │ HTTP / REST JSON
                      │ Authorization: Bearer <JWT>
┌─────────────────────▼───────────────────────┐
│           ASP.NET Core 8 Web API            │
│  Port: 5052                                 │
│  /api/photos  /api/categories               │
│  /api/auth    /api/admin                    │
│  /api/photos/{id}/comments                  │
│  Swagger UI: /swagger                       │
│  Pliki statyczne: /uploads/*                │
└─────────────────────┬───────────────────────┘
                      │ EF Core / Npgsql
┌─────────────────────▼───────────────────────┐
│           PostgreSQL                        │
│  Baza: aitsib_db                            │
│  Tabele: Users, Photos, Categories,         │
│          Tags, PhotoTags, Comments          │
└─────────────────────────────────────────────┘
```

### 3.2 Struktura backendu

```
backend/
├── Controllers/
│   ├── AuthController.cs        — logowanie OAuth, /auth/me
│   ├── PhotosController.cs      — CRUD zdjęć, wyszukiwanie, upload
│   ├── CategoriesController.cs  — zarządzanie hierarchią kategorii
│   ├── CommentsController.cs    — komentarze do zdjęć
│   └── AdminController.cs       — panel admina (użytkownicy, statystyki)
├── Models/
│   ├── User.cs                  — enum UserRole, pola OAuth
│   ├── Photo.cs                 — enum DatePrecision, metadane
│   ├── Category.cs              — self-referencing tree
│   ├── Tag.cs                   — tagi (relacja M:N z Photo)
│   └── Comment.cs
├── Data/
│   └── AppDbContext.cs          — konfiguracja EF, relacje, seed admina
├── Migrations/                  — historia schematu bazy
├── Program.cs                   — konfiguracja DI, JWT, CORS, Swagger
└── appsettings.Development.json — connection string, klucz JWT
```

### 3.3 Struktura frontendu

```
frontend/src/
├── pages/                       — widoki (1 plik = 1 strona/trasa)
│   ├── LandingPage.vue
│   ├── BrowsePage.vue           — przeglądanie + filtry
│   ├── PhotoDetailPage.vue      — szczegóły zdjęcia
│   ├── MapBrowserPage.vue       — mapa Leaflet z lokalizacjami
│   ├── UploadPhotoPage.vue
│   ├── EditPhotoPage.vue
│   ├── MyPhotosPage.vue
│   ├── LoginPage.vue
│   ├── AdminDashboardPage.vue
│   ├── AdminUsersPage.vue
│   └── AdminCategoriesPage.vue
├── components/
│   ├── layout/                  — AppHeader, AppNav, AppFooter, MobileMenu
│   ├── photos/                  — PhotoCard, PhotoGrid, PhotoFilters, PhotoSearchBar
│   ├── forms/                   — PhotoUploadForm, ImageDropzone, DatePrecisionPicker, CategorySelect
│   ├── categories/              — CategoryBreadcrumb, CategoryForm
│   ├── admin/                   — AdminPhotoTable, UserTable, BlockUserDialog
│   ├── map/                     — LocationDisplay
│   └── common/                  — AppPagination, AppSpinner, AppToast, AppEmptyState, SettingsDropdown
├── stores/                      — Pinia stores (1 store = 1 domena)
│   ├── auth.store.ts
│   ├── photos.store.ts
│   ├── categories.store.ts
│   ├── theme.store.ts
│   ├── fontsize.store.ts
│   └── toast.store.ts
├── composables/
│   ├── usePhotoSearch.ts        — synchronizacja filtrów z URL
│   ├── useAnnouncer.ts          — ARIA live region dla czytników ekranu
│   ├── usePagination.ts
│   └── useBreakpoint.ts
├── api/                         — klienty HTTP (jeden plik = jeden zasób)
│   ├── client.ts                — instancja ky z baseURL i interceptorem JWT
│   ├── photos.api.ts
│   ├── categories.api.ts
│   ├── auth.api.ts
│   ├── admin.api.ts
│   └── comments.api.ts
├── router/
│   ├── routes.ts                — definicje tras z meta (requiresAuth, requiresAdmin)
│   └── guards.ts                — nawigacyjne guardy autoryzacji
├── types/                       — interfejsy TypeScript (DTOs)
│   ├── photo.ts
│   ├── user.ts
│   ├── category.ts
│   ├── comment.ts
│   └── api.ts
└── i18n/
    └── locales/pl.json          — tłumaczenia (język polski)
```

### 3.4 Przepływ autoryzacji

```
Użytkownik klika "Zaloguj przez Google"
        │
        ▼
Frontend pobiera token od Google (client-side OAuth flow)
        │
        ▼
POST /api/auth/google { token: "<google_id_token>" }
        │
        ▼
Backend weryfikuje token u Google → pobiera email, displayName, avatarUrl
        │
        ├─ Nowy użytkownik → tworzy konto z rolą Creator
        └─ Istniejący użytkownik → pobiera dane z bazy
        │
        ▼
Backend zwraca własny JWT (payload: userId, email, role)
        │
        ▼
Frontend zapisuje JWT w localStorage (auth.store)
        │
        ▼
Każde kolejne żądanie: Authorization: Bearer <JWT>
```

---

## 4. Architektura informacji

### 4.1 Hierarchiczna struktura kategorii

Główną osią organizacji treści jest drzewo kategorii geograficzno-tematycznych. Każda kategoria może mieć dowolną liczbę podkategorii, a głębokość drzewa jest nieograniczona.

```
[Obszar geograficzny — poziom 1]
  └── [Region/Miasto — poziom 2]
        └── [Dzielnica/Miejscowość — poziom 3]
              └── [Ulica/Obiekt — poziom 4]

Przykład:
Małopolska
  ├── Kraków
  │     ├── Stare Miasto
  │     │     ├── Rynek Główny
  │     │     └── ul. Floriańska
  │     └── Kazimierz
  └── Zakopane
        └── Krupówki
```

Relacja w bazie danych: samoreferencja tabeli `Categories` przez pole `ParentId`. Usunięcie kategorii jest zablokowane (`DeleteBehavior.Restrict`) gdy posiada podkategorie lub przypisane zdjęcia.

### 4.2 Ścieżki użytkownika

#### Przeglądający (bez konta)

```
Strona główna (/)
   │
   ├── Przeglądaj archiwum (/przegladaj)
   │     ├── Filtruj po kategorii → /przegladaj/:categoryId
   │     ├── Filtruj po dacie (od–do)
   │     ├── Filtruj po tagu
   │     ├── Szukaj po frazie
   │     └── Zdjęcie → /zdjecie/:id
   │
   └── Mapa (/mapa)
         └── Pin na mapie → /zdjecie/:id
```

#### Twórca (zalogowany)

```
Logowanie (/logowanie)
   │
   ├── Moje zdjęcia (/moje-zdjecia)
   │     ├── Dodaj zdjęcie → /dodaj-zdjecie
   │     └── Edytuj → /edytuj-zdjecie/:id
   │
   └── Panel twórcy (/panel-tworcy)
```

#### Administrator

```
Panel administracyjny (/admin)
   ├── Zarządzaj użytkownikami (/admin/uzytkownicy)
   │     └── Blokuj / odblokuj użytkownika
   └── Zarządzaj kategoriami (/admin/kategorie)
         ├── Dodaj kategorię
         ├── Edytuj kategorię
         └── Usuń kategorię
```

### 4.3 Taksonomia i tagowanie

System stosuje dwie równoległe metody klasyfikacji:

| Metoda | Charakter | Zarządzanie | Przykład |
|--------|-----------|-------------|---------|
| Kategoria hierarchiczna | Geograficzna / tematyczna | Administrator | Kraków > Stare Miasto > Rynek Główny |
| Tagi swobodne | Dowolna klasyfikacja | Twórca | `#architektura`, `#transport`, `#lata60` |

Każde zdjęcie musi należeć do dokładnie jednej kategorii i może mieć dowolną liczbę tagów.

---

## 5. Model danych

### 5.1 Diagram encji (ERD)

```
┌──────────────┐       ┌──────────────────────────────┐
│    User      │       │           Photo              │
├──────────────┤       ├──────────────────────────────┤
│ Id (PK)      │───┐   │ Id (PK)                      │
│ Email        │   └──►│ AuthorId (FK → User)         │
│ DisplayName  │       │ CategoryId (FK → Category)   │
│ Role         │       │ Title                        │
│ AvatarUrl    │       │ Description                  │
│ IsBlocked    │       │ ImagePath                    │
│ BlockReason  │       │ ThumbnailPath                │
│ CreatedAt    │       │ Date                         │
│ GoogleId     │       │ DatePrecision [Year/Month/Day]│
│ FacebookId   │       │ Lat                          │
└──────────────┘       │ Lng                          │
                       │ LocationLabel                │
                       │ Technique                    │
┌──────────────┐       │ InventoryNumber              │
│   Category   │       │ OriginalFormat               │
├──────────────┤       │ License                      │
│ Id (PK)      │◄──────│ Digitization                 │
│ Name         │       │ Quote                        │
│ Description  │       │ CreatedAt                    │
│ ParentId(FK) │───┐   └──────────────┬───────────────┘
└──────┬───────┘   │                  │
       │           │                  │
       └───────────┘ (self-ref)       │
                                      │ M:N
                       ┌──────────────▼───────────────┐
                       │         PhotoTag (tabela      │
                       │         pośrednia EF)         │
                       │ PhotoId (FK)                  │
                       │ TagId (FK)                    │
                       └──────────────┬───────────────┘
                                      │
                       ┌──────────────▼───────────────┐
                       │            Tag               │
                       ├──────────────────────────────┤
                       │ Id (PK)                      │
                       │ Name (UNIQUE)                │
                       └──────────────────────────────┘

┌──────────────────────────────┐
│           Comment            │
├──────────────────────────────┤
│ Id (PK)                      │
│ PhotoId (FK → Photo)         │
│ AuthorId (FK → User)         │
│ Text                         │
│ CreatedAt                    │
│ UpdatedAt                    │
└──────────────────────────────┘
```

### 5.2 Tabela: User

| Kolumna | Typ | Ograniczenia | Opis |
|---------|-----|--------------|------|
| `Id` | int | PK, auto | Klucz główny |
| `Email` | varchar | NOT NULL, UNIQUE | Adres e-mail z dostawcy OAuth |
| `DisplayName` | varchar | NOT NULL | Nazwa wyświetlana |
| `Role` | varchar | NOT NULL, default 'Viewer' | Rola: `Viewer`, `Creator`, `Admin` |
| `AvatarUrl` | varchar | NULL | URL avatara z OAuth |
| `IsBlocked` | bool | NOT NULL, default false | Czy konto zablokowane |
| `BlockReason` | varchar | NULL | Powód blokady (uzupełniany przez admina) |
| `CreatedAt` | timestamp | NOT NULL | Data rejestracji (UTC) |
| `GoogleId` | varchar | NULL | Identyfikator konta Google |
| `FacebookId` | varchar | NULL | Identyfikator konta Facebook |

Uwaga: Pole `Role` przechowywane jako string (`HasConversion<string>()`), co ułatwia odczyt bezpośredni w SQL i JWT claims.

### 5.3 Tabela: Photo

| Kolumna | Typ | Ograniczenia | Opis |
|---------|-----|--------------|------|
| `Id` | int | PK, auto | Klucz główny |
| `Title` | varchar | NOT NULL | Tytuł zdjęcia |
| `Description` | text | NULL | Opis tekstowy |
| `ImagePath` | varchar | NOT NULL | Ścieżka do pliku (np. `/uploads/uuid.jpg`) |
| `ThumbnailPath` | varchar | NOT NULL | Ścieżka do miniatury |
| `Lat` | double | NULL | Szerokość geograficzna |
| `Lng` | double | NULL | Długość geograficzna |
| `LocationLabel` | varchar | NULL | Tekstowy opis lokalizacji |
| `Date` | varchar | NOT NULL | Data w formacie `YYYY`, `YYYY-MM` lub `YYYY-MM-DD` |
| `DatePrecision` | varchar | NOT NULL | Granularność daty: `Year`, `Month`, `Day` |
| `Technique` | varchar | NULL | Technika wykonania (np. "Sepia", "Srebro") |
| `InventoryNumber` | varchar | NULL | Numer inwentarzowy |
| `OriginalFormat` | varchar | NULL | Format oryginału (np. "13×18 cm") |
| `License` | varchar | NULL | Licencja (np. "CC BY-NC 4.0") |
| `Digitization` | varchar | NULL | Informacje o digitalizacji (np. "600 DPI, TIFF") |
| `Quote` | text | NULL | Cytat, wspomnienie lub anegdota |
| `CreatedAt` | timestamp | NOT NULL | Data dodania do systemu (UTC) |
| `AuthorId` | int | FK → User, NOT NULL | Autor materiału |
| `CategoryId` | int | FK → Category, NOT NULL | Kategoria hierarchiczna |

### 5.4 Tabela: Category

| Kolumna | Typ | Ograniczenia | Opis |
|---------|-----|--------------|------|
| `Id` | int | PK, auto | Klucz główny |
| `Name` | varchar | NOT NULL | Nazwa kategorii |
| `Description` | text | NULL | Opis kategorii |
| `ParentId` | int | FK → Category, NULL | Kategoria nadrzędna (NULL = korzeń) |

Relacja samoreferencyjana z `DeleteBehavior.Restrict` — nie można usunąć kategorii, która ma podkategorie lub przypisane zdjęcia.

### 5.5 Tabela: Tag

| Kolumna | Typ | Ograniczenia | Opis |
|---------|-----|--------------|------|
| `Id` | int | PK, auto | Klucz główny |
| `Name` | varchar | NOT NULL, UNIQUE | Nazwa tagu (unikalna w systemie) |

Relacja Photo ↔ Tag jest wielokrotna (M:N). Entity Framework Core automatycznie zarządza tabelą pośrednią `PhotoTag`.

### 5.6 Tabela: Comment

| Kolumna | Typ | Ograniczenia | Opis |
|---------|-----|--------------|------|
| `Id` | int | PK, auto | Klucz główny |
| `Text` | text | NOT NULL | Treść komentarza |
| `CreatedAt` | timestamp | NOT NULL | Data dodania (UTC) |
| `UpdatedAt` | timestamp | NULL | Data ostatniej edycji (UTC) |
| `PhotoId` | int | FK → Photo, CASCADE | Powiązane zdjęcie |
| `AuthorId` | int | FK → User, CASCADE | Autor komentarza |

Cascade delete: usunięcie zdjęcia lub użytkownika automatycznie usuwa powiązane komentarze.

---

## 6. Struktura metadanych

### 6.1 Klasyfikacja metadanych

Metadane zdjęcia podzielono na cztery grupy funkcjonalne:

#### Grupa 1: Identyfikacyjne (wymagane)

| Pole | Opis | Przykład |
|------|------|---------|
| `Title` | Tytuł zdjęcia | "Rynek Główny w Krakowie, 1965" |
| `CategoryId` | Przypisanie do hierarchii | Kraków > Stare Miasto > Rynek Główny |
| `Date` | Data wykonania zdjęcia | `1965`, `1965-06`, `1965-06-15` |
| `DatePrecision` | Granularność daty | `Year` / `Month` / `Day` |

#### Grupa 2: Lokalizacyjne

| Pole | Opis | Przykład |
|------|------|---------|
| `Lat` | Szerokość geograficzna (WGS84) | `50.0614` |
| `Lng` | Długość geograficzna (WGS84) | `19.9372` |
| `LocationLabel` | Czytelny opis miejsca | "Kraków, Rynek Główny, narożnik przy ul. Siennej" |

Współrzędne umożliwiają wyświetlenie na mapie (Leaflet) i filtrowanie przestrzenne. `LocationLabel` przeznaczony jest dla osób, które nie znają współrzędnych — może być bardziej opisowy niż sama kategoria.

#### Grupa 3: Opisowe

| Pole | Opis | Przykład |
|------|------|---------|
| `Description` | Rozbudowany opis tekstowy | "Widok na fontannę z Neptunem przed renowacją..." |
| `Quote` | Cytat, wspomnienie, anegdota | "Mój dziadek pracował w tej kamienicy..." |
| `Tags` | Swobodne słowa kluczowe | `architektura`, `fontanna`, `lata-60` |

#### Grupa 4: Techniczne / archiwistyczne

| Pole | Opis | Przykład |
|------|------|---------|
| `Technique` | Technika fotograficzna | "Odbitka srebrowo-żelatynowa", "Slajd Kodachrome" |
| `OriginalFormat` | Format fizycznego oryginału | "13×18 cm", "35 mm klisza" |
| `InventoryNumber` | Numer w archiwum / kolekcji | "HA/2023/K-102" |
| `License` | Licencja praw autorskich | "CC BY-NC 4.0", "Wszystkie prawa zastrzeżone" |
| `Digitization` | Parametry digitalizacji | "600 DPI, TIFF 16-bit, skaner drum" |

### 6.2 Uzasadnienie struktury metadanych

**Zmienna precyzja daty** (`DatePrecision`) jest kluczową decyzją projektową. Materiały historyczne rzadko posiadają dokładną datację. Przechowywanie daty jako stringa (`YYYY`, `YYYY-MM`, `YYYY-MM-DD`) zamiast `DateTime` pozwala:

- na zapis niepełnej daty bez sztucznych wartości domyślnych (np. `1965-01-01`)
- na inteligentne filtrowanie zakresów (np. zapytanie `dateTo=1970` zwróci wszystkie zdjęcia z lat 60.)
- na czytelne wyświetlanie w UI zgodne z tym, co faktycznie wiadomo o zdjęciu

**Tagi swobodne** uzupełniają sztywną hierarchię kategorii. Pozwalają na przekrojowe wyszukiwanie (np. wszystkie zdjęcia z tagiem `tramwaj` niezależnie od lokalizacji) i dają twórcom elastyczność w opisywaniu treści.

**Metadane archiwistyczne** (InventoryNumber, Technique, OriginalFormat, Digitization) są opcjonalne, ale niezbędne dla materiałów pochodzących z formalnych kolekcji. Umożliwiają zachowanie ciągłości z papierową dokumentacją archiwów i bibliotek.

### 6.3 Wyszukiwanie i indeksowanie

| Parametr wyszukiwania | Pola bazy danych | Typ dopasowania |
|-----------------------|------------------|-----------------|
| `q` (fraza) | `Title`, `Description`, `LocationLabel` | LIKE (contains) |
| `categoryId` | `CategoryId` | Równość |
| `tag` | `Tags.Name` | Równość (relacja M:N) |
| `dateFrom` / `dateTo` | `Date` (string) | Porównanie leksykograficzne |
| `sortBy=date` | `Date` | Porządkowanie leksykograficzne |
| `sortBy=createdAt` | `CreatedAt` | Porządkowanie chronologiczne |

---

## 7. Dokumentacja API

Baza URL: `http://localhost:5052/api` (development).

### Legenda dostępu

| Symbol | Znaczenie |
|--------|-----------|
| 🟢 **PUBLICZNY** | Dostęp bez logowania — część publicznego API |
| 🔒 **TWÓRCA** | Wymaga JWT (`Authorization: Bearer <token>`) |
| 🔴 **ADMIN** | Wymaga JWT z rolą `admin` |

---

### 7.1 Autentykacja (`/api/auth`)

| Metoda | Endpoint | Dostęp | Opis |
|--------|----------|--------|------|
| `POST` | `/api/auth/google` | 🟢 **PUBLICZNY** | Logowanie / rejestracja przez Google OAuth. Body: `{ "credential": "<google_id_token>" }`. Zwraca JWT + dane użytkownika. |
| `POST` | `/api/auth/facebook` | 🟢 **PUBLICZNY** | Logowanie / rejestracja przez Facebook. Body: `{ "accessToken": "<fb_access_token>" }`. Zwraca JWT + dane użytkownika. |
| `GET` | `/api/auth/me` | 🔒 **TWÓRCA** | Dane zalogowanego użytkownika (id, displayName, avatarUrl, role). |

---

### 7.2 Zdjęcia (`/api/photos`)

| Metoda | Endpoint | Dostęp | Opis |
|--------|----------|--------|------|
| `GET` | `/api/photos` | 🟢 **PUBLICZNY** | Wyszukiwanie i paginacja zdjęć. Parametry query opisane poniżej. |
| `GET` | `/api/photos/{id}` | 🟢 **PUBLICZNY** | Szczegóły pojedynczego zdjęcia — pełne metadane, tagi, autor, kategoria. |
| `GET` | `/api/photos/my` | 🔒 **TWÓRCA** | Lista własnych zdjęć zalogowanego użytkownika (z paginacją). |
| `POST` | `/api/photos` | 🔒 **TWÓRCA** | Upload nowego zdjęcia. Body: `multipart/form-data`. Pola opisane poniżej. |
| `PUT` | `/api/photos/{id}` | 🔒 **TWÓRCA / ADMIN** | Edycja metadanych zdjęcia. Autor może edytować tylko swoje zdjęcia; admin — dowolne. Body: `application/json`. |
| `DELETE` | `/api/photos/{id}` | 🔒 **TWÓRCA / ADMIN** | Usunięcie zdjęcia wraz z plikiem z dysku. Autor może usuwać tylko swoje; admin — dowolne. |

#### Parametry `GET /api/photos`

| Parametr | Typ | Opis |
|----------|-----|------|
| `q` | string | Wyszukiwanie pełnotekstowe po polach `Title`, `Description`, `LocationLabel` |
| `categoryId` | int | Filtrowanie po kategorii (tylko dokładna kategoria, bez podkategorii) |
| `tag` | string | Filtrowanie po nazwie tagu (np. `?tag=architektura`) |
| `dateFrom` | string | Dolna granica daty — format `YYYY`, `YYYY-MM` lub `YYYY-MM-DD` |
| `dateTo` | string | Górna granica daty — format `YYYY`, `YYYY-MM` lub `YYYY-MM-DD`; rok dopełniany do `YYYY-12-31` |
| `sortBy` | string | Pole sortowania: `date` \| `createdAt` (domyślnie: `createdAt`) |
| `sortDir` | string | Kierunek: `asc` \| `desc` (domyślnie: `desc`) |
| `page` | int | Numer strony (domyślnie: `1`) |
| `pageSize` | int | Wyników na stronę (domyślnie: `12`) |

#### Body `POST /api/photos` (`multipart/form-data`)

| Pole | Wymagane | Opis |
|------|----------|------|
| `file` | ✅ | Plik obrazu (jpg, png, webp itp.) |
| `title` | ✅ | Tytuł zdjęcia (max 200 znaków) |
| `categoryId` | ✅ | ID kategorii hierarchicznej |
| `date` | ✅ | Data: `YYYY`, `YYYY-MM` lub `YYYY-MM-DD` |
| `description` | — | Opis tekstowy (max 2000 znaków) |
| `lat` | — | Szerokość geograficzna |
| `lng` | — | Długość geograficzna |
| `locationLabel` | — | Czytelny opis lokalizacji |
| `technique` | — | Technika fotograficzna |
| `quote` | — | Cytat / wspomnienie (max 1000 znaków) |
| `inventoryNumber` | — | Numer inwentarzowy |
| `originalFormat` | — | Format oryginału |
| `license` | — | Licencja |
| `digitization` | — | Informacje o digitalizacji |
| `tags` | — | Tagi rozdzielone przecinkami (np. `architektura, transport`) |

#### Body `PUT /api/photos/{id}` (`application/json`)

Wszystkie pola opcjonalne (semantyka PATCH — przesyłane są tylko zmieniające się wartości):

```json
{
  "title": "Nowy tytuł",
  "description": "Opis...",
  "categoryId": 3,
  "date": "1965-06",
  "lat": 50.0614,
  "lng": 19.9372,
  "locationLabel": "Kraków, Rynek Główny",
  "technique": "Sepia",
  "quote": "Wspomnienie...",
  "inventoryNumber": "HA/2023/K-102",
  "originalFormat": "13x18 cm",
  "license": "CC BY-NC 4.0",
  "digitization": "600 DPI",
  "tags": "architektura, lata60"
}
```

---

### 7.3 Kategorie (`/api/categories`)

| Metoda | Endpoint | Dostęp | Opis |
|--------|----------|--------|------|
| `GET` | `/api/categories` | 🟢 **PUBLICZNY** | Płaska lista wszystkich kategorii (z polami `id`, `name`, `description`, `parentId`). |
| `GET` | `/api/categories/{id}` | 🟢 **PUBLICZNY** | Szczegóły pojedynczej kategorii. |
| `POST` | `/api/categories` | 🔴 **ADMIN** | Utworzenie nowej kategorii. Body: `{ "name": "...", "parentId": 2, "description": "..." }`. `parentId` = null dla kategorii korzenia. |
| `PUT` | `/api/categories/{id}` | 🔴 **ADMIN** | Edycja kategorii. Body jak przy POST. |
| `DELETE` | `/api/categories/{id}` | 🔴 **ADMIN** | Usunięcie kategorii. Zablokowane jeśli posiada podkategorie lub przypisane zdjęcia (`RESTRICT`). |

---

### 7.4 Komentarze (`/api/photos/{photoId}/comments`, `/api/comments`)

| Metoda | Endpoint | Dostęp | Opis |
|--------|----------|--------|------|
| `GET` | `/api/photos/{photoId}/comments` | 🟢 **PUBLICZNY** | Lista komentarzy do zdjęcia, posortowana chronologicznie. |
| `POST` | `/api/photos/{photoId}/comments` | 🔒 **TWÓRCA** | Dodanie komentarza. Body: `{ "text": "..." }`. |
| `PUT` | `/api/comments/{id}` | 🔒 **TWÓRCA / ADMIN** | Edycja komentarza. Autor może edytować tylko swój; admin — dowolny. Body: `{ "text": "..." }`. |
| `DELETE` | `/api/comments/{id}` | 🔒 **TWÓRCA / ADMIN** | Usunięcie komentarza. Autor — tylko swój; admin — dowolny. |

---

### 7.5 Administracja (`/api/admin`)

Wszystkie endpointy wymagają roli `admin`.

| Metoda | Endpoint | Dostęp | Opis |
|--------|----------|--------|------|
| `GET` | `/api/admin/users` | 🔴 **ADMIN** | Paginowana lista użytkowników z polami: id, email, displayName, role, isBlocked, blockReason, createdAt. Parametry: `page`, `pageSize` (domyślnie 20). |
| `PUT` | `/api/admin/users/{id}/block` | 🔴 **ADMIN** | Zablokowanie użytkownika. Body: `{ "reason": "Powód blokady" }`. |
| `PUT` | `/api/admin/users/{id}/unblock` | 🔴 **ADMIN** | Odblokowanie użytkownika. |
| `GET` | `/api/admin/stats` | 🔴 **ADMIN** | Statystyki systemu: `totalPhotos`, `totalUsers`, `blockedUsers`. |

---

### 7.6 Przykładowe odpowiedzi

#### `GET /api/photos` (odpowiedź stronicowana)

```json
{
  "items": [
    {
      "id": 42,
      "title": "Rynek Główny, 1965",
      "description": "Widok na fontannę z Neptunem...",
      "imagePath": "/uploads/abc123.jpg",
      "thumbnailPath": "/uploads/abc123.jpg",
      "lat": 50.0614,
      "lng": 19.9372,
      "locationLabel": "Kraków, Rynek Główny",
      "date": "1965",
      "datePrecision": "year",
      "technique": null,
      "inventoryNumber": null,
      "originalFormat": null,
      "license": null,
      "digitization": null,
      "quote": null,
      "createdAt": "2026-03-25T13:32:00Z",
      "authorId": 5,
      "categoryId": 3,
      "author": { "id": 5, "displayName": "Jan Kowalski" },
      "category": { "id": 3, "name": "Stare Miasto" },
      "tags": [{ "id": 1, "name": "architektura" }]
    }
  ],
  "page": 1,
  "pageSize": 12,
  "totalCount": 87,
  "totalPages": 8
}
```

#### `POST /api/auth/google` (odpowiedź)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 5,
    "displayName": "Jan Kowalski",
    "avatarUrl": "https://lh3.googleusercontent.com/...",
    "role": "Creator",
    "googleId": "1234567890",
    "facebookId": null
  }
}
```

---

### 7.7 Autoryzacja — szczegóły

Token JWT generowany jest z następującym payloadem:

```json
{
  "nameid": "5",
  "unique_name": "Jan Kowalski",
  "role": "creator",
  "exp": 1753000000
}
```

- Ważność tokenu: **30 dni**
- Algorytm podpisu: **HMAC-SHA256**
- Klucz konfigurowany przez `appsettings.Development.json` → `Jwt:Key`
- Token przekazywany w nagłówku: `Authorization: Bearer <token>`

---

### 7.8 Podsumowanie — zestawienie wszystkich endpointów

| Metoda | Endpoint | 🟢 Pub | 🔒 Twórca | 🔴 Admin |
|--------|----------|:------:|:---------:|:--------:|
| POST | `/api/auth/google` | ✓ | | |
| POST | `/api/auth/facebook` | ✓ | | |
| GET | `/api/auth/me` | | ✓ | |
| GET | `/api/photos` | ✓ | | |
| GET | `/api/photos/{id}` | ✓ | | |
| GET | `/api/photos/my` | | ✓ | |
| POST | `/api/photos` | | ✓ | |
| PUT | `/api/photos/{id}` | | ✓ (własne) | ✓ (wszystkie) |
| DELETE | `/api/photos/{id}` | | ✓ (własne) | ✓ (wszystkie) |
| GET | `/api/categories` | ✓ | | |
| GET | `/api/categories/{id}` | ✓ | | |
| POST | `/api/categories` | | | ✓ |
| PUT | `/api/categories/{id}` | | | ✓ |
| DELETE | `/api/categories/{id}` | | | ✓ |
| GET | `/api/photos/{photoId}/comments` | ✓ | | |
| POST | `/api/photos/{photoId}/comments` | | ✓ | |
| PUT | `/api/comments/{id}` | | ✓ (własne) | ✓ (wszystkie) |
| DELETE | `/api/comments/{id}` | | ✓ (własne) | ✓ (wszystkie) |
| GET | `/api/admin/users` | | | ✓ |
| PUT | `/api/admin/users/{id}/block` | | | ✓ |
| PUT | `/api/admin/users/{id}/unblock` | | | ✓ |
| GET | `/api/admin/stats` | | | ✓ |

**Łącznie: 22 endpointy — 7 publicznych, 6 wyłącznie twórcy, 9 wyłącznie admina.**

---

## 8. Analiza dostępności informacji (WCAG 2.1 AA)

### 7.1 Przegląd wymagania

Standard WCAG 2.1 poziom AA obejmuje cztery zasady: **Postrzegalność**, **Funkcjonalność**, **Zrozumiałość**, **Solidność** (POUR). Poniżej opisano sposób realizacji każdej z nich w systemie.

### 7.2 Postrzegalność (Perceivable)

#### Kryterium 1.1.1 — Treść nietekstowa (poziom A)

Wszystkie zdjęcia posiadają obowiązkowy atrybut `alt` ustawiony na tytuł zdjęcia. Avatary użytkowników posiadają `alt` z imieniem i rolą. Ikony dekoracyjne oznaczone są `aria-hidden="true"`.

```html
<img :src="photo.thumbnailUrl" :alt="photo.title" />
<img :src="user.avatarUrl" :alt="`${user.displayName}, ${user.role}`" aria-hidden="false" />
<span class="material-symbols-outlined" aria-hidden="true">delete</span>
```

#### Kryterium 1.3.1 — Informacje i relacje (poziom A)

Formularze używają jawnych elementów `<label for="...">` powiązanych z polami przez atrybut `id`. Wszystkie pola mają widoczne etykiety — nie polegają wyłącznie na `placeholder`.

Przykład z `PhotoUploadForm.vue`:
```html
<label for="pu-title">Tytuł zdjęcia *</label>
<input id="pu-title" v-model="form.title" type="text" required />
```

Strona używa semantycznego HTML5: `<nav>`, `<main>`, `<header>`, `<footer>`, `<aside>`, `<section>`, `<article>`. Paginacja i filtry otoczone są elementem `<nav aria-label="...">`.

#### Kryterium 1.4.3 — Kontrast (poziom AA)

Trzy motywy zapewniają różne poziomy kontrastu:

| Motyw | Tekst | Tło | Współczynnik |
|-------|-------|-----|-------------|
| `light` | `#1c1c17` | `#fcf9f1` | ~17:1 ✓ |
| `dark` | `#e8e5d8` | `#1a1a14` | ~14:1 ✓ |
| `high-contrast` | `#00ffff` | `#000000` | 21:1 ✓ |

#### Kryterium 1.4.4 — Zmiana rozmiaru tekstu (poziom AA)

System udostępnia trzy rozmiary czcionki: `small`, `normal`, `large` (przełączane przez `SettingsDropdown`). Typografia zbudowana jest na CSS `clamp()` i zmiennych CSS, dzięki czemu skaluje się płynnie. Przechowywanie wyboru w `localStorage` zapewnia trwałość między sesjami.

#### Kryterium 1.4.13 — Zawartość przy najechaniu / fokusie (poziom AA)

Tostowe powiadomienia (`AppToast`) wyświetlają się czasowo i nie blokują interakcji. Tooltip-y nie są używane.

### 7.3 Funkcjonalność (Operable)

#### Kryterium 2.1.1 — Klawiatura (poziom A)

Wszystkie interaktywne elementy są dostępne przez klawiaturę:

- fokus klawiatury widoczny przez `box-shadow: var(--focus-ring)` na wszystkich elementach `focus-visible`
- `tabindex="-1"` na `<main id="main-content">` umożliwia programatyczne przeniesienie fokusu (np. po nawigacji)
- menu mobilne zarządza `tabindex` elementów wewnątrz (`-1` gdy zamknięte, `0` gdy otwarte)
- dialogi (`BlockUserDialog`) ustawiają `aria-modal="true"` i pułapkują fokus wewnątrz

#### Kryterium 2.4.1 — Pominięcie bloków (poziom A)

Skip-link „Przejdź do treści" dostępny jako pierwszy element na stronie (`App.vue`). Widoczny tylko przy fokusie klawiatury (CSS `top: -100%` → `top: 8px` przy `:focus`):

```html
<a href="#main-content" class="skip-link">Przejdź do treści</a>
```

#### Kryterium 2.4.4 — Cel linku (poziom A)

Linki paginacji mają `aria-label` z numerem strony. Linki nawigacji mają `aria-current="page"` na aktywnej pozycji.

#### Kryterium 2.4.6 — Nagłówki i etykiety (poziom AA)

Każda strona posiada jeden nagłówek `<h1>`. Hierarchia nagłówków `h1` → `h2` → `h3` jest zachowana. Sekcje filtrów i wyników posiadają nagłówki.

#### Kryterium 2.3.3 — Animacja z interakcji (poziom AAA, zrealizowane)

Reguła CSS `@media (prefers-reduced-motion: reduce)` wyłącza animacje przejść między stronami i `scroll-behavior: smooth`.

### 7.4 Zrozumiałość (Understandable)

#### Kryterium 3.1.1 — Język strony (poziom A)

Atrybut `lang="pl"` na elemencie `<html>` ustawiany przez Vue i18n. Cały interfejs jest w języku polskim.

#### Kryterium 3.3.1 — Identyfikacja błędu (poziom A)

Błędy walidacji formularzy wyświetlane są w elemencie `<p role="alert">` bezpośrednio pod polem, którego dotyczą. Toast z komunikatem błędu posiada `aria-live="assertive"`.

#### Kryterium 3.3.2 — Etykiety lub instrukcje (poziom A)

Każde pole formularza posiada widoczną etykietę. Pola opcjonalne i wymagane są oznaczone gwiazdką `*` z wyjaśnieniem w kontekście formularza. Pole tagów zawiera wskazówkę „Oddziel tagi przecinkami".

### 7.5 Solidność (Robust)

#### Kryterium 4.1.2 — Nazwa, rola, wartość (poziom A)

Komponenty interaktywne używają natywnych elementów HTML (`<button>`, `<input>`, `<select>`) zamiast niestandardowych. Przyciski przełączające (np. chip filtru kategorii) używają `aria-pressed`:

```html
<button :aria-pressed="categoryId === cat.id">{{ cat.name }}</button>
```

#### Kryterium 4.1.3 — Komunikaty o stanie (poziom AA)

Composable `useAnnouncer` zarządza regionem ARIA live (`aria-live="polite"` / `"assertive"`). Zmiany liczby wyników wyszukiwania, wysyłka formularzy i komunikaty o błędach są ogłaszane przez czytniki ekranu bez przenoszenia fokusu.

```html
<!-- App.vue -->
<div class="sr-only" :aria-live="politeness" aria-atomic="true" role="status">
  {{ announcement }}
</div>
```

### 7.6 Zestawienie kryteriów WCAG 2.1 AA

| Kryterium | Opis | Status |
|-----------|------|--------|
| 1.1.1 | Treść nietekstowa (alt) | ✓ Zrealizowane |
| 1.3.1 | Informacje i relacje (semantyka HTML, label) | ✓ Zrealizowane |
| 1.3.4 | Orientacja | ✓ Brak blokady orientacji |
| 1.4.1 | Użycie koloru | ✓ Informacje nie są przekazywane wyłącznie kolorem |
| 1.4.3 | Kontrast (tekst) | ✓ Zrealizowane (trzy motywy) |
| 1.4.4 | Zmiana rozmiaru tekstu | ✓ Trzy rozmiary czcionki |
| 1.4.10 | Przepływ tekstu (reflow) | ✓ Responsywny layout |
| 1.4.11 | Kontrast elementów nietkestowych | ✓ Kontrolki mają widoczny kontrast |
| 1.4.12 | Odstępy w tekście | ✓ Brak CSS blokującego spacing |
| 2.1.1 | Klawiatura | ✓ Zrealizowane |
| 2.1.2 | Brak pułapki klawiaturowej | ✓ Dialogi zwalniają fokus po zamknięciu |
| 2.4.1 | Pomijanie bloków (skip-link) | ✓ Zrealizowane |
| 2.4.3 | Kolejność fokusu | ✓ Logiczna kolejność DOM |
| 2.4.4 | Cel linku | ✓ Zrealizowane |
| 2.4.6 | Nagłówki i etykiety | ✓ Zrealizowane |
| 2.4.7 | Widoczny fokus | ✓ Zrealizowane (focus-ring) |
| 3.1.1 | Język strony | ✓ `lang="pl"` |
| 3.2.1 | Po fokusie | ✓ Brak automatycznych zmian kontekstu |
| 3.3.1 | Identyfikacja błędu | ✓ `role="alert"` |
| 3.3.2 | Etykiety lub instrukcje | ✓ Zrealizowane |
| 4.1.1 | Parsowanie | ✓ Poprawny HTML |
| 4.1.2 | Nazwa, rola, wartość | ✓ Natywne elementy + ARIA |
| 4.1.3 | Komunikaty o stanie | ✓ `useAnnouncer` + aria-live |

### 7.7 Dostosowanie wyglądu

System implementuje pełną trójkę wariantów wyglądu wymaganych specyfikacją:

| Motyw | Trigger | Przeznaczenie |
|-------|---------|---------------|
| `light` | Domyślny lub `prefers-color-scheme: light` | Standardowe użytkowanie |
| `dark` | `prefers-color-scheme: dark` lub ręczny wybór | Komfort w słabym oświetleniu |
| `high-contrast` | `prefers-contrast: more` lub ręczny wybór | Osoby słabowidzące |

Wybór persystowany w `localStorage` (klucz `cas_theme`). Zmiana motywu następuje natychmiastowo przez atrybut `data-theme` na `<html>` bez przeładowania strony.

Skala czcionki persystowana osobno (klucz `cas_fontsize`): `small` (0.875rem base), `normal` (1rem base), `large` (1.125rem base).
