using MediaDevices;
using Sync.Extensions;

namespace Test
{
    [TestClass]
    public sealed class DebugTest
    {
        [TestMethod]
        public void EnumerateFiles_Normal()
        {
            var devices = MediaDevice.GetDevices();
            var device = devices.FirstOrDefault(item => item.FriendlyName.Equals("Apple iPhone")) ?? throw new NullReferenceException("device_name");
            device.Connect();
            var source = device.GetRootDirectory();
            var items = source.EnumerateFiles().Where(item => true);

            Console.WriteLine(items.Count());
        }
        [TestMethod]
        public void EnumerateFiles_Print()
        {
            var devices = MediaDevice.GetDevices();
            devices.Print();
        }
        [TestMethod]
        public void EnumerateFiles_Redmi()
        {
            var devices = MediaDevice.GetDevices();
            var device = devices.FirstOrDefault(item => item.FriendlyName.Equals("Redmi 5 Plus")) ?? throw new NullReferenceException("device_name");
            device.Connect();
            var source = device.GetRootDirectory();
            var items = source.EnumerateFiles().Where(item => true);
            Console.WriteLine(items.Count());
            device.Disconnect();
            device.Dispose();
        }
    }
}
