using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddProposalEvaluationScoreDto
    {
        public long Id { get; set; }

        public decimal EvaluationScore { get; set; }

    }
}
