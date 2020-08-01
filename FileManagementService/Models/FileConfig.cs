using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementService.Models
{
    /// <summary>
    /// The File Config
    /// </summary>
    public class FileConfig
    {
        /// <summary>
        /// The BlobStorage Config Model 
        /// </summary>
        public BlobStorageConfig BlobStorageConfig { get; set; }   

        /// <summary>
        /// Gets or sets max file size
        /// </summary>
        public int MaxFileSizeAllowed { get; set; }

        /// <summary>
        /// Gets or sets Supported file types
        /// </summary>
        public string[] SupportedTypes { get; set; }        
    }

    /// <summary>
    /// The BlobStorage Config class
    /// </summary>
    public class BlobStorageConfig
    {
        /// <summary>
        /// Gets or sets Connectionstring of Azure BlobStorage Config from appsettings.json file
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// gets or sets Containername from appsettings.json file
        /// </summary>
        public string ContainerName { get; set; }
    }
}
