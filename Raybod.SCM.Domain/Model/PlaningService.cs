using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class PlanService
    {
        public Guid Id { get; set; }
        public string Reference { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool DocumentManagement { get; set; }
        public bool PurchaseManagement { get; set; }
        public bool ConstructionManagement { get; set; }
        public bool FileDrive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
    }
}
