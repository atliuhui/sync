// See https://aka.ms/new-console-template for more information
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sync.Extensions;
using Sync.Options;
using Sync.Services;

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

try
{
    using var context = new RuntimeContext(services: host.Services);
    ParseAndRun(args, context);
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    Environment.ExitCode = 1;
}

static void ParseAndRun(string[] args, RuntimeContext context)
{
    Parser.Default.ParseArguments<DeviceOption>(args)
        .WithParsed(option => Run(option, context));
}
static void Run(DeviceOption option, RuntimeContext context)
{
    var devices = MediaDeviceExtension.FindDevice();

    if (option.Action == DeviceAction.None)
    {
        devices.Print();
        return;
    }

    using var device = devices.FindDevice(option.DeviceName);
    using var service = new DeviceService(context, device, option.RootPath);
    using var meter = new ActionMeter($"{service.SerialNumber}[{option.DeviceName}] {option.Action}");

    Execute(option.Action, service, meter);
    context.ArchiveIndex(service.SerialNumber);
}
static void Execute(DeviceAction action, DeviceService service, ActionMeter meter)
{
    Action<Action<string>> execute = action switch
    {
        DeviceAction.Scan => output => service.Scan(output),
        DeviceAction.Sync => output => service.Sync(output),
        _ => output => service.Sync(output),
    };

    execute(text =>
    {
        Console.Write($"\r{meter.ConsoleFormat(text)}");
    });
}
