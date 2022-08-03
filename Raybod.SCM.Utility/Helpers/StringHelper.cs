using System.Collections.Generic;
using System.Linq;

namespace Raybod.SCM.Utility.Helpers
{
    public static class StringHelper
    {
        public static List<string> SplitHelper(this string str)
        {
            return str.Split(",")
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToList();
        }
    }
}
