using System.Text.Json.Serialization;
using API;
using Microsoft.AspNetCore.Mvc;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var options = new DataOptions().UseSQLite(Environment.GetEnvironmentVariable("DB") ??"Data Source=dev.db");
builder.Services.AddSingleton(new DataOptions<GroceryDatabase>(options));
builder.Services.AddScoped<GroceryDatabase>();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApiDocument();

builder.Services.AddCors();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyAwesomeExceptionHandler>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GroceryDatabase>();
    GrocerySeed.EnsureSeeded(db);
}

app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseExceptionHandler();
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();
app.Run();

public class MyAwesomeExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        {
            Title = exception.Message
        });

        return new ValueTask<bool>();
    }
}
