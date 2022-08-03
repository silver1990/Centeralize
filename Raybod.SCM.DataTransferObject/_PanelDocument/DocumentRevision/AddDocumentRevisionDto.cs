using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddDocumentRevisionDto
    {

        [MaxLength(300)]
        public string Reason { get; set; }

        public long? DateEnd { get; set; }

        //public List<AddRevisionActivityDto> RevisionActivities { get; set; }

        //public List<string> RevisionAttachments { get; set; }
    }
}
