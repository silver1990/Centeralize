using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POActivity
{
    public class ActivityTimeSheetDto
    {
        public double ActivityProgressPercent { get; set; }
        public double PoProgressPercent { get; set; }
        public string TotalDuration { get; set; }
    }
}
