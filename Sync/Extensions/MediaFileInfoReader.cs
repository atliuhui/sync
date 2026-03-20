using MediaDevices;
using IOPath = System.IO.Path;

namespace Sync.Extensions
{
    public class MediaFileInfoReader
    {
        readonly MediaFileInfo info;
        public MediaFileInfoReader(MediaFileInfo info)
        {
            this.info = info;
        }

        public string Path => this.info.FullName;
        public string Name => this.info.Name;
        public ulong Size => this.info.Length;
        public DateTime Date => FormatCreationTime(this.info);

        static DateTime FormatCreationTime(MediaFileInfo file)
        {
            var defaultValue = FormatCreationTime_Filemeta(file);
            if (defaultValue == DateTime.MinValue)
            {
                return FormatCreationTime_Filename(file) ?? defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }
        static DateTime? FormatCreationTime_Filename(MediaFileInfo file)
        {
            var name = IOPath.GetFileNameWithoutExtension(file.Name);
            return GrokExtension.FormatDate(name);
        }
        static DateTime FormatCreationTime_Filemeta(MediaFileInfo file)
        {
            var created = file.CreationTime ?? DateTime.MaxValue;
            var updated = file.LastWriteTime ?? DateTime.MaxValue;
            var defaultValue = created < updated ? created : updated;
            return defaultValue;
        }
    }
}
