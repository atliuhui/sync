using MediaDevices;

namespace Sync.Extensions
{
    /// <summary>
    /// Adapter to make sealed MediaDevice compatible with IMediaDeviceInfo interface
    /// </summary>
    public class MediaDeviceAdapter : IMediaDeviceInfo
    {
        private readonly MediaDevice _device;

        public MediaDeviceAdapter(MediaDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public string FriendlyName => _device.FriendlyName;
        public string SerialNumber => _device.SerialNumber;
    }
}
