using CoenM.ImageHash.HashAlgorithms;
using MediaDevices;
using MediaInfo;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sync.Extensions;
using System.IO.Hashing;

namespace Test
{
    [TestClass]
    public sealed class FileParserTest
    {
        [TestMethod]
        public void MediaInfoWrapper_Normal()
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(user, @"Downloads\离别开出花-伴奏-0140-剪裁.mp4");
            using var stream = File.OpenRead(path);
            var media = new MediaInfoWrapper(stream, NullLogger<MediaInfoWrapper>.Instance);

            Console.WriteLine($"Camera = {media.Codec}");
            Console.WriteLine($"Duration = {media.Duration}");
        }
        [TestMethod]
        public void ReadMetadata_Normal()
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(user, @"OneDrive\图片\20241025\20241028152413.jpg");
            using var stream = File.OpenRead(path);
            var meta = MetadataExtractor.ImageMetadataReader.ReadMetadata(stream);

            var tags = meta.SelectMany(item => item.Tags);
            Console.WriteLine($"Width = {tags.FirstOrDefault(item => item.Name == "Image Width")?.Description}");
            Console.WriteLine($"Height = {tags.FirstOrDefault(item => item.Name == "Image Height")?.Description}");
        }
        [TestMethod]
        public void ImageHash_Normal()
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(user, @"OneDrive\图片\20241025\20241028152413.jpg");
            using var stream = File.OpenRead(path);
            var algorithm = new AverageHash();
            var hash = algorithm.Hash(Image.Load<Rgba32>(stream));
            var code = hash.ToString("X16");

            Console.WriteLine(code);
        }
        [TestMethod]
        public void FileHash_Normal()
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(user, @"OneDrive\图片\20241025\20241028152413.jpg");
            using var stream = File.OpenRead(path);
            var algorithm = new XxHash64();
            algorithm.Append(stream);
            var hash = algorithm.GetCurrentHashAsUInt64();
            var code = hash.ToString("X16");

            Console.WriteLine(code);
        }
        [TestMethod]
        public void EnumerateFiles_Normal()
        {
            var devices = MediaDevice.GetDevices();
            var device = devices.FirstOrDefault(item => item.FriendlyName.Equals("Apple iPhone")) ?? throw new NullReferenceException("device_name");
            device.Connect();
            var source = device.GetRootDirectory();
            var items = source.EnumerateFiles().Where(item => true);

            Console.WriteLine(items.Count());
        }
        [TestMethod]
        public void EnumerateFiles_Print()
        {
            var devices = MediaDevice.GetDevices();
            devices.Print();
        }
        [TestMethod]
        public void EnumerateFiles_Redmi()
        {
            var devices = MediaDevice.GetDevices();
            var device = devices.FirstOrDefault(item => item.FriendlyName.Equals("Redmi 5 Plus")) ?? throw new NullReferenceException("device_name");
            device.Connect();
            var source = device.GetRootDirectory();
            var items = source.EnumerateFiles().Where(item => true);
            Console.WriteLine(items.Count());
            device.Disconnect();
            device.Dispose();
        }
    }
}
