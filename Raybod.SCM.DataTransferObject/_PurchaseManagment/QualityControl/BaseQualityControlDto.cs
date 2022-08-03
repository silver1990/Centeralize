using System.ComponentModel.DataAnnotations;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.QualityControl
{
    public class BaseQualityControlDto 
    {
        [MaxLength(800, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Note { get; set; }
        
        public QCResult QCResult { get; set; }

    }
}