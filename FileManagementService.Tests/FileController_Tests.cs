using FileManagementService.Controllers;
using FileManagementService.Models;
using FileManagementService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FileManagementService.Tests
{
    /// <summary>
    /// The FileController Test class
    /// </summary>
    public class FileController_Tests
    {
        /// <summary>
        /// The file config options
        /// </summary>
        private Mock<IOptions<FileConfig>> _mockFileConfigOptions;

        /// <summary>
        /// The mock logger 
        /// </summary>
        private Mock<ILogger<FileController>> _mockLogger;

        /// <summary>
        /// The File controller
        /// </summary>
        private FileController _fileController;

        /// <summary>
        /// Mock Blob Service
        /// </summary>
        private Mock<IBlobStorageService> _mockBblobService;

        /// <summary>
        /// initialises a new instance of <see cref="FileController_Tests"/> and set up tests 
        /// </summary>
        public FileController_Tests()
        {
            this._mockLogger = new Mock<ILogger<FileController>>();
            this.SetUpFileConfig();
            this._mockBblobService = new Mock<IBlobStorageService>();
        }

        /// <summary>
        /// Tests UploadFile Post method return Ok response for valid file
        /// </summary>
        [Fact]
        public void UploadFile_PDF_ReturnsOK()
        {
            // Arrange 
            IFormFile file = CreateTestFormFile("Test.pdf", "Test Content", "application/pdf", 2 * 1024 * 1024);
            var _uri = $"http://WindowsAzure.co.uk/test.pdf";
            this._mockBblobService.Setup(x => x.UploadFileBlobAsync(It.IsAny<FileDetails>())).Returns(() => Task.FromResult(_uri.ToString()));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.UploadFile(file).Result;
            var expected = _uri.ToString();

            // Assert 
            Assert.IsType<OkObjectResult>(actual);
            Assert.Equal(expected, ((OkObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests Upload File returns Bad Request for invalid file type
        /// </summary>
        [Fact]
        public void UploadFile_NonPDF_ReturnsBadRequest()
        {
            // Arrange 
            IFormFile file = CreateTestFormFile("Test.json", "Test Content", "application/json", 10 * 1024);
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.UploadFile(file).Result;            
            var actualErrorString = ((SerializableError)(((BadRequestObjectResult)actual).Value)).GetValueOrDefault("InvalidFileType");
            var expectedErrorString = "Input file type is not supported";


            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);            
            Assert.Equal(expectedErrorString, ((string[])actualErrorString)[0]);
        }

        /// <summary>
        /// Tests Upload File returns Bad Request for invalid file type
        /// </summary>
        [Fact]
        public void UploadFile_FileLargerThanAllowedSize_ReturnsBadRequest()
        {
            // Arrange 
            IFormFile file = CreateTestFormFile("Test.pdf", "Test Content", "application/json", 1024 * 1024 * 1024);
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.UploadFile(file).Result;
            var actualErrorString = ((SerializableError)(((BadRequestObjectResult)actual).Value)).GetValueOrDefault("FileSizeTooBig");
            var expectedErrorString = "File size is bigger than maximum allowed file size 5242880";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expectedErrorString, ((string[])actualErrorString)[0]);
        }

        /// <summary>
        /// Tests Upload File returns Bad Request for invalid file type
        /// </summary>
        [Fact]
        public void UploadFile_BlobServiceThrowException_ReturnsBadRequest()
        {
            // Arrange 
            IFormFile file = CreateTestFormFile("Test.pdf", "Test Content", "application/pdf", 2 * 1024 * 1024);
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);            
            this._mockBblobService.Setup(x => x.UploadFileBlobAsync(It.IsAny<FileDetails>())).Throws(new UnauthorizedAccessException("Not Authorised"));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.UploadFile(file).Result;
            var expected = "failed to upload file : Test.pdf ";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests File download is successfull. 
        /// </summary>
        [Fact]
        public void GetFile_Return_File()
        {
            // Arrange 
            var fileName = "Test.pdf";
            var contentType = "application/pdf";
            byte[] bytes = Encoding.ASCII.GetBytes("This is test file string");
            FileContentResult fileContent = new FileContentResult(bytes, contentType);
            fileContent.FileDownloadName = fileName;
            FileDetails _file = new FileDetails()
            {
                FileName = fileName,
                ContentType = contentType,
                Content = fileContent.FileContents
            };
            this._mockBblobService.Setup(x => x.CheckFileBlobExistsAsync(It.IsAny<string>())).Returns(true);
            this._mockBblobService.Setup(x => x.GetFileBlobAsync(It.IsAny<string>())).Returns(() => Task.FromResult(_file));            
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);
                       

            // Act
            var actual = this._fileController.GetFile(fileName).Result as FileContentResult;
            var expected = fileContent;

            // Assert 
            Assert.IsType<FileContentResult>(actual);
            Assert.Equal(expected.ContentType, actual.ContentType);            
        }

        /// <summary>
        /// Tests getFile returns Bad Request when no file name provided
        /// </summary>
        [Fact]
        public void GetFile_WithNoFileName_Return_BadRequest()
        {
           // Arrange
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.GetFile("").Result;
            var expected = "FileName not provided";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests getFile returns Bad Request when file doesn't exist
        /// </summary>
        [Fact]
        public void GetFile_FileDoesntExist_Return_BadRequest()
        {
            // Arrange
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.GetFile("Test.pdf").Result;
            var expected = "File Doesn't exist";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests getFile returns Bad Request when file doesn't exist
        /// </summary>
        [Fact]
        public void GetFile_ThrowsException_Return_BadRequest()
        {
            this._mockBblobService.Setup(x => x.CheckFileBlobExistsAsync(It.IsAny<string>())).Throws(new UnauthorizedAccessException("Test Exception"));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.GetFile("Test.pdf").Result;
            var expected = "Error downloading file:  Test.pdf";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests GetFileList returns Bad Request when file doesn't exist
        /// </summary>
        [Fact]
        public void GetFileList_ReturnOk()
        {
            IEnumerable<FileDetails> files = new List<FileDetails>();
            this._mockBblobService.Setup(x => x.ListFileBlobsAsync()).Returns(() => Task.FromResult(files));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.GetFileList().Result;            

            // Assert 
            Assert.IsType<OkObjectResult>(actual);            
        }

        /// <summary>
        /// Tests GetFileList returns Bad Request when file doesn't exist
        /// </summary>
        [Fact]
        public void GetFileList_ThrowsException_Return_BadRequest()
        {
            this._mockBblobService.Setup(x => x.ListFileBlobsAsync()).Throws(new UnauthorizedAccessException("Test Exception"));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.GetFileList().Result;
            var expected = "Error getting file list";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests Delete file is successfull.
        /// </summary>
        [Fact]
        public void DeleteFile_Return_Success()
        {
            // Arrange            
            this._mockBblobService.Setup(x => x.CheckFileBlobExistsAsync(It.IsAny<string>())).Returns(true);
            this._mockBblobService.Setup(x => x.DeleteFileBlobAsync(It.IsAny<string>())).Returns(() => Task.FromResult(true));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);


            // Act
            var actual = this._fileController.DeleteFile("Test.pdf").Result;            

            // Assert 
            Assert.IsType<OkObjectResult>(actual);
        }

        /// <summary>
        /// Tests DeleteFile returns Bad Request when no file name provided
        /// </summary>
        [Fact]
        public void DeleteFile_WithNoFileName_Return_BadRequest()
        {
            // Arrange
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.DeleteFile("").Result;
            var expected = "FileName not provided";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Test DeleteFile returns Bad Request when file doesn't exist
        /// </summary>
        [Fact]
        public void DeleteFile_FileDoesntExist_Return_BadRequest()
        {
            // Arrange
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.DeleteFile("Test.pdf").Result;
            var expected = "File Doesn't exist";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Tests Delete file when delete failed
        /// </summary>
        [Fact]
        public void DeleteFile_WhenDeleteFailed_Return_BadRequest()
        {
            // Arrange            
            this._mockBblobService.Setup(x => x.CheckFileBlobExistsAsync(It.IsAny<string>())).Returns(true);
            this._mockBblobService.Setup(x => x.DeleteFileBlobAsync(It.IsAny<string>())).Returns(() => Task.FromResult(false));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);


            // Act
            var actual = this._fileController.DeleteFile("Test.pdf").Result;
            var expected = "Unable to delete file : Test.pdf";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        /// <summary>
        /// Test DeleteFile returns Bad Request when file doesn't exist
        /// </summary>
        [Fact]
        public void DeleteFile_ThrowsException_Return_BadRequest()
        {
            this._mockBblobService.Setup(x => x.CheckFileBlobExistsAsync(It.IsAny<string>())).Throws(new UnauthorizedAccessException("Test Exception"));
            this._fileController = new FileController(this._mockBblobService.Object, this._mockLogger.Object, this._mockFileConfigOptions.Object);

            // Act
            var actual = this._fileController.DeleteFile("Test.pdf").Result;
            var expected = "failed to delete file : Test.pdf ";

            // Assert 
            Assert.IsType<BadRequestObjectResult>(actual);
            Assert.Equal(expected, ((BadRequestObjectResult)actual).Value);
        }

        #region "Setup Tests"

        /// <summary>
        /// Sets up File Config 
        /// </summary>
        private void SetUpFileConfig()
        {
            this._mockFileConfigOptions = new Mock<IOptions<FileConfig>>();

            FileConfig config = new FileConfig()
            {
                MaxFileSizeAllowed = 5242880,
                SupportedTypes = new string[] { "application/pdf" }
            };
            this._mockFileConfigOptions.Setup(x => x.Value).Returns(config);            
        }

        /// <summary>
        /// Creates IFormFile
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <param name="content">the content</param>
        /// <param name="contentType">the contentType</param>
        /// <param name="fileLength">the fileLenght</param>
        /// <returns></returns>
        private IFormFile CreateTestFormFile(string fileName, string content, string contentType, long fileLength)
        {
            byte[] fileBytes = Encoding.UTF8.GetBytes(content);

            var file = new FormFile(new MemoryStream(fileBytes), 0, fileLength, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return file;
        }

        #endregion
    }
}
