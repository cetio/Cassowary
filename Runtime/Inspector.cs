//              FUCK OOP & FUCK INTERLACED LIBRARIES
//
//            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                   Version 2, December 2004
// 
// Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
// Everyone is permitted to copy and distribute verbatim or modified
// copies of this license document, and changing it is allowed as long
// as the name is changed.
//
//           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//  0. You just DO WHAT THE FUCK YOU WANT TO.

using Cassowary.Runtime.VM;
using System.Reflection;
using System.Text;

namespace Cassowary.Runtime
{
    public class Inspector
    {
        /// <summary>
        /// Generates a formatted string representation of an object's public fields and properties.
        /// </summary>
        /// <param name="obj">The object to inspect.</param>
        /// <returns>A StringBuilder containing the formatted output.</returns>
        public static StringBuilder Dump(object? obj)
        {
            if (obj is null)
                return new StringBuilder();

            var tableData = GetPublicFieldAndPropertyData(obj);
            var title = obj.GetType().Name.ToUpper();

            return FormatTable(title, tableData);
        }

        private static List<string[]> GetPublicFieldAndPropertyData(object obj)
        {
            var tableData = new List<string[]>();

            foreach (var field in GetPublicFields(obj))
            {
                var name = field.Name;
                var value = GetDisplayValue(field.GetValue(obj));
                tableData.Add(CreateRow(name, value));
            }

            foreach (var property in GetPublicProperties(obj))
            {
                var name = property.Name;
                var value = GetDisplayValue(property.GetValue(obj));
                tableData.Add(CreateRow(name, value));
            }

            return tableData;
        }

        private static FieldInfo[] GetPublicFields(object obj)
        {
            return obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        }

        private static PropertyInfo[] GetPublicProperties(object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        private static string[] CreateRow(string name, string value)
        {
            return new string[] { name, value };
        }

        private static string GetDisplayValue(object? value)
        {
            return value switch
            {
                null => string.Empty,
                nint nintValue => nintValue.ToString(),
                _ => value.ToString() ?? string.Empty
            };
        }

        private static StringBuilder FormatTable(string title, List<string[]> data)
        {
            var output = new StringBuilder();

            foreach (var row in data)
            {
                var rowText = string.Format("{0,-30} {1}", row[0], row[1]);
                output.AppendLine(rowText);
            }

            var paddingLength = string.Format("{0,-30} {1}", data[0][0], data[0][1]).Length / 2;
            var formattedTitle = $"--- {title} ---\n".PadLeft(paddingLength);
            output.Insert(0, formattedTitle);

            return output;
        }

        /// <summary>
        /// Generates a formatted string representation of an array of FieldDesc pointers.
        /// </summary>
        /// <param name="fields">The array of FieldDesc pointers.</param>
        /// <returns>A StringBuilder containing the formatted output.</returns>
        public static unsafe StringBuilder DumpFields(FieldDesc*[] fields)
        {
            var tableData = new List<string[]>();

            foreach (var fieldDesc in fields)
            {
                var fieldDescDump = Dump(*fieldDesc).ToString();
                tableData.Add(new string[] { fieldDescDump });
            }

            return FormatTable("FIELD DESCS", tableData);
        }
    }
}

