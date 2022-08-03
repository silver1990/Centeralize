using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddRFPSupplierEvaluationDto
    {
        [MaxLength(800)]
        public string EvaluationNote { get; set; }

        public List<AddProposalEvaluationScoreDto> ProposalEvaluationScore { get; set; }
    }
}
