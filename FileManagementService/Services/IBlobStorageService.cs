using FileManagementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementService.Services
{
    /// <summary>
    /// the BlobStorageService interface
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads file to Azure Blob Storage asynchronously 
        /// </summary>        
        /// <param name="file">the file</param>        
        /// <returns>file path</returns>
        Task<string> UploadFileBlobAsync(FileDetails file);

        /// <summary>
        /// Checks if file already exists
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>true or false</returns>
        bool CheckFileBlobExistsAsync(string fileName);

        /// <summary>
        /// Get File from Blob Storage
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>the file</returns>
        Task<FileDetails> GetFileBlobAsync(string fileName);

        /// <summary>
        /// Get the list of files asynchronously 
        /// </summary>
        /// <returns>the list of files</returns>
        Task<IEnumerable<FileDetails>> ListFileBlobsAsync();

        /// <summary>
        /// Delete File from Blob Storage
        /// </summary>
        /// <param name="fileName">the fileName</param>
        /// <returns>true or false</returns>
        Task<bool> DeleteFileBlobAsync(string fileName);

        /// <summary>
        /// Reorders files on Blob based on provided list 
        /// </summary>
        /// <param name="files">the files</param>
        /// <returns>true or false</returns>
        Task<bool> ReOrderFileBlobsAsync(IEnumerable<FileDetails> files);
    }
}
