using FundoInvestimento.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddServices();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddlewares();

app.Run();