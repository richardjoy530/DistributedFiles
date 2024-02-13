using System.Net.WebSockets;
using Backend.Storage;

namespace Backend;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        Container.Files = new Queue<IFormFile>();
        Container.ConnectedSockets = new List<WebSocket>();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        var webSocketOptions = new WebSocketOptions();

        app.UseWebSockets(webSocketOptions);

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}