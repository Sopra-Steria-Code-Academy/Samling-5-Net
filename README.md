## Oppgaver

### Forarbeid

#### Oppsett
Prosjektet bruker .NET Aspire for orkestrering. Aspire starter RabbitMQ, PostgreSQL og applikasjonen automatisk.

**Prosjektstruktur:**
- `EventDrivenApp` — Web API med publish/subscribe-endepunkter og Swagger UI
- `EventDrivenCommon` — Delt bibliotek med RabbitMQ-konstanter og connection helper
- `EventDrivenDotnet.AppHost` — Aspire-orkestrator som starter RabbitMQ, PostgreSQL og appen
- `EventDrivenDotnet.ServiceDefaults` — Felles oppsett for helsesjekker, OpenTelemetry og service discovery

1. **Installer forutsetninger:**
   - [.NET 10 SDK](https://dotnet.microsoft.com/download)
   - [Docker Desktop](https://www.docker.com/products/docker-desktop/) eller [Podman](https://podman.io/)
   - [Aspire CLI](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/aspire-cli): `dotnet tool install -g aspire`

2. **Start alt med Aspire:**
   ```bash
   aspire start
   ```
   Dette starter RabbitMQ (med management UI), PostgreSQL og EventDrivenApp automatisk.
   - **Aspire Dashboard:** URL vises i terminalen når du kjører `aspire start`
   - **Swagger UI:** Se URL for EventDrivenApp i Aspire Dashboard
   - **RabbitMQ Management:** Se URL i Aspire Dashboard (credentials vises under environment variables for messaging-ressursen)

3. **Alternativt (uten Aspire):**
   ```bash
   docker compose up -d       # Starter RabbitMQ + PostgreSQL
   dotnet run --project EventDrivenApp
   ```
   Når du kjører uten Aspire må du konfigurere connection strings via environment-variabler eller user-secrets:
   ```bash
   # Sett connection strings
   export ConnectionStrings__messaging="amqp://guest:guest@localhost:5672"
   export ConnectionStrings__eventdriven="Host=localhost;Port=5432;Database=eventdriven;Username=postgres;Password=postgres"
   ```

4. Gå til RabbitMQ Management UI og logg deg på (guest/guest ved docker-compose, se Aspire Dashboard for credentials ved Aspire)
5. Gjør deg litt kjent inne i admin-panelet. Hvor ser man connections? Exchanger? Køer?
6. Gjør deg kjent med innholdet i `EventDrivenCommon/RabbitMQConnectionHelper.cs` og `EventDrivenCommon/RabbitMQConst.cs`

#### Arkitektur
Prosjektet har én applikasjon `EventDrivenApp` som eksponerer REST-endepunkter via Swagger UI:
- **POST /api/exchange/declare** — Deklarer en exchange (fanout, direct, topic, headers)
- **POST /api/publish** — Publiser en melding til en exchange
- **POST /api/subscribe** — Opprett en kø, bind den til en exchange, og start konsumering
- **GET /api/messages** — Hent konsumerte meldinger fra databasen
- **DELETE /api/messages** — Slett alle konsumerte meldinger
- **GET /api/subscriptions** — Se aktive subscriptions

Konsumerte meldinger lagres i en PostgreSQL-database, slik at du kan se dem via `GET /api/messages`.

**Tanken er at man kan kjøre flere instanser av applikasjonen** (på ulike porter eller i Docker-containere) — noen som publiserer og noen som konsumerer — for å demonstrere dynamikken i de forskjellige kø-typene.

For å kjøre en ekstra instans:
```bash
dotnet run --project EventDrivenApp --urls http://localhost:5002
```

### FANOUT
1. Bruk Swagger UI til å deklarere en exchange av typen fanout via `POST /api/exchange/declare`
2. Bruk `POST /api/publish` til å publisere meldinger til exchangen. Ser du deg selv under connections i RabbitMQ Management?
3. Kjør en instans av appen som subscriber. Bruk `POST /api/subscribe` til å opprette og binde en kø til exchangen. Se mottatte meldinger med `GET /api/messages`
4. Kjør en annen instans (eller bruk en annen kø-navn). Bruk `POST /api/subscribe` med `autoDelete: true`. Denne skal motta nøyaktig de samme meldingene.
5. Hva skjer når du stopper subscriber-instansen med autoDelete?
6. Hva skjer når du starter den igjen?
7. Hva skjer hvis begge subscribers lytter på samme kø?
8. Slett køene og exchangene dine i RabbitMQ Management UI.

### DIRECT
1. Deklarer en ny exchange av typen direct via `POST /api/exchange/declare`
2. Bruk `POST /api/publish` til å publisere meldinger med en routingKey
3. Opprett en subscriber med `POST /api/subscribe` og en fornuftig routingKey. Mottar du meldingene?
4. Opprett en annen subscriber med en annen routingKey. Mottar du meldingene? Hvorfor ikke?
5. Slett køene og exchangene dine.

### TOPIC
1. Deklarer en ny exchange av typen topic via `POST /api/exchange/declare`
2. Publiser meldinger annenhver gang med routingKey = `soprasteria` og routingKey = `soprasteria.secret`
3. Subscriber 1: Bind en kø med routingKey = `soprasteria.applications`. Hvilke meldinger mottar du?
4. Subscriber 2: Bind en kø med routingKey = `soprasteria.*`. Hvilke meldinger mottar du?
5. Subscriber 3: Bind en kø med routingKey = `soprasteria.secret`. Hvilke meldinger mottar du?
6. Hvilken subscriber mottar meldingene hvis du publiserer med routingKey = `soprasteria.secret.unicorn`?
7. Slett køene og exchangene dine.

### HEADER
1. Deklarer en ny exchange av typen headers via `POST /api/exchange/declare`
2. Publiser meldinger med ulike headere:
   - `"type": "vehicle", "color": "red"`
   - `"type": "vehicle", "color": "blue"`
   - `"type": "bike", "color": "purple"`
3. Subscriber 1: Bind en kø med headers `"type": "vehicle", "color": "red", "x-match": "all"`. Hvilke meldinger mottar du?
4. Subscriber 2: Bind en kø med headers `"type": "vehicle", "color": "purple", "x-match": "any"`. Hvilke meldinger mottar du?
5. Slett køene og exchangene dine.

### Hvis du trenger en utfordring
1. Lag deg et program som publiserer og konsumerer mange hundre meldinger i sekundet
2. Øk prefetch counten til noe stort
3. Hvordan kan du sørge for å disconnecte gracefully når man har satt en høy prefetch count? Hva skjer med meldingene som er prefetched, når consumeren f.eks feiler eller restarter? Er meldingene tapt? Hint: ShutDownSignal
4. Prøv å send en fil over RabbitMQ. Hvordan får man til det?