using Microsoft.AspNetCore.Mvc.ModelBinding;
using Raybod.SCM.DataTransferObject.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Utility.Extention
{
    public static class CheckModelStateValid
    {
        public static ModelStateDictionary IsModelStateValid(this AddUserDto model)
        {
            ModelStateDictionary result = new ModelStateDictionary();
            if (String.IsNullOrEmpty(model.FirstName))
                result.AddModelError("FirstName", "الزامی می باشد");
            if(!String.IsNullOrEmpty(model.FirstName)&&(model.FirstName.Length>50||model.FirstName.Length<2))
                result.AddModelError("FirstName", $"حداقل مقدار برای فیلد {2} و حداکثر {50} می باشد.");
            if (String.IsNullOrEmpty(model.LastName))
                result.AddModelError("LastName", "الزامی می باشد");
            if (!String.IsNullOrEmpty(model.LastName) && (model.LastName.Length > 100 || model.LastName.Length < 2))
                result.AddModelError("LastName", $"حداقل مقدار برای فیلد {2} و حداکثر {100} می باشد.");
            if (!String.IsNullOrEmpty(model.UserName) && (model.UserName.Length > 100 || model.UserName.Length < 2))
                result.AddModelError("UserName", $"حداقل مقدار برای فیلد {5} و حداکثر {50} می باشد.");
            if (String.IsNullOrEmpty(model.Email))
                result.AddModelError("Email", "الزامی می باشد");
            if (!String.IsNullOrEmpty(model.Email))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(model.Email);
                }
                catch(Exception ex)
                {
                    result.AddModelError("Email", "آدرس ایمیل اشتباه وارد شده است");
                }
            }
            if (!String.IsNullOrEmpty(model.Image) && (model.Image.Length >300))
                result.AddModelError("UserName", $"حداکثر مقدار برای فیلد {300} می باشد.");
            if (!String.IsNullOrEmpty(model.Password) && (model.Password.Length > 60||model.Password.Length<6))
                result.AddModelError("UserName", $"حداقل مقدار برای فیلد {6} و حداکثر {60} می باشد.");
            return result;
        }
    }
}
