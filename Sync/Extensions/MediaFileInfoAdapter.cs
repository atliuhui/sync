using MediaDevices;

namespace Sync.Extensions
{
    /// <summary>
    /// Adapter to make sealed MediaFileInfo compatible with IMediaFileInfo interface
    /// </summary>
    public class MediaFileInfoAdapter : IMediaFileInfo
    {
        private readonly MediaFileInfo _fileInfo;

        public MediaFileInfoAdapter(MediaFileInfo fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }

        public string FullName => _fileInfo.FullName;
        public string Name => _fileInfo.Name;
        public ulong Length => _fileInfo.Length;
        public DateTime? CreationTime => _fileInfo.CreationTime;
        public DateTime? LastWriteTime => _fileInfo.LastWriteTime;
    }
}
