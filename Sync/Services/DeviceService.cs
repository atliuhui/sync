using MediaDevices;
using Sync.Extensions;
using System.Runtime.InteropServices;

namespace Sync.Services
{
    internal class DeviceService : IDisposable
    {
        readonly RuntimeContext context;
        readonly MediaDevice device;
        readonly MediaDeviceInfoReader device_reader;
        readonly MediaDirectoryInfo source;

        public string SerialNumber => this.device_reader.SerialNumber;
        public string FriendlyName => this.device_reader.FriendlyName;

        public DeviceService(
            RuntimeContext context,
            MediaDevice device,
            string root)
        {
            this.context = context;
            this.device = device;
            this.device.Connect();
            this.device_reader = new MediaDeviceInfoReader(new MediaDeviceAdapter(device));
            if (string.IsNullOrEmpty(root))
            {
                this.source = device.GetRootDirectory();
            }
            else
            {
                this.source = device.GetDirectoryInfo(root);
            }
        }

        public void Scan(Action<string, bool> output)
        {
            this.Scan(this.source, output);
        }
        public void Sync(Action<string, bool> output)
        {
            this.Copy(this.source, output);
        }

        void Scan(MediaDirectoryInfo source, Action<string, bool> output)
        {
            var files = source.EnumerateFiles();
            foreach (var item in files)
            {
                this.context.RecordIndex(item.FullName, () => Scan(item, output));
            }

            var folders = source.EnumerateDirectories();
            foreach (var item in folders)
            {
                this.Scan(item, output);
            }
        }
        FileIndex Scan(MediaFileInfo source, Action<string, bool> output)
        {
            output($"{source.Name}", false);

            var reader = new MediaFileInfoReader(new MediaFileInfoAdapter(source));
            var source_index = new FileIndex
            {
                Path = reader.Path,
                Size = (long)reader.Size,
                Date = reader.Date,
                Note = $"scan",
            };

            return source_index;
        }
        void Copy(MediaDirectoryInfo source, Action<string, bool> output)
        {
            var files = SafeEnumerateFiles(source, output)
                .Where(item => this.context.AllowedFile(item.Name));
            foreach (var item in files)
            {
                try
                {
                    this.context.RecordIndex(item.FullName, () => Copy(item, output));
                }
                catch (COMException ex)
                {
                    output($"0x{ex.HResult:X8} {item.FullName} {ex.Message}", true);
                    this.context.RecordIndex(item.FullName, () => new FileIndex
                    {
                        Path = item.FullName,
                        Note = $"copy:exception:0x{ex.HResult:X8}",
                    });
                }
            }

            var folders = SafeEnumerateDirectories(source, output)
                .Where(item => this.context.AllowedDirectory(item.Name));
            foreach (var item in folders)
            {
                this.Copy(item, output);
            }
        }
        IEnumerable<MediaFileInfo> SafeEnumerateFiles(MediaDirectoryInfo source, Action<string, bool> output)
        {
            IEnumerator<MediaFileInfo>? enumerator = null;
            try
            {
                enumerator = source.EnumerateFiles().GetEnumerator();
            }
            catch (COMException ex)
            {
                output($"0x{ex.HResult:X8} {source.FullName} [all-files]: {ex.Message}", true);
                yield break;
            }

            using (enumerator)
            {
                while (true)
                {
                    MediaFileInfo current;
                    try
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }
                        current = enumerator.Current;
                    }
                    catch (COMException ex)
                    {
                        output($"0x{ex.HResult:X8} {source.FullName} [part-files]: {ex.Message}", true);
                        yield break;
                    }
                    yield return current;
                }
            }
        }
        IEnumerable<MediaDirectoryInfo> SafeEnumerateDirectories(MediaDirectoryInfo source, Action<string, bool> output)
        {
            IEnumerator<MediaDirectoryInfo>? enumerator = null;
            try
            {
                enumerator = source.EnumerateDirectories().GetEnumerator();
            }
            catch (COMException ex)
            {
                output($"0x{ex.HResult:X8} {source.FullName} [all-dirs]: {ex.Message}", true);
                yield break;
            }

            using (enumerator)
            {
                while (true)
                {
                    MediaDirectoryInfo current;
                    try
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }
                        current = enumerator.Current;
                    }
                    catch (COMException ex)
                    {
                        output($"0x{ex.HResult:X8} {source.FullName} [part-dirs]: {ex.Message}", true);
                        yield break;
                    }
                    yield return current;
                }
            }
        }
        FileIndex Copy(MediaFileInfo source, Action<string, bool> output)
        {
            output($"{source.Name}", false);

            var reader = new MediaFileInfoReader(new MediaFileInfoAdapter(source));
            var source_index = new FileIndex
            {
                Path = reader.Path,
                Size = (long)reader.Size,
                Date = reader.Date,
            };
            var target_file = CreateTarget(reader, source_index);
            var projection_file = new FileInfo($"{target_file.FullName}.rsls");

            if (projection_file.Exists)
            {
                source_index.Note = $"skip:projection";
            }
            else if (target_file.Exists == false)
            {
                this.Copy(source, target_file);
                source_index.Note = $"copy";
            }
            else
            {
                var target_reader = new FileInfoReader(target_file);
                if (source_index.Size == target_reader.Size)
                {
                    source_index.Note = $"skip:same";
                }
                else
                {
                    source_index.Note = $"skip:size-mismatch:{source_index.Size}|{target_reader.Size}";
                }
            }

            return source_index;
        }
        void Copy(MediaFileInfo source, FileInfo target)
        {
            if (target.Directory?.Exists == false)
            {
                target.Directory.Create();
            }
            try
            {
                source.CopyTo(target.FullName, false);
            }
            catch
            {
                // Clean up partial file so a retry can proceed and the index stays consistent.
                try
                {
                    target.Refresh();
                    if (target.Exists)
                    {
                        target.Delete();
                    }
                }
                catch
                {
                    // Swallow cleanup failures; rethrow original below.
                }
                throw;
            }
        }
        FileInfo CreateTarget(MediaFileInfoReader reader, FileIndex index)
        {
            var folder = this.context.GetSubfolder(index);
            var path = Path.Combine(this.context.StorageGalleryDir.FullName, this.device_reader.SerialNumber, folder, reader.Name);
            var target_file = new FileInfo(path);

            return target_file;
        }

        public void Dispose()
        {
            try
            {
                if (this.device.IsConnected)
                {
                    this.device.Disconnect();
                }
            }
            finally
            {
                this.device.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
