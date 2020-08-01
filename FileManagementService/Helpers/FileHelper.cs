using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementService.Helpers
{
    /// <summary>
    /// The FileHelper static class
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Returns byte array for provide stream content
        /// </summary>
        /// <param name="input">the input stream</param>
        /// <returns>byte array</returns>
        public static byte[] ReadFileContent(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
