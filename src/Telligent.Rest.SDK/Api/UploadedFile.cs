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
        public string FileName { get; private set; }
        public string ContentType { get; private set; }
        public Stream FileData { get; private set; }
        public Guid UploadContext { get; set; }
    }

    public class FileUploadProgress
    {
        public FileUploadProgress()
        {
            PercentComplete = 0;
        }
        public int PercentComplete { get; internal set; }
        public Guid? UploadContext { get; internal set; }
    }

    public class UploadedFileInfo
    {
        public UploadedFileInfo(Guid uploadContext)
        {
            IsError = false;
            UploadContext = uploadContext;
        }
        public Guid UploadContext { get;private set; }
        public bool IsError { get; internal set; }
        public string Message { get; internal set; }
    }
}
