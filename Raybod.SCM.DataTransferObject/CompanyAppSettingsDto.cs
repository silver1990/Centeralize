using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class CompanyAppSettingsDto
    {
        public string WepApiHost { get; set; }
        public string ClientHost { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyLogoFront { get; set; }
        public string CompanyNameFA { get; set; }
        public string CompanyNameEN { get; set; }
        public string PowerBIRoot { get; set; }

        public List<string> RemoteIPs { get; set; }
        public List<string> ReportExceptionEmailCC { get; set; }
        public string ReportExceptionEmailTo { get; set; }
        public bool IsCheckedIP { get; set; }
        public CompanyAppSettingsDto()
        {
            RemoteIPs = new List<string>();
            ReportExceptionEmailCC = new List<string>();
        }
    }
}
