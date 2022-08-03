using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raybod.SCM.Utility.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<SelectListItem> GetItems(
       this Type enumType, int? selectedValue)
        {
            if (!typeof(Enum).IsAssignableFrom(enumType))
            {
                throw new ArgumentException("Type must be an enum");
            }

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType).Cast<int>();

            var items = names.Zip(values, (name, value) =>
                    new SelectListItem
                    {
                        Text = GetName(enumType, name),
                        Value = value.ToString(CultureInfo.InvariantCulture),
                        Selected = value == selectedValue
                    }
                );
            return items;
        }

        static string GetName(Type enumType, string name)
        {
            var result = name;

            var attribute = enumType
                .GetField(name)
                .GetCustomAttributes(inherit: false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            if (attribute != null)
            {
                result = attribute.GetName();
            }

            return result;
        }

        public static string GetEnumDescription(Type type, string value)
        {
            var name = Enum.GetNames(type).Where(f => f.Equals(value, StringComparison.CurrentCultureIgnoreCase)).Select(d => d).FirstOrDefault();

            if (name == null)
            {
                return string.Empty;
            }
            var field = type.GetField(name);
            var customAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : name;
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static bool ValidateItem<T>(T eEnumItem)
        {
            
            if (Enum.IsDefined(typeof(T), eEnumItem) == true)
                return true;
            else
                return false;
        }
        public static bool ValidateItem<T>(List<T> eEnumItem)
        {
            if(eEnumItem!=null&& eEnumItem.Any())
            {
                foreach (var item in eEnumItem)
                {
                    if (!(Enum.IsDefined(typeof(T), item) == true))
                        return false;
    
                }
                return true;
            }
            return false;
        }

        public static T ToEnum<T>(string value, T defaultValue) where T : struct
        {
            try
            {
                T enumValue;
                if (!Enum.TryParse(value, true, out enumValue))
                {
                    return defaultValue;
                }
                return enumValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static List<T> ConvertToEnumList<T>(string enumValu)
        {
            var result = new List<T>();
            if (string.IsNullOrEmpty(enumValu))
                return null;
            if (!enumValu.Contains(","))
            {
                result.Add(EnumHelper.ToEnum<T>(enumValu));
                return result;
            }

            var listEnumString = StringHelper.SplitHelper(enumValu);
            if (listEnumString.Count == 0) return null;
            foreach (var item in listEnumString)
            {
                result.Add(EnumHelper.ToEnum<T>(item));
            }
            return result;
        }

        public static string EnumListConvertToString<T>(List<T> requestTypes)
        {
            var result = string.Empty;
            if (requestTypes.Count == 0) return result;
            for (int i = 0; i < requestTypes.Count(); i++)
            {
                if (i == 0)
                    result = $"{requestTypes[i].ToString()}";
                else result = $"{result},{requestTypes[i].ToString()}";
            }
            return result;
        }

    }
}