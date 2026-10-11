using System.Collections.Concurrent;
using Acme.Common.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// In-memory store (the "real" one was a SQL Server DB that got decommissioned).
var customers = new ConcurrentDictionary<int, Customer>();
var nextId = 0;
foreach (var c in new[]
{
    new Customer { Name = "Contoso Ltd", Email = "buyer@contoso.example", Tier = CustomerTier.Gold },
    new Customer { Name = "Fabrikam Inc", Email = "orders@fabrikam.example", Tier = CustomerTier.Silver },
    new Customer { Name = "Northwind Traders", Email = "ops@northwind.example", Tier = CustomerTier.Bronze },
})
{
    c.Id = Interlocked.Increment(ref nextId);
    customers[c.Id] = c;
}

app.MapGet("/health", () => Results.Ok(new { service = "customers-api", status = "ok", version = "1.1.0" }));

app.MapGet("/api/customers", () => customers.Values.OrderBy(c => c.Id));

app.MapGet("/api/customers/{id:int}", (int id) =>
    customers.TryGetValue(id, out var c) ? Results.Ok(c) : Results.NotFound());

app.MapPost("/api/customers", (Customer input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { error = "name is required" });
    if (input.Tier is not (CustomerTier.Bronze or CustomerTier.Silver or CustomerTier.Gold))
        return Results.BadRequest(new { error = "tier must be Bronze, Silver or Gold" });

    input.Id = Interlocked.Increment(ref nextId);
    customers[input.Id] = input;
    return Results.Created($"/api/customers/{input.Id}", input);
});

app.Run();
