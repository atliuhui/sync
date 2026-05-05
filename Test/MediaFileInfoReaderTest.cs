using Sync.Extensions;

namespace Test
{
    internal class StubMediaFileInfo : IMediaFileInfo
    {
        public Func<string> FullNameGetter { get; set; } = () => "";
        public Func<string> NameGetter { get; set; } = () => "";
        public Func<ulong> LengthGetter { get; set; } = () => 0;
        public Func<DateTime?> CreationTimeGetter { get; set; } = () => null;
        public Func<DateTime?> LastWriteTimeGetter { get; set; } = () => null;

        public string FullName => FullNameGetter();
        public string Name => NameGetter();
        public ulong Length => LengthGetter();
        public DateTime? CreationTime => CreationTimeGetter();
        public DateTime? LastWriteTime => LastWriteTimeGetter();
    }

    [TestClass]
    public sealed class MediaFileInfoReaderTest
    {
        [TestCategory("CI")]
        [TestMethod]
        public void CanReadPath()
        {
            var stub = new StubMediaFileInfo();
            stub.FullNameGetter = () => "/DCIM/IMG001.jpg";
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Path;

            Assert.AreEqual("/DCIM/IMG001.jpg", result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadName()
        {
            var stub = new StubMediaFileInfo();
            stub.NameGetter = () => "IMG001.jpg";
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Name;

            Assert.AreEqual("IMG001.jpg", result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadSize()
        {
            var stub = new StubMediaFileInfo();
            stub.LengthGetter = () => 1024000;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Size;

            Assert.AreEqual((ulong)1024000, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadDate_FromFileMetadata()
        {
            var testDate = new DateTime(2024, 5, 15, 14, 30, 0);
            var stub = new StubMediaFileInfo();
            stub.CreationTimeGetter = () => testDate;
            stub.LastWriteTimeGetter = () => testDate.AddHours(1);
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Date;

            Assert.AreEqual(testDate, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void DateExtraction_PreferCreationTime_WhenEarlier()
        {
            var creationTime = new DateTime(2024, 5, 15, 10, 0, 0);
            var modificationTime = new DateTime(2024, 5, 15, 14, 0, 0);
            var stub = new StubMediaFileInfo();
            stub.CreationTimeGetter = () => creationTime;
            stub.LastWriteTimeGetter = () => modificationTime;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Date;

            Assert.AreEqual(creationTime, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void DateExtraction_PreferLastWriteTime_WhenEarlier()
        {
            var creationTime = new DateTime(2024, 5, 15, 14, 0, 0);
            var modificationTime = new DateTime(2024, 5, 15, 10, 0, 0);
            var stub = new StubMediaFileInfo();
            stub.CreationTimeGetter = () => creationTime;
            stub.LastWriteTimeGetter = () => modificationTime;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Date;

            Assert.AreEqual(modificationTime, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void DateExtraction_HandleNullCreationTime()
        {
            var modificationTime = new DateTime(2024, 5, 15, 10, 0, 0);
            var stub = new StubMediaFileInfo();
            stub.CreationTimeGetter = () => null;
            stub.LastWriteTimeGetter = () => modificationTime;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Date;

            Assert.AreEqual(modificationTime, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void DateExtraction_HandleNullLastWriteTime()
        {
            var creationTime = new DateTime(2024, 5, 15, 10, 0, 0);
            var stub = new StubMediaFileInfo();
            stub.CreationTimeGetter = () => creationTime;
            stub.LastWriteTimeGetter = () => null;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Date;

            Assert.AreEqual(creationTime, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void DateExtraction_HandleBothNullDates()
        {
            var stub = new StubMediaFileInfo();
            stub.CreationTimeGetter = () => null;
            stub.LastWriteTimeGetter = () => null;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Date;

            Assert.AreEqual(DateTime.MaxValue, result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void PathAndNameProperties_WorkCorrectly()
        {
            var creationTime = new DateTime(2024, 5, 15, 10, 0, 0);
            var stub = new StubMediaFileInfo();
            stub.FullNameGetter = () => "/DCIM/Camera/IMG001.jpg";
            stub.NameGetter = () => "IMG001.jpg";
            stub.LengthGetter = () => 2048000;
            stub.CreationTimeGetter = () => creationTime;
            stub.LastWriteTimeGetter = () => creationTime;
            
            var reader = new MediaFileInfoReader(stub);

            var path = reader.Path;
            var name = reader.Name;
            var size = reader.Size;
            var date = reader.Date;

            Assert.AreEqual("/DCIM/Camera/IMG001.jpg", path);
            Assert.AreEqual("IMG001.jpg", name);
            Assert.AreEqual((ulong)2048000, size);
            Assert.AreEqual(creationTime, date);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void SizeProperty_ReturnsUlong()
        {
            var stub = new StubMediaFileInfo();
            stub.LengthGetter = () => (ulong)uint.MaxValue + 1;
            
            var reader = new MediaFileInfoReader(stub);

            var result = reader.Size;

            Assert.AreEqual((ulong)uint.MaxValue + 1, result);
            Assert.IsTrue(result > uint.MaxValue);
        }
    }
}
