using CommandLine;

namespace Sync.Options
{
#if DEBUG
    [Verb("Device", isDefault: false)]
#else
    [Verb("Device", isDefault: false, HelpText = "Device Sync")]
#endif
    internal class DeviceOption
    {
#if DEBUG
        [Option('a', "action", Required = false, Default = DeviceAction.Scan)]
#else
        [Option('a', "action", Required = true, HelpText = "None, Scan, Sync")]
#endif
        public DeviceAction Action { get; set; }
#if DEBUG
        //[Option('n', "name", Required = false, Default = "Apple iPhone")]
        [Option('n', "name", Required = false, Default = "Redmi 5 Plus")]
#else
        [Option('n', "name", Required = false, HelpText = "Device Name")]
#endif
        public string DeviceName { get; set; } = string.Empty;
#if DEBUG
        [Option('r', "root", Required = false, Default = @"")]
#else
        [Option('r', "root", Required = false, HelpText = "Device Root Directory")]
#endif
        public string RootPath { get; set; } = string.Empty;
    }

    internal enum DeviceAction
    {
        None,
        Scan,
        Sync,
    }
}
