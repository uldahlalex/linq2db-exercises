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

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyCustomExceptionHandler>();

builder.Services.AddOpenApiDocument(settings => settings.SchemaSettings.SchemaProcessors.Add(new RequireNotNullableSchemaProcessor()));


builder.Services.AddCors();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GroceryDatabase>();
    GrocerySeed.EnsureSeeded(db);
}

app.UseExceptionHandler();
app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();
app.Run();

public class MyException : Exception;

public class MyCustomExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, 
        CancellationToken cancellationToken)
    {
     
        if (exception is ValidationException)
        {
            httpContext.Response.StatusCode = 401;
        }
        httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        {
            Title = exception.Message
        });

        return default;
    }
}
