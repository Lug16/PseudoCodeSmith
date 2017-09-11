using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbeExtractor.FileProcessor
{
    static class FileHandler
    {
        public static void GenerateFiles(string schema, string table, string location, List<ColumnInfo> tableInfo)
        {
            var className = GetClassName(table);
            var ebeClassName = $"ABO{className}EBE";
            var ebeDataRowClassName = $"ABO{className}DataRowEBE";
            var ebeDataViewClassName = $"ABO{className}DataViewEBE";

            var ebe = EbeGenerator.GenerateEbe(schema, table, ebeClassName, ebeDataRowClassName, ebeDataViewClassName, tableInfo);
            SaveFile(ebeClassName, ebe, location);

            var ebeDataRow = EbeDataRowGenerator.GenerateEbe(ebeClassName, ebeDataRowClassName, tableInfo);
            SaveFile(ebeDataRowClassName, ebeDataRow, location);

            var ebeDataView = EbeDataViewGenerator.GenerateEbe(ebeClassName, ebeDataViewClassName, ebeDataRowClassName, tableInfo);
            SaveFile(ebeDataViewClassName, ebeDataView, location);
        }

        private static void SaveFile(string ebeClassName, string ebe, string location)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(location + "\\" + ebeClassName + ".cs");
            file.WriteLine(ebe);
            file.Close();
        }

        private static string GetClassName(string table)
        {
            var split = table.Split('_');
            var name = string.Empty;
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;

            if (split.Length > 2)
            {
                name = string.Join(string.Empty, info.ToTitleCase(split[split.Length - 2]), info.ToTitleCase(split.Last()));
            }
            else if (split.Any())
            {
                name = string.Join(string.Empty, split.Select(r => info.ToTitleCase(r)));
            }
            else
            {
                name = info.ToTitleCase(table);
            }

            return name;
        }
    }
}
