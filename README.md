# Acme Core (test)

.NET 10 core services + Vue admin console.

| Project | Type | Port | Depends on |
|---|---|---|---|
| `src/Acme.Common` | class library | - | - |
| `src/Acme.Customers.Api` | ASP.NET minimal API | 5102 | - |
| `src/Acme.Orders.Api` | ASP.NET controllers | 5101 | customers-api, **pricing-service (test2)**, **inventory-service (test2)**, **notification-service (test3)** |
| `web/acme-admin-web` | Vue 3 + Vite | 5173 | orders-api, customers-api, **inventory-service (test2)**, **report-service (test3)**, **notification-service (test3)** |

Called by other repos:
- `customers-api` <- pricing-service (test2), report-service (test3)
- `orders-api` <- report-service (test3)

## Run

```bash
dotnet run --project src/Acme.Customers.Api
dotnet run --project src/Acme.Orders.Api
cd web/acme-admin-web && npm install && npm run dev
```

Service URLs live in `src/Acme.Orders.Api/appsettings.json` (`ServiceUrls`) and `web/acme-admin-web/vite.config.js`.
