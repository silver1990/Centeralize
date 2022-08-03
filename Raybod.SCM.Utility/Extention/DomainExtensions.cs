using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Utility.Extention
{
    public static class DomainExtensions
    {
        public static BaseCustomerDto GetBaseCustomerDto(this List<Customer> customers)
        {
            if (customers != null && customers.Any())
            {
                var temp = customers.FirstOrDefault();
                return new BaseCustomerDto { Address = temp.Address, Email = temp.Email, CustomerCode = temp.CustomerCode, Id = temp.Id, Name = temp.Name, PostalCode = temp.PostalCode, Fax = temp.Fax, TellPhone = temp.TellPhone, Website = temp.Website, Logo = temp.Logo };
            }
            return null;
        }
        public static BaseCustomerDto GetBaseCustomerDto(this List<Consultant> customers)
        {
            if (customers != null && customers.Any())
            {
                var temp = customers.FirstOrDefault();
                return new BaseCustomerDto { Address = temp.Address, Email = temp.Email, CustomerCode = temp.ConsultantCode, Id = temp.Id, Name = temp.Name, PostalCode = temp.PostalCode, Fax = temp.Fax, TellPhone = temp.TellPhone, Website = temp.Website, Logo = temp.Logo };
            }
            return null;
        }
        public static async  Task<string> GetTransmittlNumber(this DbSet<Transmittal> _transmittalRepository, long revisionId, string contractCode)
        {
            var result =await _transmittalRepository.AsNoTracking()
                      .Where(d => (d.TransmittalType == TransmittalType.Customer||d.TransmittalType==TransmittalType.Consultant) &&
                      d.ContractCode == contractCode &&
                      d.TransmittalRevisions.Any(c => c.DocumentRevisionId == revisionId&&c.POI==POI.IFA))
                       .OrderByDescending(e => e.CreatedDate)
                       .Select(v => new
                       {
                           TransmittalNumber = v.TransmittalNumber,
                           TransmittalDate = v.CreatedDate,

                       }).FirstOrDefaultAsync();
            if (result != null)
            {
                var number = (result.TransmittalNumber!=null)?result.TransmittalNumber:"";
                var date = (result.TransmittalDate != null) ? result.TransmittalDate.ToPersianDateString() : "";
                return number+","+date;
            }
            return "";
        }

        public static async Task<string> GetTransmittlDate(this DbSet<Transmittal> _transmittalRepository, long revisionId, string contractCode)
        {
            var result =await _transmittalRepository.AsNoTracking()
                    .Where(d => (d.TransmittalType == TransmittalType.Customer||d.TransmittalType==TransmittalType.Consultant) &&
                    d.ContractCode == contractCode &&
                    d.TransmittalRevisions.Any(c => c.DocumentRevisionId == revisionId))
                     .OrderByDescending(e => e.CreatedDate)
                     .Select(v => new
                     {
                         TransmittalDate = v.CreatedDate,

                     }).FirstOrDefaultAsync();
            if (result != null && result.TransmittalDate != null)
            {
                return result.TransmittalDate.ToPersianDateString();
            }
            return "";
        }
    }
}
