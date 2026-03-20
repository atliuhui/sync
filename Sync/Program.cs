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

using (var context = new RuntimeContext(services: host.Services))
{
    Parser.Default.ParseArguments<DeviceOption>(args)
    .WithParsed<DeviceOption>(option =>
    {
        var devices = MediaDeviceExtension.FindDevice();

        if (option.Action == DeviceAction.None)
        {
            devices.Print();
            return;
        }

        switch (option.Action)
        {
            case DeviceAction.None:
                devices.Print();
                break;
            case DeviceAction.Scan:
                using (var device = devices.FindDevice(option.DeviceName))
                using (var service = new DeviceService(context, device, option.RootPath))
                using (var meter = new ActionMeter($"{service.SerialNumber}[{option.DeviceName}] {option.Action}"))
                {
                    service.Scan((text) =>
                    {
                        Console.Write($"\r{meter.ConsoleFormat(text)}");
                    });
                    context.ArchiveIndex(service.SerialNumber);
                }
                break;
            case DeviceAction.Sync:
            default:
                using (var device = devices.FindDevice(option.DeviceName))
                using (var service = new DeviceService(context, device, option.RootPath))
                using (var meter = new ActionMeter($"{service.SerialNumber}[{option.DeviceName}] {option.Action}"))
                {
                    service.Sync((text) =>
                    {
                        Console.Write($"\r{meter.ConsoleFormat(text)}");
                    });
                    context.ArchiveIndex(service.SerialNumber);
                }
                break;
        }
    })
    ;
}
