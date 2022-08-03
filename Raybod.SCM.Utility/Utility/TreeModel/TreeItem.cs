using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Utility.Utility.TreeModel
{
    public class TreeItem<T>
    {
        public T Directory { get; set; }
        public IEnumerable<TreeItem<T>> Children { get; set; }
    }
}
