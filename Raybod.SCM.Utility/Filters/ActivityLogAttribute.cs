using System;

namespace Raybod.SCM.Utility.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ActivityLogAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
