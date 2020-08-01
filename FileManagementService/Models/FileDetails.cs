using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementService.Models
{
    /// <summary>
    /// The File class 
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// Gets or sets the FileName
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the contentType 
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///  Gets or sets FileLength
        /// </summary>
        public long? FileLength { get; set; }

        /// <summary>
        /// Gets or sets the file stream
        /// </summary>
        public byte[] Content { get; set; }
    }
}
