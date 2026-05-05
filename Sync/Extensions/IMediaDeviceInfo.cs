namespace Sync.Extensions
{
    /// <summary>
    /// Interface for media device information to enable testing with Fake objects
    /// </summary>
    public interface IMediaDeviceInfo
    {
        string FriendlyName { get; }
        string SerialNumber { get; }
    }
}
