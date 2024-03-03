using Common;
using Common.Events;
using FileServerSlave.EventHandlers;
using FileServerSlave.Events;
using FileServerSlave.Files;
using FileServerSlave.Utils;

namespace FileServerSlave;

public abstract partial class Program
{
    private readonly static CancellationTokenSource cts = new();

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole( o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss:ffff ";
        });

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

        builder.Services.AddSingleton<IFileManager, FileManager>();
        builder.Services.AddSingleton<IHostStringRetriver, HostStringRetriver>();
        builder.Services.AddSingleton<IDestinationLocator, DestinationLocator>();
        builder.Services.AddSingleton<IMasterServerRetriver, MasterServerRetriver>();

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

        if (args.Contains("--save"))
        {
            var saveLocation = args[args.ToList().IndexOf("--save") + 1];
            app.Services.GetRequiredService<IDestinationLocator>().SetCustomLocation(saveLocation);
        }

        if (args.Contains("--master"))
        {
            var masterHostString = args[args.ToList().IndexOf("--master") + 1];
            app.Services.GetRequiredService<IMasterServerRetriver>().SetCustomMasterHostString(new HostString(masterHostString));
        }

        var sm = app.Services.GetRequiredService<ISocketManager>();

        // establish connection only when server started running
        app.Lifetime.ApplicationStarted.Register(() => sm.EstablishConnection(cts.Token));

        app.Lifetime.ApplicationStopping.Register(cts.Cancel);

        app.MapControllers();

        app.Run();
    }
}