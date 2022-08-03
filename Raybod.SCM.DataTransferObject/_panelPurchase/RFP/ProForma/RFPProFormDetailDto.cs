using Raybod.SCM.DataTransferObject.Supplier;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPProFormDetailDto
    {
        public long ProFormId { get; set; }
        public string Duration { get; set; }
        public string Price { get; set; }
        public long RFPSupplierId { get; set; }
        public List<RFPProFormaAttachmentDto> Attachments { get; set; }
    }

    public class RFPSupplierProFormDetailDto
    {
        public int SupplierId { get; set; }

        [Display(Name = "کد تامین کننده")]
        public string SupplierCode { get; set; }

        [Display(Name = "نام شرکت")]
        public string Name { get; set; }

        public List<string> ProductGroups { get; set; }

        public string TellPhone { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "لوگو")]
        public string Logo { get; set; }
        public long ProFormId { get; set; }
        public string Duration { get; set; }
        public string Price { get; set; }
        public long RFPSupplierId { get; set; }
        public bool IsWinner { get; set; }
        public List<RFPProFormaAttachmentDto> Attachments { get; set; }
    }

    public class RFPSupplierProFormListDto
    {
        public List<int> Winners { get; set; }
        public List<RFPSupplierProFormDetailDto> RFPSupplierProForma { get; set; }
        public RFPSupplierProFormListDto()
        {
            Winners = new List<int>();
            RFPSupplierProForma = new List<RFPSupplierProFormDetailDto>();
        }
    }
}
