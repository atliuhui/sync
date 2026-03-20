using MediaDevices;

namespace Sync.Extensions
{
    public static class MediaDeviceExtension
    {
        public static IEnumerable<MediaDevice> FindDevice()
        {
            return MediaDevice.GetDevices();
        }
        public static MediaDevice FindDevice(this IEnumerable<MediaDevice> devices, string name)
        {
            var device = devices.FirstOrDefault(item => item.FriendlyName.Equals(name));
            ArgumentNullException.ThrowIfNull(device);
            return device;
        }
        public static void Print(this IEnumerable<MediaDevice> devices)
        {
            foreach (var item in devices)
            {
                Console.WriteLine($"FriendlyName = {item.FriendlyName}");
                Console.WriteLine($"Description  = {item.Description}");
                Console.WriteLine($"Manufacturer = {item.Manufacturer}");
                Console.WriteLine($"DeviceId     = {item.DeviceId}");
                Console.WriteLine();
            }
        }
    }
}
