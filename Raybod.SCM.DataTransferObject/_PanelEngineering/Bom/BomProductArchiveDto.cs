using Raybod.SCM.DataTransferObject.MasterMrpReport;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Bom
{
   public class BomProductArchiveDto
    {

        public MrpReportDto Mrps { get; set; }
        public PurchaseRequestReportDto PurchaseRequests { get; set; }
        public RFPReportDto RFPs { get; set; }
        public PrContractReportDto PrContracts { get; set; }
        public POReportDto Pos { get; set; }



    }
    public class MrpReportDto
    {
        public decimal Quantity { get; set; }
        public List<MrpReportListDto> MrpList { get; set; }
        public MrpReportDto()
        {
            MrpList = new List<MrpReportListDto>();
        }
    }
    public class PurchaseRequestReportDto
    {
        public decimal Quantity { get; set; }
        public List<PRReportListDto> PurchaseRequestList { get; set; }
        public PurchaseRequestReportDto()
        {
            PurchaseRequestList = new List<PRReportListDto>();
        }
    }
    public class RFPReportDto
    {
        public decimal Quantity { get; set; }
        public List<RFPReportListDto> RFPList { get; set; }
        public RFPReportDto()
        {
            RFPList = new List<RFPReportListDto>();
        }
    }
    public class PrContractReportDto
    {
        public decimal Quantity { get; set; }
        public List<PRCReportListDto> PrContractList { get; set; }
        public PrContractReportDto()
        {
            PrContractList = new List<PRCReportListDto>();
        }
    }
    public class POReportDto
    {
        public decimal Quantity { get; set; }
        public List<POReportListDto> PoList { get; set; }
        public POReportDto()
        {
            PoList = new List<POReportListDto>();
        }
    }
}
