using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject
{
    public class MiniSearchResult
    {
        public long Id { get; set; }

        public string Code { get; set; }

        /// <summary>
        /// کد مرجع
        /// </summary>
        public string RefCode { get; set; }

        public string Description { get; set; }

        public string DescriptionEn { get; set; }

        public string SearchIn { get; set; }

    }
}
