using FileServerSlave.EventHandlers;
using FileServerSlave.Events;
using System.Runtime.InteropServices;

namespace FileServerSlave;

public abstract partial class Program
{
    private readonly static CancellationTokenSource cts = new();

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<ISocketManager, SocketManager>();
        builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
        builder.Services.AddSingleton<IEventHandlerResolver, EventHandlerResolver>();
        builder.Services.AddKeyedSingleton<IEventHandler, CheckInEventHandler>(nameof(CheckInEvent));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var sm = app.Services.GetRequiredService<ISocketManager>();
        sm.EstablishConnection(cts.Token);
        SetConsoleCtrlHandler(Handler, true);

        app.Run();
    }

    [LibraryImport("Kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, [MarshalAs(UnmanagedType.Bool)] bool add);

    private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

    private enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    private static bool Handler(CtrlType signal)
    {
        switch (signal)
        {
            case CtrlType.CTRL_BREAK_EVENT:
            case CtrlType.CTRL_C_EVENT:
            case CtrlType.CTRL_LOGOFF_EVENT:
            case CtrlType.CTRL_SHUTDOWN_EVENT:
            case CtrlType.CTRL_CLOSE_EVENT:
                Console.WriteLine("Closing ...");
                cts.Cancel();
                Environment.Exit(0);
                return false;

            default:
                return false;
        }
    }
}