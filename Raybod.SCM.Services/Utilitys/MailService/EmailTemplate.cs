namespace Raybod.SCM.Services.Utilitys.MailService
{
    public static class EmailTemplate
    {
        //public static string UserCommentMentionNotificationEmailTemplate(string userFullName, string formName, string rfpCode)
        //{
        //    string html = $"<body> <br><br>";
        //    html += "<table width='100%' style=' background-color: #e2e7ed; padding: 40px; font-family:Tahoma,Geneva,Verdana,sans-serif; font-size: 14px; font-weight: 300; color: #4A4A4A;'>";
        //    html += "<tr>";
        //    html += "<td colspan='3'  style='padding: 28px 0 20px 0;'>";
        //    html += "<div dir='rtl' style='text-align:right'>";
        //    html += $"<h3 dir='rtl'>کاربر محترم سلام.</h3>";
        //    html += $"<p>{userFullName}، در پرسش و پاسخ {formName} {rfpCode} به شما اشاره شده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید.</p>";
        //    html += "</div>";
        //    html += "</td>";
        //    html += "</tr>";
        //    html += "</table> <br><br>";

        //    html += "<center> <img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' width='80' height='60'><br><br><span style ='font-size: 12px; font-weight: 300; color: #4A4A4A;'>© 2020 Raybod Ravesh, Inc. All rights";
        //    html += " Reserved.</span>";
        //    html += "</center><br><br><br>";
        //    html += "</body>";

        //    return html;
        //}

        public static string UserCommentMentionNotificationEmailTemplate(string userFullName, string supplierName, string formName, string formCode, string linkUrl)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += $"<p style='margin:15px 0;'>{userFullName}، در پرسش و پاسخ {formName} {formCode} شرکت {supplierName} به شما اشاره کرده است. لطفا با مراجعه به آن بخش نظرات خود را به اشتراک بگذارید.</p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            html += "color: white;'> مشاهده پیام";
            html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }




        public static string UserMentionRevisionCommentEmailTemplate(string docTitle, string docNumber, string revisionCode, string linkUrl)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += $"<p style='margin:15px 0;'>به نام شما در ویرایش {revisionCode} تهیه مدرک {docTitle} به شماره {docNumber} اشاره شده است.برای مشاهده و پاسخ پیام رو دکمه زیر کلیک کنید.</p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            html += "color: white;'> مشاهده پیام";
            html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }

        public static string UserMentionCommunicationEmailTemplate(string description, string linkUrl)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += $"<p style='margin:15px 0;'>به نام شما در {description} اشاره شده است.برای مشاهده و پاسخ پیام رو دکمه زیر کلیک کنید.</p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            html += "color: white;'> مشاهده پیام";
            html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }
        public static string NotConfirmDocument(string revisionNumber,string documentNumber,string projectName,string modifireUser,string rejectReason,string documentName, string linkUrl)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += @$"<p style='margin:15px 0;'> 
                در پروژه  {projectName} مدرک {documentName} به شماره {documentNumber}  به شماره ویرایش {revisionNumber} جهت اصلاح توسط کاربر {modifireUser} در سیستم مدیریت پروژه رایبد بازگشت داده شد .
               
             </p>";
            html += $"<p style='margin:15px 0;'> علت عدم تائید مدرک: {rejectReason} </p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            html += "color: white;'> مشاهده مدرک";
            html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }
        public static string ReplyCommunicationEmailTemplate(string commonocationCode, string replyBy, string dateTime, string linkUrl)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += $"<p style='margin:15px 0;'>مکاتبه {commonocationCode} توسط {replyBy} در تاریخ {dateTime} پاسخ داده شد.</p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            html += "color: white;'> مشاهده پاسخ";
            html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }

        public static string TransmittalEmailTemplate(string body)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += $"<p style='margin:15px 0;'>{body}</p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            //html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            //html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            //html += "color: white;'> مشاهده پیام";
            //html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }

        public static string CommunicationEmailTemplate(string body)
        {
            string html = "<div style='font-family:Verdana,Geneva,Tahoma,sans-serif;width: 100%;'>";
            html += "<div style='display: block;width: 800px;margin: 0 auto;max-height: 700px;'>";
            html += "<div style='display: block;background-color:#f5f5f5;padding: 30px 50px 50px 50px;'>";
            html += "<div style='display: block;'>";
            html += "<div style='direction: rtl;'>";
            html += "<img src='https://raybodravesh.com/wp-content/uploads/2019/11/LOGO-RR-IC.png' alt='logo' style='padding: 10px;' class='CToWUd'>";
            html += "</div>";
            html += "<div style='min-width:0;word-wrap:break-word;background-color:#fff;background-clip:border-box;border:1px solid rgba(0,0,0,0.125);border-radius:0.25rem;'>";
            html += "<div style='padding: 30px 30PX 30px 30PX;direction:rtl;text-align:right;font-size:16px;line-height: 1.8; white-space:normal;'>";
            html += "<p style='margin:15px 0;'>سلام</p>";
            html += $"<p>{body}</p>";
            html += "</div>";
            html += "<div style='padding: 1.75rem 1.25rem;background-color:white;border-top:1px solid rgba(0,0,0,0.125);text-align:end;'>";
            //html += $"<a href='{linkUrl}' target='_blank' style ='border: none;";
            //html += " text-decoration: none; outline: none; background-color: #28a745;  padding: 12px; width: 100px; border-radius: 6px; ";
            //html += "color: white;'> مشاهده پیام";
            //html += "</a>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";
            html += "</div>";

            return html;
        }

    }
}
