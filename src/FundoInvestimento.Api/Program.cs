using FundoInvestimento.Api.Extensions;
using FundoInvestimento.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        initializer.Initialize();
    }
}

// Configure the HTTP request pipeline
app.UseMiddlewares();

app.Run();