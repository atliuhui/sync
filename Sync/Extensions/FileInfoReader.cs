namespace Sync.Extensions
{
    public class FileInfoReader
    {
        readonly FileInfo info;
        public FileInfoReader(FileInfo info)
        {
            this.info = info;
        }

        public string Path => this.info.FullName;
        public string Name => this.info.Name;
        public long Size => this.info.Length;
        public DateTime Date => FormatCreationTime(this.info);

        static DateTime FormatCreationTime(FileInfo file)
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
        static DateTime? FormatCreationTime_Filename(FileInfo file)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
            return GrokExtension.FormatDate(name);
        }
        static DateTime FormatCreationTime_Filemeta(FileInfo file)
        {
            var created = file.CreationTime;
            var updated = file.LastWriteTime;
            var defaultValue = created < updated ? created : updated;
            return defaultValue;
        }
    }
}
