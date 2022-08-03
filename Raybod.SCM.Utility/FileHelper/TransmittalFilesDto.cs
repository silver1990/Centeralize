using Raybod.SCM.DataTransferObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Utility.FileHelper
{
    public class TransmittalFilesDto
    {
        public List<InMemoryFileDto> RevisionFiles { get; set; }
        public DownloadFileDto TransmitallFile { get; set; }
        public TransmittalFilesDto()
        {
            RevisionFiles =new List<InMemoryFileDto>();
        }
    }
}
