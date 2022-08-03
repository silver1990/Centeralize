using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class PackDetailsDto
    {
        public List<WaitingPOSubjectDto> PackSubjects { get; set; }

        public List<PackingAttachmentsDto> Attachments { get; set; }
    }
}
