using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbeExtractor
{
    public static class EbeDataRowGenerator
    {
        public static string GenerateEbe(string ebeClassName, string ebeDataRowClassName, List<ColumnInfo> columnInfo)
        {
            var builder = new StringBuilder();

            GetNameSpace(builder);
            GetClassHeader(ebeDataRowClassName, builder);
            GetColumns(ebeClassName, columnInfo, builder);
            GetNullableColumns(ebeClassName, columnInfo, builder);

            builder.AppendLine("}");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static void GetNullableColumns(string ebeClassName, List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("#region \"Column Nullable\"");
            var nullableColums = columnInfo.Where(r => r.IsNullable).ToArray();
            foreach (var item in nullableColums)
            {
                builder.AppendLine($"public bool COL_{item.NameToCamel}Null");
                builder.AppendLine("{");
                builder.AppendLine("get");
                builder.AppendLine("{");
                builder.AppendLine($"return EBEDataRow.IsNull((int){ebeClassName}.COLUMN_INDEX.{item.Name.ToUpper()});");
                builder.AppendLine("}");
                builder.AppendLine("set");
                builder.AppendLine("{");
                builder.AppendLine("if (value == true)");
                builder.AppendLine($"EBEDataRow[(int){ebeClassName}.COLUMN_INDEX.{item.Name.ToUpper()}] = DBNull.Value;");
                builder.AppendLine("else");
                builder.AppendLine($"throw new ApplicationException(\"COL_{item.NameToCamel} Set property only accepts 'true' value.\");");
                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine();
            }
            builder.AppendLine("#endregion");
            builder.AppendLine();
        }

        private static void GetColumns(string ebeClassName, List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("#region \"Column Value\"");
            foreach (var item in columnInfo)
            {
                builder.AppendLine($"public {item.NetType} COL_{item.NameToCamel}");
                builder.AppendLine("{");
                builder.AppendLine("get");
                builder.AppendLine("{");
                builder.AppendLine("try");
                builder.AppendLine("{");
                builder.AppendLine(string.Format($"return {item.CastFormat}", $"EBEDataRow[(int){ebeClassName}.COLUMN_INDEX.{item.Name.ToUpper()}]"));
                builder.AppendLine("}");
                builder.AppendLine("catch (InvalidCastException ex)");
                builder.AppendLine("{");
                builder.AppendLine($"throw new StrongTypingException(\"cannot get value of COL_{item.NameToCamel} because it is DBNull.\", ex);");
                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine("set");
                builder.AppendLine("{");
                builder.AppendLine($"EBEDataRow[(int){ebeClassName}.COLUMN_INDEX.{item.Name.ToUpper()}] = value;");
                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine();
            }
            builder.AppendLine("#endregion");
            builder.AppendLine();
        }

        private static void GetClassHeader(string ebeDataRowClassName, StringBuilder builder)
        {
            builder.AppendLine($"public class {ebeDataRowClassName} : ABODataRowEBE, ISerializable");
            builder.AppendLine("{");

            builder.AppendLine($"internal {ebeDataRowClassName}() : base()");
            builder.AppendLine("{");
            builder.AppendLine("//");
            builder.AppendLine("// TODO: Add constructor logic here");
            builder.AppendLine("//");
            builder.AppendLine("}");

            builder.AppendLine($"internal {ebeDataRowClassName}(DataRow v_objDataRow) : base(v_objDataRow)");
            builder.AppendLine("{");
            builder.AppendLine("//");
            builder.AppendLine("// TODO: Add constructor logic here");
            builder.AppendLine("//");
            builder.AppendLine("}");

            builder.AppendLine($"public static implicit operator {ebeDataRowClassName}(DataRow v_objDataRow)");
            builder.AppendLine("{");
            builder.AppendLine($"return new {ebeDataRowClassName}(v_objDataRow);");
            builder.AppendLine("}");

            builder.AppendLine();
        }

        private static void GetNameSpace(StringBuilder builder)
        {
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Runtime.Serialization;");
            builder.AppendLine("using System.ComponentModel;");
            builder.AppendLine("using System.Data;");
            builder.AppendLine();
            builder.AppendLine("namespace Accruent.AutoGenerated");
            builder.AppendLine("{");
            builder.AppendLine("[Serializable()]");
        }
    }
}
