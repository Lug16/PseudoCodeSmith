using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbeExtractor
{
    public static class EbeGenerator
    {
        public static string GenerateEbe(string schema, string tableName, string ebeClassName, string ebeDataRowClassName, string ebeDataViewClassName, List<ColumnInfo> columnInfo)
        {
            var builder = new StringBuilder();

            GetNameSpace(builder);
            GetClassHeader(ebeClassName, ebeDataRowClassName, builder);
            GetMethods(ebeDataRowClassName, builder);
            GetColumnIndexEnum(columnInfo, builder);

            var primarykeys = columnInfo.Where(r => r.IsPrimary).ToArray();
            GetKeyColumnIndex(builder, primarykeys);

            GetConstants(schema, tableName, columnInfo, builder, primarykeys);

            GetColumnNames(columnInfo, builder);
            GetPublicColumnNames(columnInfo, builder);

            GetArrayColumnNullables(columnInfo, builder);
            GetArrayColumnSize(columnInfo, builder);
            GetArrayColumnDbType(columnInfo, builder);
            GetArrayColumnIndex(builder, primarykeys);
            GetXsd(schema, tableName, ebeClassName, columnInfo, builder);
            GetConstructors(ebeClassName, builder);
            GetGenericMethods(ebeDataRowClassName, ebeDataViewClassName, builder);
            GetColumns(columnInfo, builder);
            GetNullableColumns(columnInfo, builder);

            builder.AppendLine("}");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static void GetNullableColumns(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("#region \"Column Nullable\"");
            var nullableColums = columnInfo.Where(r => r.IsNullable).ToArray();
            foreach (var item in nullableColums)
            {
                builder.AppendLine("[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)]");
                builder.AppendLine($"public bool COL_{item.NameToCamel}Null");
                builder.AppendLine("{");
                builder.AppendLine("get");
                builder.AppendLine("{");
                builder.AppendLine($"return EBECurrentRow.IsNull((int)COLUMN_INDEX.{item.Name.ToUpper()});");
                builder.AppendLine("}");
                builder.AppendLine("set");
                builder.AppendLine("{");
                builder.AppendLine("if (value == true)");
                builder.AppendLine($"EBECurrentRow[(int)COLUMN_INDEX.{item.Name.ToUpper()}] = DBNull.Value;");
                builder.AppendLine("else");
                builder.AppendLine($"throw new ApplicationException(\"COL_{item.NameToCamel} Set property only accepts 'true' value.\");");
                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine();
            }
            builder.AppendLine("#endregion");
            builder.AppendLine();
        }

        private static void GetColumns(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("#region \"Column Value\"");
            foreach (var item in columnInfo)
            {
                builder.AppendLine("[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)]");
                builder.AppendLine($"public {item.NetType} COL_{item.NameToCamel}");
                builder.AppendLine("{");
                builder.AppendLine("get");
                builder.AppendLine("{");
                builder.AppendLine("try");
                builder.AppendLine("{");
                builder.AppendLine(string.Format($"return {item.CastFormat}", $"EBECurrentRow[(int)COLUMN_INDEX.{item.Name.ToUpper()}]"));
                builder.AppendLine("}");
                builder.AppendLine("catch (InvalidCastException ex)");
                builder.AppendLine("{");
                builder.AppendLine($"throw new StrongTypingException(\"cannot get value of COL_{item.NameToCamel} because it is DBNull.\", ex);");
                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine("set");
                builder.AppendLine("{");
                builder.AppendLine($"EBECurrentRow[(int)COLUMN_INDEX.{item.Name.ToUpper()}] = value;");
                builder.AppendLine("}");
                builder.AppendLine("}");
                builder.AppendLine();
            }
            builder.AppendLine("#endregion");
            builder.AppendLine();
        }

        private static void GetGenericMethods(string ebeDataRowClassName, string ebeDataViewClassName, StringBuilder builder)
        {
            builder.AppendLine("public override String EBEDataSetXSDSchema");
            builder.AppendLine("{");
            builder.AppendLine("get { return cstrXSDTDSSchema; }");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override int EBEColumnCount");
            builder.AppendLine("{");
            builder.AppendLine("get");
            builder.AppendLine("{");
            builder.AppendLine("return cintEBEColumnCount;");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine($"public {ebeDataRowClassName} EBECurrent{ebeDataRowClassName}");
            builder.AppendLine("{");
            builder.AppendLine($"get {{ return new {ebeDataRowClassName}(EBEDataTable.Rows[EBECurrentRowNo]); }}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine($"public {ebeDataViewClassName} EBEDefault{ebeDataViewClassName}");
            builder.AppendLine("{");
            builder.AppendLine($"get {{ return new {ebeDataViewClassName}(EBEDataTable.DefaultView); }}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine($"public {ebeDataViewClassName} EBENew{ebeDataViewClassName}(string v_RowFilter, string v_Sort, DataViewRowState v_RowState)");
            builder.AppendLine("{");
            builder.AppendLine($"return new {ebeDataViewClassName}(EBEDataTable.DefaultView, v_RowFilter, v_Sort, v_RowState);");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override string EBEColumnName(int intColumnIndexVal)");
            builder.AppendLine("{");
            builder.AppendLine("return mstrEBEColumnName[intColumnIndexVal];");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override System.Data.DbType EBEColumnDBDataType(int intColumnIndexVal)");
            builder.AppendLine("{");
            builder.AppendLine("return menmEBEColumnDBDataType[intColumnIndexVal];");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override bool EBEColumnIsNullable(int intColumnIndexVal)");
            builder.AppendLine("{");
            builder.AppendLine("return mblnEBEColumnIsNullable[intColumnIndexVal];");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override int EBEColumnSize(int intColumnIndexVal)");
            builder.AppendLine("{");
            builder.AppendLine("return mintEBEColumnSize[intColumnIndexVal];");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public string EBEColumnName(COLUMN_INDEX v_enmCOLUMN_INDEX)");
            builder.AppendLine("{");
            builder.AppendLine("return EBEColumnName((int)v_enmCOLUMN_INDEX);");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public System.Data.DbType EBEColumnDBDataType(COLUMN_INDEX v_enmCOLUMN_INDEX)");
            builder.AppendLine("{");
            builder.AppendLine("return EBEColumnDBDataType((int)v_enmCOLUMN_INDEX);");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override int EBEKeyColumnIndex(int intColumnIndexVal)");
            builder.AppendLine("{");
            builder.AppendLine("return (int)menmEBEKeyColumnsIndex[intColumnIndexVal];");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public bool EBEColumnIsNullable(COLUMN_INDEX v_enmCOLUMN_INDEX)");
            builder.AppendLine("{");
            builder.AppendLine("return EBEColumnIsNullable((int)v_enmCOLUMN_INDEX);");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override int EBEKeyColumnCount");
            builder.AppendLine("{");
            builder.AppendLine("get");
            builder.AppendLine("{");
            builder.AppendLine("return cintEBEKeyColumnCount;");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override int EBEIdentityColumnIndex");
            builder.AppendLine("{");
            builder.AppendLine("get");
            builder.AppendLine("{");
            builder.AppendLine("return (int)cenmEBEIdentityColumn;");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override int EBEEntityGlobalID");
            builder.AppendLine("{");
            builder.AppendLine("get");
            builder.AppendLine("{");
            builder.AppendLine("return cintEBEEntityGlobalID;");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public override string EBETableName");
            builder.AppendLine("{");
            builder.AppendLine("get");
            builder.AppendLine("{");
            builder.AppendLine("return cstrEBETableName;");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("public static string EBE_TableName");
            builder.AppendLine("{");
            builder.AppendLine("get");
            builder.AppendLine("{");
            builder.AppendLine("return cstrEBETableName;");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();
        }

        private static void GetConstructors(string ebeClassName, StringBuilder builder)
        {
            builder.AppendLine("#region constructors");
            builder.AppendLine($"public {ebeClassName}() : base()");
            builder.AppendLine("{");
            builder.AppendLine("//");
            builder.AppendLine("// TODO: Add constructor logic here");
            builder.AppendLine("//");
            builder.AppendLine("EBEInitializeDataSet();");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine("// To do the deserialization, we add the following constructor to the DataSet:");
            builder.AppendLine($"protected {ebeClassName}(SerializationInfo info, StreamingContext context) : base(info, context)");
            builder.AppendLine("{");
            builder.AppendLine("}");
            builder.AppendLine();

            builder.AppendLine($"public {ebeClassName}(DataSet objDataSetEBE) : base(objDataSetEBE)");
            builder.AppendLine("{");
            builder.AppendLine("//");
            builder.AppendLine("// TODO: Add constructor logic here");
            builder.AppendLine("//");
            builder.AppendLine("EBEInitializeDataSet();");
            builder.AppendLine("}");
            builder.AppendLine("#endregion");
            builder.AppendLine();
        }

        private static void GetXsd(string schema, string tableName, string ebeClassName, List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("const string cstrXSDTDSSchema =");
            builder.AppendLine("\"<?xml version=\\\"1.0\\\" encoding=\\\"us-ascii\\\"?>\" + ");
            builder.AppendLine($"\"<xs:schema id=\\\"{ebeClassName}\\\" xmlns=\\\"\\\" xmlns:xs=\\\"http://www.w3.org/2001/XMLSchema\\\" xmlns:msdata=\\\"urn:schemas-microsoft-com:xml-msdata\\\">\" + ");
            builder.AppendLine($"\"<xs:element name=\\\"{ebeClassName}\\\" msdata:IsDataSet=\\\"true\\\">\" + ");
            builder.AppendLine("\"<xs:complexType>\" +");
            builder.AppendLine("\"<xs:choice maxOccurs=\\\"unbounded\\\">\" + ");
            builder.AppendLine($"\"<xs:element name=\\\"{schema}.{tableName}\\\">\" + ");
            builder.AppendLine("\"<xs:complexType>\" +");
            builder.AppendLine("\"<xs:sequence>\" +");
            foreach (var item in columnInfo)
            {
                builder.AppendLine($"\"<xs:element name =\\\"{item.Name}\\\" type=\\\"xs:{item.XsdType}\\\" minOccurs=\\\"0\\\" />\" + ");
            }
            builder.AppendLine("\"</xs:sequence>\" +");
            builder.AppendLine("\"</xs:complexType>\" +");
            builder.AppendLine("\"</xs:element>\" +");
            builder.AppendLine("\"</xs:choice>\" +");
            builder.AppendLine("\"</xs:complexType>\" +");
            builder.AppendLine("\"</xs:element>\" +");
            builder.AppendLine("\"</xs:schema>\";");
            builder.AppendLine();
        }

        private static void GetArrayColumnIndex(StringBuilder builder, ColumnInfo[] primarykeys)
        {
            builder.AppendLine("static COLUMN_INDEX[] menmEBEKeyColumnsIndex = new COLUMN_INDEX[cintEBEKeyColumnCount]{");

            foreach (var item in primarykeys)
            {
                builder.AppendLine($"COLUMN_INDEX.{item.Name.ToUpper()}" + (item == primarykeys.Last() ? string.Empty : ","));
            }

            builder.AppendLine("};");
            builder.AppendLine();
        }

        private static void GetArrayColumnDbType(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("static System.Data.DbType[] menmEBEColumnDBDataType = { ");
            foreach (var item in columnInfo)
            {
                builder.AppendLine(item.DbType + (item == columnInfo.Last() ? string.Empty : ","));
            }
            builder.AppendLine("};");
            builder.AppendLine();
        }

        private static void GetArrayColumnSize(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("static int[] mintEBEColumnSize = { ");
            foreach (var item in columnInfo)
            {
                builder.AppendLine(item.MaxLength + (item == columnInfo.Last() ? string.Empty : ","));
            }
            builder.AppendLine("};");
            builder.AppendLine();
        }

        private static void GetArrayColumnNullables(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("static bool[] mblnEBEColumnIsNullable = { ");
            foreach (var item in columnInfo)
            {
                builder.AppendLine((item.IsNullable ? "true" : "false") + (item == columnInfo.Last() ? string.Empty : ","));
            }
            builder.AppendLine("};");
            builder.AppendLine();
        }

        private static void GetPublicColumnNames(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            for (int i = 0; i < columnInfo.Count; i++)
            {
                builder.AppendLine($"public static string CN_{columnInfo[i].Name.ToUpper()}");
                builder.AppendLine("{");
                builder.AppendLine($"get{{return mstrEBEColumnName[{i}];}}");
                builder.AppendLine("}");
            }
            builder.AppendLine();
        }

        private static void GetColumnNames(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("static string[] mstrEBEColumnName = { ");
            foreach (var item in columnInfo)
            {
                builder.AppendLine(string.Format("\"{0}\"{1}", item.Name, (item == columnInfo.Last() ? string.Empty : ",")));
            }
            builder.AppendLine("};");

            builder.AppendLine();
        }

        private static void GetConstants(string schema, string tableName, List<ColumnInfo> columnInfo, StringBuilder builder, ColumnInfo[] primarykeys)
        {
            builder.AppendLine($"const int cintEBEColumnCount = { columnInfo.Count };");

            builder.AppendLine();

            builder.AppendLine("const COLUMN_INDEX cenmEBEIdentityColumn = COLUMN_INDEX.ROW_ID;");

            builder.AppendLine();

            builder.AppendLine($"const int cintEBEKeyColumnCount = {primarykeys.Length};");

            builder.AppendLine();

            builder.AppendLine($"const string cstrEBETableName = \"{schema}.{tableName}\";");

            builder.AppendLine();

            //TODO: No def for cintEBEEntityGlobalID research is needed
            builder.AppendLine("const int cintEBEEntityGlobalID = 0;");

            builder.AppendLine();
        }

        private static void GetKeyColumnIndex(StringBuilder builder, ColumnInfo[] primarykeys)
        {
            builder.AppendLine("public enum KEY_COLUMN_INDEX{");

            for (int i = 0; i < primarykeys.Length; i++)
            {
                builder.AppendLine($"{primarykeys[i].Name} = {i},");
            }
            builder.AppendLine("}");

            builder.AppendLine();
        }

        private static void GetColumnIndexEnum(List<ColumnInfo> columnInfo, StringBuilder builder)
        {
            builder.AppendLine("public enum COLUMN_INDEX");
            builder.AppendLine("{");
            builder.AppendLine("NEGATIVEONE = -1,");

            foreach (var item in columnInfo)
            {
                var index = item.Position - 1;
                var indexName = item.Name.ToUpper();
                builder.AppendLine($"{indexName} = {index},");
            }
            builder.AppendLine("}");
            builder.AppendLine();
        }

        private static void GetMethods(string ebeDataRowClassName, StringBuilder builder)
        {
            builder.AppendLine($"public {ebeDataRowClassName} EBESelectDataRow(string v_strCriteria)");
            builder.AppendLine("{");
            builder.AppendLine($"{ebeDataRowClassName}[] obj{ebeDataRowClassName} = EBESelect(v_strCriteria);");
            builder.AppendLine($"return obj{ebeDataRowClassName}.Length == 1 ? obj{ebeDataRowClassName}[0] : null;");
            builder.AppendLine("}");
            builder.AppendLine();
            builder.AppendLine($"public {ebeDataRowClassName} EBESelectDataRow(string v_strCriteria, string v_strRecordNotFoundErrorMsg)");
            builder.AppendLine("{");
            builder.AppendLine("try");
            builder.AppendLine("{");
            builder.AppendLine($"{ebeDataRowClassName} obj{ebeDataRowClassName} = EBEDataTable.Select(v_strCriteria)[0];");
            builder.AppendLine($"return obj{ebeDataRowClassName};");
            builder.AppendLine("}");
            builder.AppendLine("catch");
            builder.AppendLine("{");
            builder.AppendLine("throw new Exception(v_strRecordNotFoundErrorMsg);");
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine();
            builder.AppendLine("public bool EBESelectDataRowExists(string v_strCriteria)");
            builder.AppendLine("{");
            builder.AppendLine("bool blnExists = false;");
            builder.AppendLine("blnExists = EBEDataTable.Select(v_strCriteria).GetLength(0) > 0 ? true : false;");
            builder.AppendLine("return blnExists;");
            builder.AppendLine("}");
            builder.AppendLine();
        }

        private static void GetClassHeader(string ebeClassName, string ebeDataRowClassName, StringBuilder builder)
        {
            builder.AppendLine($"public class {ebeClassName} : ABOBaseUpdatableTypedDataSetEBE, ISerializable");
            builder.AppendLine("{");
            builder.AppendLine($"public {ebeDataRowClassName}[] EBESelect(string v_strCriteria)");
            builder.AppendLine("{");
            builder.AppendLine("DataRow[] objSrcDataRow = EBEDataTable.Select(v_strCriteria);");
            builder.AppendLine($"{ebeDataRowClassName}[] obj{ebeDataRowClassName} = new {ebeDataRowClassName}[objSrcDataRow.Length];");
            builder.AppendLine("for (int intIndex = 0; intIndex < objSrcDataRow.GetLength(0); intIndex++)");
            builder.AppendLine("{");
            builder.AppendLine($"obj{ebeDataRowClassName}[intIndex] = objSrcDataRow[intIndex];");
            builder.AppendLine("}");
            builder.AppendLine($"return obj{ebeDataRowClassName};");
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
