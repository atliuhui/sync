using MediaDevices;

namespace Sync.Extensions
{
    public class MediaDeviceInfoReader
    {
        readonly MediaDevice info;
        public MediaDeviceInfoReader(MediaDevice info)
        {
            this.info = info;
        }

        public string SerialNumber => this.info.SerialNumber;
        public string FriendlyName => this.info.FriendlyName;
    }
}
