namespace Sync.Extensions
{
    public class MediaDeviceInfoReader
    {
        readonly IMediaDeviceInfo info;
        public MediaDeviceInfoReader(IMediaDeviceInfo info)
        {
            this.info = info ?? throw new ArgumentNullException(nameof(info));
        }

        public string SerialNumber => this.info.SerialNumber;
        public string FriendlyName => this.info.FriendlyName;
    }
}
