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
