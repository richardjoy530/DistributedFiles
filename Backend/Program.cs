using System.Net.WebSockets;
using Backend.Storage;

namespace Backend;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<IFileContainer, FileContainer>();
        builder.Services.AddSingleton<IWebSocketContainer, WebSocketContainer>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var webSocketOptions = new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromMinutes(5)
        };

        app.UseWebSockets(webSocketOptions);

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}