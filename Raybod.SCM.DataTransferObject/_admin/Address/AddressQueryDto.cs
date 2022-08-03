using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Address
{
    public class AddressQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 1500;

        public AddressType AddressType { get; set; }

        public string DeliveryLocation { get; set; }

        [Display(Name = "شناسه شرکت")]
        public int? CompanyId { get; set; }


    }
}
