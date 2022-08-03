using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddConfirmationAnswerDto
    {
        [MaxLength(800)]
        public string Note { get; set; }

        public bool IsAccept { get; set; }
    }
}
