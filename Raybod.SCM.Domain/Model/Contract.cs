using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class Contract : BaseEntity
    {
        [Key]
        [Required]
        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }


        [MaxLength(64)]
        public string ContractNumber { get; set; }

        [MaxLength(800)]
        public string Description { get; set; }

        public string ParnetContractCode { get; set; }

        public string Details { get; set; }


        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Cost { get; set; }

        public ContractStatus ContractStatus { get; set; }

        public ContractType ContractType { get; set; }

  
        public DateTime? DateIssued { get; set; }


        public DateTime? DateEffective { get; set; }


        public DateTime? DateEnd { get; set; }

        public int? ContractDuration { get; set; }
        public int? ConsultantId { get; set; }
        public int? CustomerId { get; set; }
        public bool DocumentManagement { get; set; }
        public bool PurchaseManagement { get; set; }
        public bool ConstructionManagement { get; set; }
        public bool FileDrive { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ParnetContractCode))]
        public Contract ParnetContract { get; set; }

        [ForeignKey(nameof(ConsultantId))]
        public Consultant Consultant { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }


        public ICollection<Contract> AddendumContracts { get; set; }


        public virtual ICollection<ContractAttachment> Attachments { get; set; }


        public virtual ICollection<SCMAuditLog> SCMAuditLogs { get; set; }


        public virtual ICollection<Mrp> Mrps { get; set; }

        public virtual ICollection<MasterMR> MasterMRs { get; set; }

        //public virtual ICollection<Document> Documents { get; set; }


        public virtual ICollection<Document> Documents { get; set; }

        public virtual ICollection<PurchaseRequest> PurchaseRequests { get; set; }

        public virtual ICollection<ContractFormConfig> ContractFormConfigs { get; set; }

        public virtual ICollection<Transmittal> Transmittals { get; set; }

        public virtual ICollection<PO> POs { get; set; }

        public virtual ICollection<RFP> RFPs { get; set; }

        public virtual ICollection<PendingForPayment> PendingForPayments { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<WarehouseOutputRequest> WarehouseOutputRequests { get; set; }
        public virtual ICollection<WarehouseDespatch> WarehouseDespatches { get; set; }


        public virtual TeamWork TeamWork { get; set; }
    }
}
