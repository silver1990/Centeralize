using Raybod.SCM.DataTransferObject.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Utility.Utility
{
    public class ProductInfoEqualityCompare : IEqualityComparer<ProductMiniInfo>
    {
        #region IEqualityComparer<Contact> Members

        public bool Equals(ProductMiniInfo x, ProductMiniInfo y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(ProductMiniInfo obj)
        {
            return obj.ProductCode.GetHashCode();
        }

        #endregion
    }
}
