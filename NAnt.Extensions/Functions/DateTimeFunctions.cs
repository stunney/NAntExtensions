using System;
using System.Globalization;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Extensions.Functions
{
    [FunctionSet("datetime", "datetime")]
    public class DateTimeFunctions : NAnt.Core.FunctionSetBase
    {
        public DateTimeFunctions(Project project, PropertyDictionary properties)
            : base(project, properties)
        {
        }

        [Function("parse-exact")]
        public static DateTime ParseExact(string value, string format)
        {
            return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
    }
}