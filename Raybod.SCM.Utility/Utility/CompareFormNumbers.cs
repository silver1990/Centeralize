using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Raybod.SCM.Utility.Utility
{
    public class CompareFormNumbers : IComparer<string>
    {
        public int Compare([AllowNull] string x, [AllowNull] string y)
        {
            if (String.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)) return 1;
            if (String.IsNullOrEmpty(y) && !string.IsNullOrEmpty(x)) return -1;
            if (String.IsNullOrEmpty(y) && string.IsNullOrEmpty(x)) return 0;
            if (ConvertToInt(x) > ConvertToInt(y)) return 1;
            if (ConvertToInt(x) < ConvertToInt(y)) return -1;
            return 0;
        }
        private int ConvertToInt(string communicationCode)
        {
            int i = 0;
            for (i = communicationCode.Length - 1; communicationCode[i] <= '9' && communicationCode[i] >= '0'; i--) ;
            return (Convert.ToInt32(communicationCode.Substring(i + 1)));
        }
    }
}
