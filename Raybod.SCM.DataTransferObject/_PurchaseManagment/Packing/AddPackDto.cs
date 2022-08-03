using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class AddPackDto
    {
        public List<AddPackSubjectDto> PackSubjects { get; set; }

        public List<AddAttachmentDto> Attachments { get; set; }
    }
}
