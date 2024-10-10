using Microsoft.EntityFrameworkCore;
using WeatherNewsAPI.Data;
using WeatherNewsAPI.Services;
using System.Net.Http.Headers;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure HttpClient for WeatherService
        builder.Services.AddHttpClient<WeatherService>(client =>
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/weather");
        });

        // Configure HttpClient for NewsService
         builder.Services.AddHttpClient<NewsService>(client =>
        {
            client.BaseAddress = new Uri("https://newsapi.org/v2/everything");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherNewsAPI/1.0");
        });

        // Register services with ILogger injection
        builder.Services.AddScoped<WeatherService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(WeatherService));
            var configuration = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<WeatherService>>();
            return new WeatherService(httpClient, configuration, logger);
        });

        builder.Services.AddScoped<NewsService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NewsService));
            var configuration = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<NewsService>>();
            return new NewsService(httpClient, configuration, logger);
        });

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        builder.Services.AddCors(options =>
        {
             options.AddPolicy("AllowReactApp",
                builder => builder
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        // Use CORS before other middleware
        app.UseCors("AllowReactApp");

        app.Use(async (context, next) =>
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:3000";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Origin, X-Requested-With, Content-Type, Accept";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                await context.Response.CompleteAsync();
            }
            else
            {
                await next();
            }
        });

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}