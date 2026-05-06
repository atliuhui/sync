using Sync.Extensions;

namespace Test
{
    [TestClass]
    public sealed class FileInfoReaderTest
    {
        [TestCategory("CI")]
        [TestMethod]
        public void CanReadPath()
        {
            var tempFile = Path.GetTempFileName();
            
            try
            {
                File.WriteAllText(tempFile, "Test content for path reading");
                
                var fileInfo = new FileInfo(tempFile);
                var reader = new FileInfoReader(fileInfo);

                Assert.IsNotNull(reader.Path);
                Assert.IsGreaterThan(0, reader.Path.Length);
                Assert.AreEqual(tempFile, reader.Path);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadName()
        {
            var tempFile = Path.GetTempFileName();
            
            try
            {
                File.WriteAllText(tempFile, "Test content");
                var fileInfo = new FileInfo(tempFile);
                var reader = new FileInfoReader(fileInfo);
                
                Assert.EndsWith(".tmp", reader.Name);
                Assert.DoesNotContain("\\", reader.Name);
                Assert.DoesNotContain("/", reader.Name);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadSize()
        {
            var tempFile = Path.GetTempFileName();
            
            try
            {
                var testContent = "Test content for size check";
                File.WriteAllText(tempFile, testContent);
                var fileInfo = new FileInfo(tempFile);
                fileInfo.Refresh();
                
                var reader = new FileInfoReader(fileInfo);

                Assert.AreEqual(testContent.Length, reader.Size);
                Assert.IsGreaterThan(0, reader.Size);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadDate()
        {
            var tempFile = Path.GetTempFileName();
            
            try
            {
                File.WriteAllText(tempFile, "Test content");
                var fileInfo = new FileInfo(tempFile);
                fileInfo.Refresh();
                
                var reader = new FileInfoReader(fileInfo);

                Assert.AreNotEqual(DateTime.MinValue, reader.Date);
                Assert.IsTrue(reader.Date <= DateTime.Now.AddSeconds(1));
                Assert.IsTrue(reader.Date >= DateTime.Now.AddMinutes(-5));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestCategory("CI")]
        [TestMethod]
        public void DateExtraction_PreferCreationTime()
        {
            var tempFile = Path.GetTempFileName();
            
            try
            {
                File.WriteAllText(tempFile, "Test content");
                
                var pastDate = DateTime.Now.AddDays(-10);
                var fileInfo = new FileInfo(tempFile);
                fileInfo.CreationTime = pastDate;
                fileInfo.LastWriteTime = DateTime.Now;
                
                var reader = new FileInfoReader(fileInfo);

                Assert.IsTrue(reader.Date <= DateTime.Now.AddDays(-9));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
