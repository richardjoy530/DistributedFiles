using Common;
using Common.Events;
using FileServerMaster.EventHandlers;
using FileServerMaster.Events;
using FileServerMaster.Storage;

namespace FileServerMaster;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss:ffff ";
        });

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<IFileContainer, FileContainer>();
        builder.Services.AddSingleton<IWebSocketContainer, WebSocketContainer>();
        builder.Services.AddSingleton<IFileDistributorManager, FileDistributorManager>();
        builder.Services.AddSingleton<IHostStringRetriver, HostStringRetriver>();

        builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
        builder.Services.AddSingleton<IEventHandlerResolver, EventHandlerResolver>();

        builder.Services.AddKeyedSingleton<IEventHandler, RequestCheckInEventHandler>(nameof(RequestCheckInEvent));
        builder.Services.AddKeyedSingleton<IEventHandler, SocketClosedEventHandler>(nameof(SocketClosedEvent));
        builder.Services.AddKeyedSingleton<IEventHandler, DisconnectSlaveEventHandler>(nameof(DisconnectSlaveEvent));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        //else
        //{
        //    app.UseHttpsRedirection();
        //}

        var webSocketOptions = new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromMinutes(5)
        };

        app.UseWebSockets(webSocketOptions);

        app.MapControllers();

        var ed = app.Services.GetRequiredService<IEventDispatcher>();
        app.Lifetime.ApplicationStopping.Register(() => ed.FireEvent(new DisconnectSlaveEvent()));

        app.Run();
    }
}