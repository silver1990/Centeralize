using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Raybod.SCM.Utility.Extention
{
   public static class StringExtentions
    {
        /// <summary>
        /// convert persian and arabic number to english number
        /// </summary>
        /// <param name="input">input with arabic number</param>
        /// <returns></returns>
        public static string ConvertToEnglishNumber(this string input)
        {

            var charunicodeArray = input.ToCharArray();

            var englishNumbers = "";
            if (input == null)
                return null;

            foreach (var ch in charunicodeArray)
            {
                // arabic range digit
                if (ch >= 1632 && ch <= 1641)
                {
                    // european digit range
                    var newChar = ch - 1584;
                    englishNumbers = englishNumbers + Convert.ToChar(newChar);
                }
                else if (ch >= 1776 && ch <= 1785)
                {
                    //persian range digit
                    // android
                    var newChar = ch - 1728;
                    englishNumbers = englishNumbers + Convert.ToChar(newChar);
                }
                else
                    englishNumbers = englishNumbers + Convert.ToChar(ch);
            }

            return englishNumbers;


        }

        public static string RemovePersianCharacterFromString(this string str)
        {
            return Regex.Replace(str, @"[^\u0000-\u007F]+", string.Empty);
        }

        public static string FormatMessageWithMentoinPattern(this string str)
        {
            if (str.StartsWith('@'))
            {
                string result = "";
                var mentions = str.Split('@', StringSplitOptions.RemoveEmptyEntries);
                if (mentions.Length == 1)
                {
                    result =  str.Substring(0, str.IndexOf(']') +1) + str.Substring(str.IndexOf(')') + 1);
                }
                else
                {
                    foreach (var item in mentions.Take(mentions.Count() - 1))
                    {
                        result += "@"+item.Substring(0, str.IndexOf(']') );
                    }
                    result += "@" + mentions.Last().Substring(0, str.IndexOf(']') ) + mentions.Last().Substring(str.IndexOf(')') + 1);
                }

                return   result;
            }
            else
            {
                return str;
            }
        }

        public static Dictionary<string, string> FormatEmailMessageWithMentoinPattern(this string str)
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            string firstSection = "";
            var mentions = str.Split('@', StringSplitOptions.RemoveEmptyEntries);
            if (mentions.Length == 1)
            {
                result.Add(mentions[0].Substring(1, str.IndexOf(']') - 2), mentions[0].Substring(str.IndexOf(']')));
            }
            else
            {
                foreach (var item in mentions.Take(mentions.Count() - 1))
                {
                    firstSection += item.Substring(1, str.IndexOf(']') - 2) + " ";
                }
                firstSection += mentions.Last().Substring(1, str.IndexOf(']') - 2) + " ";
                result.Add(firstSection, mentions.Last().Substring(str.IndexOf(']')));
            }

            return result;
        }
        public static string FormatEmailMessageWithMentoin(this string str)
        {
            string results = "";
            results = Regex.Replace(str, @"@\[", "<span style='color:blue'>");
            results = Regex.Replace(results, @"\]\([0-9]*\)", "</span>");
            return results;
        }

        static readonly string[] suffixes =
        { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        public static string FormatSize(this long bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        public static string CreatePath(this string path)
        {
            string result = "public";
            var paths = path.Split('/',StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length > 4)
            {
                for (int i = 4; i < paths.Length; i++)
                    result += "/" + paths[i];
            }
            
            return result;
        }
        public static string CreatePathPrivate(this string path)
        {
            string result = "private";
            var paths = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length > 5)
            {
                for (int i = 5; i < paths.Length; i++)
                    result += "/" + paths[i];
            }

            return result;
        }
    }
}
