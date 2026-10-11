using CajunLyrics.Lib.Configuration;
using CajunLyrics.Lib.Http;
using CajunLyrics.Lib.Services;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Allow the React dev server to call the API directly during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevServer", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Register HttpClientOptions as configuration management for ILyricsClient
builder.Services.Configure<HttpClientOptions>(builder.Configuration.GetSection("HttpClient"));

builder.Services.AddHttpClient(nameof(CajunLyricsClient), (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<HttpClientOptions>>().Value;
    client.BaseAddress = options.BaseAddress;
    client.DefaultRequestHeaders.UserAgent.ParseAdd("CajunLyrics.Api/1.0");
});

builder.Services.AddScoped<ILyricsService, CajunLyricsService>();
builder.Services.AddScoped<ILyricsClient, CajunLyricsClient>();

// Probe endpoint for container hosts; kept as a self-check so a flaky
// upstream never causes the platform to restart-loop the app.
builder.Services.AddHealthChecks();

var app = builder.Build();

// Only Development gets HTTP→HTTPS redirection. In every other environment the
// app runs behind a proxy that terminates TLS, and redirecting here would fight it.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CajunLyrics API v1");
    });
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
    app.UseCors("ReactDevServer");
}

app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
