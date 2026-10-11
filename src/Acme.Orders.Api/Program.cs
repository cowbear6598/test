using Acme.Orders.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<OrderRepository>();
builder.Services.AddSingleton<DownstreamClients>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { service = "orders-api", status = "ok" }));
app.MapControllers();

app.Run();
