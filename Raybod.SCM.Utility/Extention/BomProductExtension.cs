using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataTransferObject.MasterMrpReport;
using Raybod.SCM.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raybod.SCM.Utility.Extention
{
    public static class BomProductExtension
    {
        public static List<MasterMrAreaDto> GetArea(this IEnumerable<MasterMrAreaDto> firstLevel, IEnumerable<MasterMrAreaDto> secondLevel)
        {
            List<MasterMrAreaDto> resutl = new List<MasterMrAreaDto>();
            if (firstLevel != null&&firstLevel.Any())
            {
                resutl.AddRange(firstLevel.Where(a=>a.AreaId>0 && !resutl.Any(b => b.AreaId == a.AreaId)));
                if (secondLevel != null && secondLevel.Any())
                {
                    foreach (var item in secondLevel)
                    {
                        if ((!resutl.Any(a => a.AreaId == item.AreaId)) && item.AreaId != 0)
                        {
                            resutl.Add(item);
                        }
                    }
                }
                
            }
            else if (secondLevel != null && secondLevel.Any())
            {
                resutl.AddRange(secondLevel.Where(a => a.AreaId > 0&&!resutl.Any(b=>b.AreaId==a.AreaId)));
            }
            return resutl;
        }
    }
}
