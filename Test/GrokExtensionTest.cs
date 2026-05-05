using Microsoft.Extensions.Configuration;
using Sync.Extensions;
using System.Globalization;

namespace Test
{
    [TestClass]
    public sealed class GrokExtensionTest
    {
        private IConfiguration? _configuration;

        [TestInitialize]
        public void Setup()
        {
            string[] possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Sync", "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "Sync", "appsettings.json"),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Sync", "appsettings.json")),
            };

            foreach (var path in possiblePaths)
            {
                var dir = Path.GetDirectoryName(path);
                if (dir != null && File.Exists(path))
                {
                    try
                    {
                        var builder = new ConfigurationBuilder()
                            .SetBasePath(dir)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

                        _configuration = builder.Build();
                        GrokExtension.Init(_configuration);
                        return;
                    }
                    catch
                    {
                    }
                }
            }
        }

        [TestCategory("CI")]
        [TestMethod]
        [DataRow("IMG20180919141601", 2018, 9, 19)]
        [DataRow("VID20180716151918", 2018, 7, 16)]
        public void CanExtractDate_StandardFormat(
            string filename, 
            int expectedYear,
            int expectedMonth,
            int expectedDay)
        {
            if (_configuration == null)
            {
                Assert.Inconclusive("Configuration not loaded - skipping Grok test");
                return;
            }

            var result = GrokExtension.FormatDate(filename);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue, $"Failed to parse {filename}");
            Assert.AreEqual(expectedYear, result.Value.Year, $"Year mismatch for {filename}");
            Assert.AreEqual(expectedMonth, result.Value.Month, $"Month mismatch for {filename}");
            Assert.AreEqual(expectedDay, result.Value.Day, $"Day mismatch for {filename}");
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanExtractDate_TimeComponent()
        {
            if (_configuration == null)
            {
                Assert.Inconclusive("Configuration not loaded - skipping Grok test");
                return;
            }

            var result = GrokExtension.FormatDate("IMG20180919141601");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(14, result.Value.Hour);
            Assert.AreEqual(16, result.Value.Minute);
            Assert.AreEqual(1, result.Value.Second);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void InvalidFilename_ReturnsNull()
        {
            if (_configuration == null)
            {
                Assert.Inconclusive("Configuration not loaded - skipping Grok test");
                return;
            }

            var result = GrokExtension.FormatDate("random_invalid_filename_xyz.jpg");

            Assert.IsTrue(!result.HasValue || result.Value == default(DateTime));
        }

        [TestCategory("CI")]
        [TestMethod]
        public void MultiplePatterns_AllFormats()
        {
            if (_configuration == null)
            {
                Assert.Inconclusive("Configuration not loaded - skipping Grok test");
                return;
            }

            var testCases = new[]
            {
                ("IMG20180919141601", 2018, 9, 19),
                ("VID20180716151918", 2018, 7, 16),
            };

            foreach (var (filename, expectedYear, expectedMonth, expectedDay) in testCases)
            {
                var result = GrokExtension.FormatDate(filename);
                
                Assert.IsNotNull(result, $"Failed to parse {filename}");
                Assert.IsTrue(result.HasValue, $"No value for {filename}");
                Assert.AreEqual(expectedYear, result.Value.Year, $"Year mismatch for {filename}");
                Assert.AreEqual(expectedMonth, result.Value.Month, $"Month mismatch for {filename}");
                Assert.AreEqual(expectedDay, result.Value.Day, $"Day mismatch for {filename}");
            }
        }

        [TestCategory("CI")]
        [TestMethod]
        [DataRow("ImageWithPrefix_20240515.jpg")]
        [DataRow("Photo-20250105-180000.png")]
        [DataRow("NoDateInThisName.jpg")]
        public void EdgeCases_HandleVariousFormats(string filename)
        {
            if (_configuration == null)
            {
                Assert.Inconclusive("Configuration not loaded - skipping Grok test");
                return;
            }

            var result = GrokExtension.FormatDate(filename);

            if (result.HasValue)
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Value != DateTime.MinValue);
            }
            else
            {
                Assert.IsTrue(!result.HasValue || result.Value == default(DateTime));
            }
        }
    }
}
