using CajunLyrics.Lib.Configuration;
using CajunLyrics.Lib.Http;
using CajunLyrics.Lib.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Register HttpClientOptions as configuration management for ILyricsService
builder.Services.Configure<HttpClientOptions>(builder.Configuration.GetSection("HttpClient"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ILyricsService, CajunLyricsService>();
builder.Services.AddScoped<ILyricsClient, CajunLyricsClient>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
