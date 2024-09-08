using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Register HttpClient
builder.Services.AddLogging(); // Register logging

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Method to send contact details to Discord webhook
async Task SendToDiscordWebhook(Contact contact, IHttpClientFactory httpClientFactory, ILogger logger)
{
    var webhookUrl = app.Configuration["DiscordWebhookUrl"];
    if (string.IsNullOrEmpty(webhookUrl))
    {
        throw new InvalidOperationException("Discord webhook URL is not configured.");
    }

    var payload = new
    {
        content = $"Name: {contact.Name}\nEmail: {contact.Email}\nMessage: {contact.Message}"
    };
    var jsonPayload = JsonSerializer.Serialize(payload);
    var httpClient = httpClientFactory.CreateClient();
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    try
    {
        var response = await httpClient.PostAsync(webhookUrl, content);
        response.EnsureSuccessStatusCode();
        logger.LogInformation("Contact info sent to Discord webhook successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error sending contact info to Discord webhook.");
        throw;
    }
}

// POST that takes contact information and sends it to a Discord webhook
app.MapPost("/contact", async (Contact contact, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        // Validate contact fields
        if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Email) || string.IsNullOrWhiteSpace(contact.Message))
        {   
            return Results.BadRequest("Name, Email, and Message are required fields.");
        }

        logger.LogInformation($"Received contact info: Name={contact.Name}, Email={contact.Email}, Message={contact.Message}");
        await SendToDiscordWebhook(contact, httpClientFactory, logger);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to process contact info.");
        return Results.BadRequest("Failed to process contact info.");
    }
});

app.Run();

public record Contact(string Name, string Email, string Message);
