using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public sealed class UploadedFile
    {
        public UploadedFile(Guid uploadContext,string fileName,string contentType,Stream fileData)
        {
            FileName = fileName;
            ContentType = contentType;
            FileData = fileData;
            UploadContext = uploadContext;
        }
        /// <summary>
        /// The name of the file being uploaded
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// The files content type such as image\gif, text\plain, etc
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// The binary data of the file
        /// </summary>
        public Stream FileData { get; private set; }
        /// <summary>
        /// A value used to identify the file uploaded in the system. User defined.
        /// </summary>
        public Guid UploadContext { get; set; }
    }

    public class FileUploadProgress
    {
        public FileUploadProgress()
        {
            PercentComplete = 0;
        }
        /// <summary>
        /// Teh current percentage of the file uploaded
        /// </summary>
        public int PercentComplete { get; internal set; }
        /// <summary>
        /// The identifier of the file being reported on
        /// </summary>
        public Guid? UploadContext { get; internal set; }
        //TODO:add filename
    }

    public class UploadedFileInfo
    {
        public UploadedFileInfo(Guid uploadContext)
        {
            IsError = false;
            UploadContext = uploadContext;
        }
        //TODO:Add filename
        /// <summary>
        /// The identifier of the file being reported on
        /// </summary>
        public Guid UploadContext { get;private set; }
        /// <summary>
        /// Indicates whether or not the upload encoutered and error.
        /// </summary>
        public bool IsError { get; internal set; }
        /// <summary>
        /// If there was an error a message describing it
        /// </summary>
        public string Message { get; internal set; }
    }
}
