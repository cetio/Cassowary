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

using Cassowary.Intrinsics.VM;
using ConsoleTableExt;
using System.Text;

namespace Cassowary.Intrinsics
{
    public class Inspector
    {
        public static StringBuilder Dump<T>(object? obj)
        {
            if (obj is null)
                return new StringBuilder();

            TypedReference typedReference = __makeref(obj);
            List<List<object>> tableData = new List<List<object>>();

            foreach (var field in typeof(T).GetFields())
            {
                object? value = field.GetValueDirect(typedReference);

                tableData.Add(new List<object>
                {
                    field.Name,
                    field.FieldType.IsPointer
                    ? Unsafe.As<object?, nint>(ref value).ToString("X16")
                    : value ?? string.Empty
                });
            }

            foreach (var property in typeof(T).GetProperties())
            {
                object? value = property.GetValue(obj);

                tableData.Add(new List<object>
                {
                    property.Name,
                    property.PropertyType.IsPointer
                    ? Unsafe.As<object?, nint>(ref value).ToString("X16")
                    : value ?? string.Empty
                });
            }

            return ConsoleTableBuilder
                .From(tableData)
                .WithTitle(obj.GetType().Name)
                .WithFormat(ConsoleTableBuilderFormat.Minimal)
                .Export();
        }

        public static unsafe StringBuilder DumpFields(FieldDesc*[] fields)
        {
            List<List<object>> tableData = new List<List<object>>();

            foreach (FieldDesc* fieldDesc in fields)
                tableData.Add(new List<object>()
                {
                    Dump<FieldDesc>(*fieldDesc)
                });

            return ConsoleTableBuilder
                .From(tableData)
                .WithFormat(ConsoleTableBuilderFormat.Minimal)
                .Export();
        }
    }
}
