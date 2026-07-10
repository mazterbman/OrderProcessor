using Microsoft.EntityFrameworkCore;
using OrderProcessor.Application;
using OrderProcessor.Grpc;
using OrderProcessor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrderGrpcService>();
app.MapGet("/", () => "gRPC-сервер работает. Общаться со мной нужно через gRPC-клиент.");

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();