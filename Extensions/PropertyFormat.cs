using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OCSoft.Common.Extensions
{
    public struct PropertyFormat
    {
        public static readonly Type[] DefaultIgnoreTypes = new Type[]
        {
                typeof(string),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(Version),
                typeof(Exception),
                typeof(System.Net.NetworkCredential)
        };

        #region Properties

        public string Separator { get; }

        public string Indent { get; }

        public int Level { get; }

        public string ListSeparator { get; set; }

        public string NullValue { get; set; }

        public IEnumerable<Type> IgnoreTypes { get; set; }

        public Func<KeyValuePair<string, string>, string> FormatValue { get; set; }

        internal Func<PropertyInfo, bool> WhereInternal { get; private set; }

        #endregion

        #region Constructors

        public PropertyFormat(PropertyFormat format, int level)
            : this(format.Separator, format.Indent, level)
        {
            ListSeparator = format.ListSeparator;
            NullValue = format.NullValue;
            IgnoreTypes = format.IgnoreTypes;
            FormatValue = format.FormatValue;
            WhereInternal = format.WhereInternal;
        }

        public PropertyFormat(string separator, string indent = "", int level = 0)
        {
            Separator = GetIndentedSeparator(separator, indent, level);
            Indent = indent;
            Level = level;
            ListSeparator = ",";
            NullValue = "null";
            IgnoreTypes = DefaultIgnoreTypes;
            FormatValue = p => $"{p.Key}={p.Value}";
            WhereInternal = p => true;
        }

        private static string GetIndentedSeparator(string separator, string indent, int level)
        {
            if (level > 0 && !string.IsNullOrEmpty(indent))
            {
                var sb = new StringBuilder(separator);
                for (int i = 0; i < level; ++i)
                {
                    sb.Append(indent);
                }

                return sb.ToString();
            }

            return separator;
        }

        #endregion

        public PropertyFormat Where(Func<PropertyInfo, bool> predicate)
        {
            WhereInternal = predicate;
            return this;
        }

        public static PropertyFormat NewLinesIndented => new PropertyFormat(Environment.NewLine, "\t");

        public static PropertyFormat ListColumn => new PropertyFormat(";") { FormatValue = p => p.Key };

        public static PropertyFormat ListValue => new PropertyFormat(";") { FormatValue = p => p.Value };

    }
}
