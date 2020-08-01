using Azure.Storage.Blobs;
using FileManagementService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Text;
using FileManagementService.Helpers;

namespace FileManagementService.Services
{
    /// <summary>
    /// The BlobStorage Service class
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        /// <summary>
        ///  The fileconfig 
        /// </summary>
        private readonly IOptions<FileConfig> _fileConfig;

        /// <summary>
        /// The BlobCContainer client
        /// </summary>
        private BlobContainerClient _blobContainerClient;

        /// <summary>
        /// Creates a new instance of <see cref="BlobStorageService"/>
        /// </summary>        
        /// <param name="fileConfig"> </param>
        public BlobStorageService(IOptions<FileConfig> _fileConfig)
        {
            try
            {
                this._fileConfig = _fileConfig;

                // Creates a new Blob Container
                this.CreateBlobContainer();
            }
            catch (Exception ex)
            {
                // Preserve and throw original stack trace for error tracking and debugging purposes. 
                throw ex;
            }
        }

        /// <summary>
        /// Checks if file already exists
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>true or false</returns>
        public bool CheckFileBlobExistsAsync(string fileName)
        {
            bool fileExists = false;
            try
            {                
                var blobClient = this._blobContainerClient.GetBlobClient(fileName);
                if (blobClient != null)
                {
                    fileExists = true;
                }
            }
            catch (Exception ex)
            {
                // Preserve and throw original stack trace for error tracking and debugging purposes. 
                throw ex;
            }
            return fileExists;
        }

        /// <summary>
        /// Delete File from Blob Storage
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>true or false</returns>
        public async Task<bool> DeleteFileBlobAsync(string fileName)
        {
            try
            {
                var blobClient = this._blobContainerClient.GetBlobClient(fileName);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Preserve and throw original stack trace for error tracking and debugging purposes. 
                throw ex;
            }            
        }

        /// <summary>
        /// Get File from Blob Storage
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>the file</returns>
        public async Task<FileDetails> GetFileBlobAsync(string fileName)
        {
            try 
            {
                var blobClient = this._blobContainerClient.GetBlobClient(fileName);
                var blobdownloadInfo = await blobClient.DownloadAsync();

                FileDetails file = new FileDetails()
                {
                    FileName = fileName,
                    Content = FileHelper.ReadFileContent(blobdownloadInfo.Value.Content),
                    ContentType = blobdownloadInfo.Value.ContentType
                };

                return file;
            }
            catch (Exception ex)
            {
                // Preserve and throw original stack trace for error tracking and debugging purposes. 
                throw ex;
            }
        }

        /// <summary>
        /// Get the list of files asynchronously 
        /// </summary>
        /// <returns>the list of files</returns>
        public async Task<IEnumerable<FileDetails>> ListFileBlobsAsync()
        {            
            try 
            {
                var files = new List<FileDetails>();

                await foreach (var blobItem in this._blobContainerClient.GetBlobsAsync())
                {
                    files.Add(new FileDetails()
                    {
                        FileName = blobItem.Name,
                        ContentType = blobItem.Properties.ContentType,
                        FileLength = blobItem.Properties.ContentLength
                    });
                }

                return files;
            }
            catch (Exception ex)
            {
                // Preserve and throw original stack trace for error tracking and debugging purposes. 
                throw ex;
            }
        }

        /// <summary>
        /// Delete File from Blob Storage
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>true or false</returns>
        public Task<bool> ReOrderFileBlobsAsync(IEnumerable<FileDetails> files)
        {
            // Read all the files info and reorder them based on provided list.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uploads file to Azure Blob Storage asynchronously 
        /// </summary>        
        /// <param name="file">the file</param>        
        /// <returns>file path</returns>
        public async Task<string> UploadFileBlobAsync(FileDetails file)
        {
            try 
            {
                var blobClient = this._blobContainerClient.GetBlobClient(file.FileName);

                await blobClient.UploadAsync(new MemoryStream(file.Content), new BlobHttpHeaders { ContentType = file.ContentType });
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                // Preserve and throw original stack trace for error tracking and debugging purposes. 
                throw ex;
            }
        }

        /// <summary>
        /// Create Container in Azure Blob storage
        /// </summary>
        private async void CreateBlobContainer()
        {
            // Create blob Container if not already exists 
            this._blobContainerClient = new BlobContainerClient(_fileConfig.Value.BlobStorageConfig.ConnectionString, _fileConfig.Value.BlobStorageConfig.ContainerName);
            await this._blobContainerClient.CreateIfNotExistsAsync();


            /* Another way to create Blob container
             
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(_fileConfig.Value.BlobStorageConfig.ConnectionString);

            //Create a unique name for the container
            string containerName = _fileConfig.Value.BlobStorageConfig.ContainerName + Guid.NewGuid().ToString();            

            // Create the container and return a container client object
            this._blobContainerClient= await blobServiceClient.CreateBlobContainerAsync(containerName);
            */
        }
    }
}
