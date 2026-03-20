using CsvHelper;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sync.Extensions;
using System.Globalization;
using System.Text;

namespace Sync.Services
{
    internal class RuntimeContext : IDisposable
    {
        public IEnumerable<string> DeviceCameraNamespaces { get; }
        public DirectoryInfo StorageCachingDir { get; }
        public DirectoryInfo StorageGalleryDir { get; }
        public FileInfo StorageCachingIndexFile { get; }
        StreamWriter? StorageCachingIndexStreamWriter { get; set; }
        CsvWriter? StorageCachingIndexWriter { get; set; }
        public IFluidTemplate StorageGallerySubfolderNameTemplate { get; }
        public IEnumerable<string> IgnoredFilePrefixs { get; }
        public IEnumerable<string> IgnoredFileSuffixs { get; }
        public IEnumerable<string> IgnoredDirectoryPrefixs { get; }
        public IEnumerable<string> IgnoredDirectorySuffixs { get; }
        public IDictionary<string, string> Mappings { get; }
        public Dictionary<string, FileIndex> Records { get; }
        public ILogger Logger { get; }

        public RuntimeContext(IServiceProvider services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            var storage_caching_path = configuration.GetValue<string>("Storage:Caching:Path") ?? throw new ArgumentNullException("Storage:Caching:Path");
            var storage_caching_indexfile_name = configuration.GetValue<string>("Storage:Caching:IndexFile:Name") ?? throw new ArgumentNullException("Storage:Caching:IndexFile:Name");
            var storage_gallery_path = configuration.GetValue<string>("Storage:Gallery:Path") ?? throw new ArgumentNullException("Storage:Gallery:Path");
            var storage_gallery_subfolder_name_template = configuration.GetValue<string>("Storage:Gallery:Subfolder:Name:Template") ?? throw new ArgumentNullException("Storage:Gallery:Subfolder:Name:Template");
            var env_user_profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var parser = new FluidParser();

            this.DeviceCameraNamespaces = new List<string>();
            this.IgnoredFilePrefixs = new List<string>();
            this.IgnoredFileSuffixs = new List<string>();
            this.IgnoredDirectoryPrefixs = new List<string>();
            this.IgnoredDirectorySuffixs = new List<string>();
            this.Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Records = new Dictionary<string, FileIndex>();
            this.Logger = services.GetRequiredService<ILogger<RuntimeContext>>();

            configuration.GetRequiredSection("Device:Camera:Namespaces").Bind(this.DeviceCameraNamespaces);
            configuration.GetRequiredSection("Ignore:File:Prefix").Bind(this.IgnoredFilePrefixs);
            configuration.GetRequiredSection("Ignore:File:Suffix").Bind(this.IgnoredFileSuffixs);
            configuration.GetRequiredSection("Ignore:Directory:Prefix").Bind(this.IgnoredDirectoryPrefixs);
            configuration.GetRequiredSection("Ignore:Directory:Suffix").Bind(this.IgnoredDirectorySuffixs);
            configuration.GetRequiredSection("Mapping:CategoryProvider").Bind(this.Mappings);

            this.StorageCachingDir = new DirectoryInfo(Path.Combine(env_user_profile, storage_caching_path));
            this.StorageGalleryDir = new DirectoryInfo(Path.Combine(env_user_profile, storage_gallery_path));
            this.StorageCachingIndexFile = new FileInfo(Path.Combine(this.StorageCachingDir.FullName, storage_caching_indexfile_name));
            this.StorageGallerySubfolderNameTemplate = parser.Parse(storage_gallery_subfolder_name_template);
            this.Mappings = this.Mappings.Where(item => item.Value.Equals("ignore", StringComparison.OrdinalIgnoreCase) == false).ToDictionary(StringComparer.OrdinalIgnoreCase);
            this.LoadIndex();

            GrokExtension.Init(configuration);
        }

        void LoadIndex()
        {
            if (this.StorageCachingIndexFile.Exists)
            {
                using (var stream = this.StorageCachingIndexFile.OpenRead())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<FileIndex>();
                    foreach (var item in records)
                    {
                        this.Records.Add(item.Path, item);
                    }
                }
            }

            this.StorageCachingIndexStreamWriter = new StreamWriter(this.StorageCachingIndexFile.FullName, true, Encoding.UTF8);
            this.StorageCachingIndexWriter = new CsvWriter(this.StorageCachingIndexStreamWriter, CultureInfo.InvariantCulture);
            if (this.StorageCachingIndexFile.Exists == false)
            {
                this.StorageCachingIndexWriter.WriteHeader<FileIndex>();
                this.StorageCachingIndexWriter.NextRecord();
                this.StorageCachingIndexWriter.Flush();
            }
        }
        public void ArchiveIndex(string alias)
        {
            this.StorageCachingIndexWriter?.Dispose();
            this.StorageCachingIndexWriter = null;

            this.StorageCachingIndexStreamWriter?.Dispose();
            this.StorageCachingIndexStreamWriter = null;

            var path = Path.Combine(this.StorageCachingIndexFile.Directory!.FullName, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-{alias}.csv");
            File.Move(this.StorageCachingIndexFile.FullName, path);
        }
        public void RecordIndex(string key, Func<FileIndex> action)
        {
            if (this.Records.ContainsKey(key) == false)
            {
                var index = action.Invoke();
                lock (this)
                {
                    this.Records.Add(key, index);
                    this.StorageCachingIndexWriter?.WriteRecord(index);
                    this.StorageCachingIndexWriter?.NextRecord();
                    this.StorageCachingIndexWriter?.Flush();
                }
            }
        }
        public string GetSubfolder(FileIndex index)
        {
            if (this.StorageGallerySubfolderNameTemplate.TryRender(JObject.FromObject(index), out var text, out var message))
            {
                return text ?? throw new ArgumentNullException("Storage:Gallery:Subfolder:Name:Template#Render"); ;
            }
            else
            {
                throw new ArgumentNullException("Storage:Gallery:Subfolder:Name:Template#Render");
            }
        }
        public bool AllowedFile(string name)
        {
            return this.IgnoredFile(name) == false
                && this.Mappings.Keys.Contains(Path.GetExtension(name));
        }
        public bool IgnoredFile(string name)
        {
            return this.IgnoredFilePrefixs.Any(item => name.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                || this.IgnoredFileSuffixs.Any(item => name.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
        public bool AllowedDirectory(string name)
        {
            return this.IgnoredDirectory(name) == false;
        }
        public bool IgnoredDirectory(string name)
        {
            return this.IgnoredDirectoryPrefixs.Any(item => name.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                || this.IgnoredDirectorySuffixs.Any(item => name.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            if (this.StorageCachingIndexWriter != null) { this.StorageCachingIndexWriter.Dispose(); }
            if (this.StorageCachingIndexStreamWriter != null) { this.StorageCachingIndexStreamWriter.Dispose(); }
        }
    }

    public class FileIndex
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
}
