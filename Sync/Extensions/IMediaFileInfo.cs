namespace Sync.Extensions
{
    /// <summary>
    /// Interface for media file information to enable testing with Fake objects
    /// </summary>
    public interface IMediaFileInfo
    {
        string FullName { get; }
        string Name { get; }
        ulong Length { get; }
        DateTime? CreationTime { get; }
        DateTime? LastWriteTime { get; }
    }
}
