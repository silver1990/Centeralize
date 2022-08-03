using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Utility.EnumType;
using System.Collections.Generic;
using System.Linq;

namespace Raybod.SCM.Utility.Common
{
    public static class CodeGenerator
    {
        public static string GroupCodeGenerator(List<string> previouseCode, string parentCode)
        {
            parentCode = parentCode.Trim();
            List<string> num = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            if (string.IsNullOrEmpty(parentCode))
            {
                if (previouseCode == null || previouseCode.Count == 0)
                {
                    return "1";
                }
                else
                {
                    var exceptionList = num.Where(x => !previouseCode.Contains(x)).ToList();
                    return exceptionList.First();
                }
            }

            if (previouseCode == null || previouseCode.Count == 0)
            {
                return $"{parentCode}01";
            }
            else
            {
                var lastCode = previouseCode.OrderByDescending(x => x).First();
                return (int.Parse(lastCode) + 1).ToString();
            }

        }

        public static string ProductCodeGenerator(int countProduct, string groupCode)
        {

            int length = groupCode.Length + countProduct.ToString().Length;
            string minimum = "";
            for (int i = 1; i <= (12 - length); i++)
            {
                minimum += "0";
            }
            return $"{groupCode}{minimum}{++countProduct}";

        }

        public static string DocumentTransmittalAndCommunicationCodeGenerator(int communicationCount, string form, string contractCode)
        {

            int length = (communicationCount+1).ToString().Length;
            string minimum = "";
            for (int i = 1; i <= (4 - length); i++)
            {
                minimum += "0";
            }
            return $"{contractCode}-{form}-{minimum}{++communicationCount}";

        }

        public static string SCMFormCodeGenerator(long tblCount, string form, string contractCode)
        {
            string code = string.Empty;

            code = $"{contractCode}-{form}-{++tblCount}";

            return code;
        }

        public static string RevisionFormCodeGenerator(long tblCount)
        {
            string code = string.Empty;

            if (tblCount == 0)
                code = $"0{tblCount}";
            else if ((tblCount / 10) >= 1)
                code = $"{tblCount}";
            else
                code = $"0{tblCount}";

            return code;
        }
    }
}
