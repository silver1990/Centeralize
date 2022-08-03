using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{ 
    public class PoProgress:BaseEntity
    {
        public long Id { get; set; }

        public long POId { get; set; }

        public double ProgressPercent { get; set; }


        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }
    }
}
