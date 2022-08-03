using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class LastRevisionDto
    {
        public long RevisionId { get; set; }

        public string RevisionCode { get; set; }

        public RevisionStatus RevisionStatus { get; set; }
    }
}
