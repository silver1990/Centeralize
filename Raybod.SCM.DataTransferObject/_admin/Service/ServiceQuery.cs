﻿using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.Service
{
    public  class ServiceQuery : IQueryObject
    {
        public string SearchText { get; set; }        

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;
    }
}
