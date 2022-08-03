namespace Raybod.SCM.DataTransferObject.PO.POActivity
{
    public class ResultAddActivityTimeSheetDto : ListActivityTimeSheetDto
    {
        public string TotalDuration { get; set; }
        public double ActivityProgressPercent { get; set; }
        public double PoProgressPercent { get; set; }

    }
}
