using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OCSoft.Common.Extensions
{
    public static class ObjectExtensions
    {
        #region PropertiesToString
        
        public static string PropertiesToString(this object obj)
        {
            return PropertiesToString(obj, PropertyFormat.NewLinesIndented);
        }
        
        public static string PropertiesToString(this object obj, PropertyFormat format)
        {
            if (obj == null)
            {
                return format.NullValue;
            }
            
            var type = obj.GetType();
            if (type.IsClass && !format.IgnoreTypes.Contains(type))
            {
                try
                {
                    var nextFormat = new PropertyFormat(format, format.Level + 1);
                    if (obj is IEnumerable list)
                    {
                        var x = new List<string>();
                        foreach (var item in list)
                        {
                            x.Add(PropertiesToString(item, nextFormat));
                        }
                        return string.Join(format.ListSeparator, x);
                    }

                    var propDict = type.GetProperties().Where(format.WhereInternal)
                        .ToDictionary(p => p.Name, p => PropertiesToString(p.GetValue(obj), nextFormat));
                    return string.Join(format.Separator, propDict.Select(format.FormatValue));
                }
                catch (Exception ex)
                {
                    return MostInnerException(ex).Message;
                }
            }

            return obj.ToString();
        }

        #endregion

        public static Exception MostInnerException(this Exception ex)
        {
            if (ex.InnerException != null)
            {
                return MostInnerException(ex.InnerException);
            }
            return ex;
        }
    }
}
