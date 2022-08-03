using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
    public class ActivityTimeSheetDto
    {
        public double ActivityProgressPercent { get; set; }
        public double OperationProgressPercent { get; set; }
        public string TotalDuration { get; set; }
    }
}
