using System;
using System.Globalization;

namespace Raybod.SCM.Utility.Extention
{
    public static class DateTimeExtension
    {
        public static DateTime ToGregorianDateFromPersianDate(this string persianDate)
        {
            CultureInfo persianCulture = new CultureInfo("fa-IR");
            string[] formats = { "yyyy/MM/dd", "yyyy/M/d", "yyyy/MM/d", "yyyy/M/dd" };
            DateTime d1 = DateTime.ParseExact(persianDate, formats,
                                              persianCulture, DateTimeStyles.None);
            PersianCalendar persian_date = new PersianCalendar();
            DateTime dt = persian_date.ToDateTime(d1.Year, d1.Month, d1.Day, 0, 0, 0, 0, 0);
            return dt;
        }

        public static DateTime ToGregorianDate(this string dateTime)
        {
            return DateTime.Parse(dateTime);
        }

        public static DateTime? ToGregorianDate(this DateTime? dateTime)
        {
            if (dateTime == null) return (DateTime?)null;
            DateTime d = dateTime.Value;
            return new PersianCalendar().ToDateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, 0);
        }

        public static DateTime ToGregorianDate(this DateTime dateTime)
        {
            return new PersianCalendar().ToDateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, 0);
        }

        public static string ToPersianDate(this DateTime dateTime)
        {
            PersianCalendar p = new PersianCalendar();

            return $"{p.GetYear(dateTime).ToString("0000")}/{p.GetMonth(dateTime).ToString("00")}/{p.GetDayOfMonth(dateTime).ToString("00")}";
            //return new DateTime(p.GetYear(dateTime), p.GetMonth(dateTime), p.GetDayOfMonth(dateTime), p.GetHour(dateTime), p.GetMinute(dateTime), p.GetSecond(dateTime),0);
        }

        public static string ToPersianDateString(this DateTime? dateTime)
        {
            if (dateTime == null) return "";
            DateTime d = dateTime.Value;
            PersianCalendar p = new PersianCalendar();
            return $"{p.GetYear(d).ToString("0000")}/{p.GetMonth(d).ToString("00")}/{p.GetDayOfMonth(d).ToString("00")}";
            //return new DateTime(p.GetYear(d), p.GetMonth(d), p.GetDayOfMonth(d), p.GetHour(d), p.GetMinute(d), p.GetSecond(d));
        }
        public static string ToJalaliWithTime(this DateTime? dateTime)
        {
            if (dateTime == null) return "";
            PersianCalendar persianCalendar = new PersianCalendar();
            return persianCalendar.GetYear(dateTime.Value).ToString("0000")
                + "/" +
                persianCalendar.GetMonth(dateTime.Value).ToString("00")
                + "/" +
                persianCalendar.GetDayOfMonth(dateTime.Value).ToString("00")
                + " - " + dateTime.Value.ToLocalTime().ToString("HH:mm");
        }
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            var dateTimeOffset = (long)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            return dateTimeOffset;
        }

        public static long? ToUnixTimestamp(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }
            var dateTimeOffset = (long)(dateTime.Value.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            return dateTimeOffset;
        }

        public static DateTime UnixTimestampToDateTime(this long unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(unixTimeStamp)).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime? UnixTimestampToDateTime(this long? unixTimeStamp)
        {
            if (unixTimeStamp == null)
            {
                return null;
            }
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(unixTimeStamp)).ToLocalTime();
            return dtDateTime;
        }
    }
}