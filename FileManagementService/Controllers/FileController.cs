using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileManagementService.Helpers;
using FileManagementService.Models;
using FileManagementService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileManagementService.Controllers
{
    /// <summary>
    /// The File API Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        /// <summary>
        /// The Logger 
        /// </summary>
        private readonly ILogger<FileController> _logger;

        /// <summary>
        /// The BlobStorage Service
        /// </summary>
        private readonly IBlobStorageService _blobService;

        /// <summary>
        /// The BlobStorage config 
        /// </summary>
        private readonly IOptions<FileConfig> _fileConfig;

        /// <summary>
        /// Creates a new instance of <see cref="FileController"/> API 
        /// </summary>
        /// <param name="blobService"></param>
        /// <param name="logger"></param>
        /// <param name="fileConfig"></param>
        public FileController(IBlobStorageService blobService, ILogger<FileController> logger, IOptions<FileConfig> fileConfig)
        {
            this._blobService = blobService;
            this._logger = logger;
            this._fileConfig = fileConfig;
        }

        /// <summary>
        /// Http Get Method for file request
        /// </summary>
        /// <returns>the request file if exists</returns>
        [ProducesResponseType(typeof(FileContentResult),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return BadRequest("FileName not provided");

                if (this._blobService.CheckFileBlobExistsAsync(fileName))
                {
                    var file = await this._blobService.GetFileBlobAsync(fileName);

                    return new FileContentResult(file.Content, file.ContentType);
                }
                else
                {
                    return BadRequest("File Doesn't exist");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error downloading file:  {fileName}";
                this._logger.LogError(ex, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        /// <summary>
        /// Http Post method to upload pdf file to Azure Blob Storage
        /// </summary>
        /// <param name="file">the file</param>
        /// <returns>uploaded file Absolute URI</returns>
        [Route("uploadfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                    ModelState.AddModelError("NoFile", "file not uploaded");

                if (file.Length > this._fileConfig.Value.MaxFileSizeAllowed)
                    ModelState.AddModelError("FileSizeTooBig", $"File size is bigger than maximum allowed file size {this._fileConfig.Value.MaxFileSizeAllowed}");


                if (!this._fileConfig.Value.SupportedTypes.Contains(file.ContentType))
                    ModelState.AddModelError("InvalidFileType", $"Input file type is not supported");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                FileDetails fileDetails = new FileDetails()
                {
                    FileName = Path.GetFileName(file.FileName),
                    FileLength = file.Length,
                    ContentType = file.ContentType
                };

                using (var target = new MemoryStream())
                {
                    file.CopyTo(target);
                    fileDetails.Content = target.ToArray();
                }

                var fileLocation = await this._blobService.UploadFileBlobAsync(fileDetails);

                return Ok(fileLocation);
            }
            catch (Exception ex)
            {
                var errorMessage = $"failed to upload file : {file.FileName} ";
                this._logger.LogError(ex, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        /// <summary>
        /// Returns the list of files stored in storage.
        /// </summary>
        [HttpGet]
        [Route("list")]
        [ProducesResponseType(typeof(IEnumerable<FileDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFileList()
        {
            try
            {
                return Ok(await _blobService.ListFileBlobsAsync());
            }
            catch (Exception ex)
            {
                var errorMessage = "Error getting file list";
                _logger.LogError(ex,$"Message: {errorMessage}");
                return BadRequest(errorMessage);
            }
        }

        /// <summary>
        /// Delete file from Storage 
        /// </summary>
        /// <param name="fileName">the fileName</param>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return BadRequest("FileName not provided");

                if (this._blobService.CheckFileBlobExistsAsync(fileName))
                {
                    if (await this._blobService.DeleteFileBlobAsync(fileName))
                        return Ok($"File : {fileName} Deteled successfully");
                    else
                        return BadRequest($"Unable to delete file : {fileName}");                    
                }
                else
                {
                    return BadRequest("File Doesn't exist");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"failed to delete file : {fileName} ";
                this._logger.LogError(ex, errorMessage);
                return BadRequest(errorMessage);
            }
        }

        /// <summary>
        /// Re-Order files based on provided files  
        /// </summary>
        /// <param name="files">the files</param>
        /// <returns>true or false</returns>
        [HttpPatch]
        public async Task<IActionResult> ReorderFiles(IEnumerable<string> files)
        {
            try
            {
                if (files != null && files.Count() > 0)
                    return BadRequest("File list is empty");

                List<FileDetails> _files = files.Select(a => new FileDetails()
                {
                    FileName = a.ToString()
                }).ToList();

                return Ok(await this._blobService.ReOrderFileBlobsAsync(_files));
            }
            catch (Exception ex)
            {
                var errorMessage = "unable to re-Order files";
                this._logger.LogError(ex, errorMessage);
                return BadRequest(errorMessage);
            }
        }
    }
}
