using System.Numerics;
using FileServerMaster.Controllers;
using FileServerMaster.Storage;
using FileServerMaster.Web.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileServerMaster.Tests
{
    public class CheckinControllerTests
    {
        private Mock<IFileContainer>? _fileContainer;
        private Mock<IFileDistributorManager>? _fileDistributorManager;

        [TestCase((string[])[], (string[])[], (string[])[], 0)]
        [TestCase((string[])["cat"], (string[])[], (string[])["cat"], 1)]
        [TestCase((string[])["cat"], (string[])[], (string[])["cat"], 1)]
        [TestCase((string[])[], (string[])[], (string[])["cat", "dog"], 2)]
        [TestCase((string[])["cat"], (string[])["cat"], (string[])["cat"], 0)]
        [TestCase((string[])["cat", "dog"], (string[])["cat"], (string[])["cat", "dog"], 1)]
        public void CheckIn(string[] containerFiles , string[] remoteFiles, string[] totalFiles, int retrivalCount)
        {
            _fileContainer = new Mock<IFileContainer>();
            _fileContainer.Setup(i => i.GetTempFileNames()).Returns(containerFiles);
            _fileContainer.Setup(i => i.DiscardFilesAsync(It.IsAny<string[]>()));

            _fileDistributorManager = new Mock<IFileDistributorManager>();
            _fileDistributorManager.Setup(i => i.UpdateFileAvailablity(new HostString("1.1.1.1", 443), remoteFiles));
            _fileDistributorManager.Setup(i => i.GetAllFileNames()).Returns(totalFiles);

            foreach (var fileName in totalFiles)
            {
                _fileDistributorManager.Setup(i => i.GetRetrivalHost(fileName)).Returns(new HostString("1.1.1.1", 443));
            }

            var controller = GetController();
            var resp = controller.CheckIn(new AvailableFiles { AvailableFileNames = remoteFiles, SlaveHostStrings = [new HostString("1.1.1.1", 443).ToString()] });

            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FileLinks, Is.Not.Null);
            Assert.That(resp.FileLinks, Has.Count.EqualTo(retrivalCount));
            foreach (var fileName in resp.FileLinks.Keys)
            {
                Assert.That(resp.FileLinks[fileName], Is.EqualTo(new HostString("1.1.1.1", 443).ToString()));
            }
        }

        private CheckInController? GetController()
        {
            //return new CheckInController(_fileDistributorManager!.Object, _fileContainer!.Object, new Mock<ILogger<CheckInController>>().Object);
            return null;
        }
    }
}