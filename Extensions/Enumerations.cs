using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Nysc.API.Extensions
{
    public static class Enumerations
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string ToDescription<TEnum>(this TEnum EnumValue) where TEnum : struct
        {
            return GetEnumDescription((Enum)(object)EnumValue);
        }

        public static string[] GetDescriptions<TEnum>() where TEnum : struct
        {
            Array values = Enum.GetValues(typeof(TEnum));
            string[] descriptions = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                descriptions[i] = ((TEnum)values.GetValue(i)).ToDescription();

            return descriptions;
        }
    }
}
