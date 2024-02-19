using Common;
using FileServerSlave.EventHandlers;
using FileServerSlave.Events;
using FileServerSlave.Files;

namespace FileServerSlave;

public abstract partial class Program
{
    private readonly static CancellationTokenSource cts = new();

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<ISocketManager, SocketManager>();

        builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
        builder.Services.AddSingleton<IEventHandlerResolver, EventHandlerResolver>();

        builder.Services.AddKeyedSingleton<IEventHandler, CheckInEventHandler>(nameof(CheckInEvent));
        builder.Services.AddKeyedSingleton<IEventHandler, DownLoadEventHandler>(nameof(DownloadEvent));

        builder.Services.AddSingleton<IHostStringRetriver, HostStringRetriver>();
        builder.Services.AddSingleton<IFileManager, FileManager>();

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

        var sm = app.Services.GetRequiredService<ISocketManager>();
        sm.EstablishConnection(cts.Token);

        app.MapControllers();

        app.Run();
    }
}