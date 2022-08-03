using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Helper
{
    public static class LogTxt
    {            
            public static  void log(Exception ex, string root)
            {
                string fileName = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second
                + "_" + DateTime.Now.Millisecond + "_" + DateTime.Now.Ticks + ".txt";
                string cnt = "";
                cnt += "\t\t\t" + DateTime.Now + Environment.NewLine;
                cnt += "──────────────────────────────────────────────────" + Environment.NewLine;
                cnt += "──────────────────────────────────────────────────" + Environment.NewLine;
                cnt += ">> Message:\t";
                cnt += ex.Message;
                cnt += Environment.NewLine + Environment.NewLine;
                cnt += ">> Stack trace:\t" + Environment.NewLine;
                cnt += ex.StackTrace;
                cnt += "\n\n──────────────────────────────────────────────────";

                var innerEx = ex.InnerException;
                var indent = 1;
                while (innerEx != null)
                {
                    var spaces = String.Concat(Enumerable.Repeat(" ", indent));
                    cnt += spaces + "─────────────────────────────────" + Environment.NewLine;
                    cnt += spaces + ">> Message:\t" + Environment.NewLine;
                    cnt += spaces + innerEx.Message;
                    cnt += spaces + "\n\n";
                    cnt += spaces + ">> Stack trace:\t" + Environment.NewLine;
                    cnt += spaces + innerEx.StackTrace;
                    cnt += spaces + "\n\n─────────────────────────────────";
                    innerEx = innerEx.InnerException;
                    indent++;
                }
                if (!Directory.Exists(root + "/Files/logs/")) { Directory.CreateDirectory(root + "/Files/logs/"); }
                File.WriteAllText(root + "/Files/logs/" + fileName, cnt);
            }

            public static void log(string root, string Name, string msg, string msg2 = null, int? user_id = null)
            {
                string fileName = Name + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second
                  + "_" + DateTime.Now.Millisecond + "_" + DateTime.Now.Ticks + ".txt";
                string cnt = "";
                cnt += "user_id" + user_id != null ? user_id.ToString() : "";
                cnt += "──────────────────────────────────────────────────" + Environment.NewLine;
                cnt += "change:" + msg;
                cnt += "──────────────────────────────────────────────────" + Environment.NewLine;
                cnt += "beforeChange:" + msg2;

                File.WriteAllText(root + "/Files/logs/" + fileName, cnt);
            }
        
    }
}
