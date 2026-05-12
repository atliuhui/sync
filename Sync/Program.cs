// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sync.Extensions;
using Sync.Services;
using System.CommandLine;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(config =>
        {
            config.AddConsole(options =>
            {
                options.FormatterName = "SimpleConsoleFormatter";
            });
            config.AddConsoleFormatter<SimpleConsoleFormatter, ConsoleFormatterOptions>();
            config.AddFilter((category, level) => false);
        });
    });
var host = builder.Build();

// https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/tutorials/system-command-line
var actionOption = new Option<DeviceAction>("--action", "-a")
{
    Description = $"Action to perform on the device ({string.Join(" | ", Enum.GetNames<DeviceAction>())})",
    HelpName = "action",
#if DEBUG
    DefaultValueFactory = _ => DeviceAction.Scan,
#else
    Required = true,
#endif
};
var nameOption = new Option<string>("--name", "-n")
{
    Description = "Friendly name of the target media device (e.g. \"Apple iPhone\")",
    HelpName = "name",
#if DEBUG
    DefaultValueFactory = _ => "Redmi 5 Plus",
#else
    DefaultValueFactory = _ => string.Empty,
#endif
};
var rootOption = new Option<string>("--root", "-r")
{
    Description = "Root directory on the device to scan or sync (empty = device default)",
    HelpName = "path",
    DefaultValueFactory = _ => string.Empty,
};

var deviceCommand = new Command("Device", "Scan or sync media files on a connected portable device")
{
    Options = { actionOption, nameOption, rootOption, },
};

var rootCommand = new RootCommand("Sync — command-line tool for scanning and syncing media files from portable devices")
{
    Options = { actionOption, nameOption, rootOption, },
    Subcommands = { deviceCommand, },
};

try
{
    using var context = new RuntimeContext(services: host.Services);
    deviceCommand.SetAction(parseResult =>
    {
        var action = parseResult.GetValue(actionOption);
        var name = parseResult.GetValue(nameOption) ?? string.Empty;
        var root = parseResult.GetValue(rootOption) ?? string.Empty;
        Run(action, name, root, context);
    });

    return rootCommand.Parse(args).Invoke();
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

static void Run(DeviceAction action, string name, string rootPath, RuntimeContext context)
{
    var devices = MediaDeviceExtension.FindDevice();

    if (action == DeviceAction.None)
    {
        devices.Print();
        return;
    }

    using var device = devices.FindDevice(name);
    using var service = new DeviceService(context, device, rootPath);
    using var meter = new ActionMeter($"{service.SerialNumber}[{name}] {action}");

    Execute(action, service, meter);
    context.ArchiveIndex(service.SerialNumber);
}
static void Execute(DeviceAction action, DeviceService service, ActionMeter meter)
{
    Action<Action<string, bool>> execute = action switch
    {
        DeviceAction.Scan => output => service.Scan(output),
        DeviceAction.Sync => output => service.Sync(output),
        _ => output => service.Sync(output),
    };

    var lastProgress = string.Empty;
    execute((text, isError) =>
    {
        if (isError)
        {
            meter.ConsoleClear();
            Console.WriteLine(text);
        }
        else
        {
            lastProgress = text;
        }

        if (lastProgress.Length > 0)
        {
            Console.Write($"\r{meter.ConsoleFormat(lastProgress)}");
        }
    });
}

enum DeviceAction
{
    None,
    Scan,
    Sync,
}
