using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raybod.SCM.Utility.Utility.TreeModel
{
    public static class GenericTreeMaker
    {
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
        this IEnumerable<T> collection,
        Func<T, K> id_selector,
        Func<T, K> parent_id_selector,
        K root_id = default(K))
        {
            foreach (var c in collection.Where(c => parent_id_selector(c).Equals(root_id)))
            {
                var temp = c as FileDriveDirectory;
                yield return new TreeItem<T>
                {
                   
                    Directory = c,
                    Children = collection.GenerateTree(id_selector, parent_id_selector, id_selector(c))
                };
            }
        }

        
            
        

    }
}
