using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class AddCommentQuestionDto
    {
        [Required]
        public string Description { get; set; }
    }
}
