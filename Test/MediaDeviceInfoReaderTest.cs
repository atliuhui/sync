using Sync.Extensions;

namespace Test
{
    internal class StubMediaDeviceInfo : IMediaDeviceInfo
    {
        public Func<string> FriendlyNameGetter { get; set; } = () => "";
        public Func<string> SerialNumberGetter { get; set; } = () => "";

        public string FriendlyName => FriendlyNameGetter();
        public string SerialNumber => SerialNumberGetter();
    }

    [TestClass]
    public sealed class MediaDeviceInfoReaderTest
    {
        [TestCategory("CI")]
        [TestMethod]
        public void CanReadFriendlyName()
        {
            var stub = new StubMediaDeviceInfo();
            stub.FriendlyNameGetter = () => "iPhone";
            stub.SerialNumberGetter = () => "ABC123456";
            
            var reader = new MediaDeviceInfoReader(stub);

            var result = reader.FriendlyName;

            Assert.AreEqual("iPhone", result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void CanReadSerialNumber()
        {
            var stub = new StubMediaDeviceInfo();
            stub.FriendlyNameGetter = () => "iPhone";
            stub.SerialNumberGetter = () => "ABC123456";
            
            var reader = new MediaDeviceInfoReader(stub);

            var result = reader.SerialNumber;

            Assert.AreEqual("ABC123456", result);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void BothProperties_WorkCorrectly()
        {
            var stub = new StubMediaDeviceInfo();
            stub.FriendlyNameGetter = () => "Samsung Galaxy";
            stub.SerialNumberGetter = () => "XYZ789012";
            
            var reader = new MediaDeviceInfoReader(stub);

            var friendlyName = reader.FriendlyName;
            var serialNumber = reader.SerialNumber;

            Assert.AreEqual("Samsung Galaxy", friendlyName);
            Assert.AreEqual("XYZ789012", serialNumber);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void Reader_PassthroughsProperties()
        {
            var testName = "Test Device";
            var testSerial = "TEST001";
            var stub = new StubMediaDeviceInfo();
            stub.FriendlyNameGetter = () => testName;
            stub.SerialNumberGetter = () => testSerial;
            
            var reader = new MediaDeviceInfoReader(stub);

            Assert.AreEqual(testName, reader.FriendlyName);
            Assert.AreEqual(testSerial, reader.SerialNumber);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void Reader_HandlesEmptyStrings()
        {
            var stub = new StubMediaDeviceInfo();
            stub.FriendlyNameGetter = () => "";
            stub.SerialNumberGetter = () => "";
            
            var reader = new MediaDeviceInfoReader(stub);

            var friendlyName = reader.FriendlyName;
            var serialNumber = reader.SerialNumber;

            Assert.AreEqual("", friendlyName);
            Assert.AreEqual("", serialNumber);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void Reader_AcceptsSpecialCharacters()
        {
            var stub = new StubMediaDeviceInfo();
            stub.FriendlyNameGetter = () => "Device (Mobile) #1";
            stub.SerialNumberGetter = () => "SN-2024/05/15";
            
            var reader = new MediaDeviceInfoReader(stub);

            var friendlyName = reader.FriendlyName;
            var serialNumber = reader.SerialNumber;

            Assert.AreEqual("Device (Mobile) #1", friendlyName);
            Assert.AreEqual("SN-2024/05/15", serialNumber);
        }

        [TestCategory("CI")]
        [TestMethod]
        public void Reader_ThrowsOnNullDeviceInfo()
        {
            try
            {
                var reader = new MediaDeviceInfoReader(null!);
                Assert.Fail("Expected ArgumentNullException was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("info", ex.ParamName);
            }
        }
    }
}
