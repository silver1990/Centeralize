using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.Domain.Enum
{
    public enum ModuleType
    {
        [Description("مدیریت مدارک")]
        [Display(Name = "Document management")]
        Documents =1,
        [Description("مدیریت زنجیره تامین")]
        [Display(Name = "Procurement Management")]
        Procurement = 2,
        [Description("مدیریت اجرا")]
        [Display(Name = "Construction Management")]
        Construction = 3
    }
}
