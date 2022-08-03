using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.Utility.EnumType
{
    public enum LicenseStatus
    {
        [Display(Name = "عملیات با موفقیت انجام شد")]
        OprationSuccess = 0,
        [Display(Name = "خطای سیستمی رخ داده است!")]
        ServerError = -1,
        [Display(Name = "زمان استفاده شما از سیستم به پایان رسیده است!")]
        LicenseExpire = -2,
        [Display(Name = "اطلاعات سیستم شما با اطلاعات ثبت شده مغایرت دارد!")]
        IpAndPortNotValid = -3,
        [Display(Name = "اعتبار سنجی شما با مشکل مواجه شده است!")]
        LicenseNotValid = -4,
    }
}
