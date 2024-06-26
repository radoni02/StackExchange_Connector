

using Microsoft.Extensions.Options;
using Serilog;
using Stack.Api.Extensions;
using Stack.Api.Middleware;
using Stack.Application;
using Stack.Application.Extensions;
using Stack.Infrastructure;
using Stack.Infrastructure.SeedData;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<StackExchangeOptions>(builder.Configuration.GetSection(nameof(StackExchangeOptions)));

builder.Services.AddHttpClient<StackExchangeService>((serviceProvider, httpClient) =>
{
    var stackSettings = serviceProvider.GetRequiredService<IOptions<StackExchangeOptions>>().Value;

    httpClient.DefaultRequestHeaders.Add("key", stackSettings.Key);
    httpClient.BaseAddress = new Uri(stackSettings.WebSite);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{   //used to provide safe injection this transient service into singleton
    return new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    };
})
.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});



var app = builder.Build();
app.ApplyMigrations();
app.SeedDataProvider();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseMiddleware<ExceptionHandingMiddleware>();
app.MapControllers();

app.Run();

public partial class Program { }