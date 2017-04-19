using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbeExtractor
{
    public class ColumnInfo
    {
        public string Name { get; set; }

        public int Position { get; set; }

        public bool IsNullable { get; set; }

        public string Type { get; set; }

        public int? NumericPrecision { get; set; }

        public bool IsPrimary { get; set; }

        public int MaxLength { get; set; }

        public string DbType
        {
            get
            {
                switch (Type)
                {
                    case "int":
                        return "System.Data.DbType.Int32";
                    case "numeric":
                        return "System.Data.DbType.Decimal";
                    case "bit":
                        return "System.Data.DbType.Boolean";
                    case "nvarchar":
                        return "System.Data.DbType.AnsiString";
                    case "timestamp":
                        return "System.Data.DbType.Binary";
                    case "datetime":
                        return "System.Data.DbType.DateTime";
                    default:
                        throw new ApplicationException($"The type '{Type}' cannot be mapped");
                }
            }
        }

        public string CastFormat
        {
            get
            {
                switch (Type)
                {
                    case "int":
                        return "Convert.ToInt32({0});";
                    case "numeric":
                        return "(decimal){0};";
                    case "bit":
                        return "Convert.ToBoolean({0});";
                    case "nvarchar":
                        return "(string){0};";
                    case "timestamp":
                        return "(byte[]){0};";
                    case "datetime":
                        return "(DateTime){0};";
                    default:
                        throw new ApplicationException($"The type '{Type}' cannot be mapped");
                }
            }
        }

        public string NetType
        {
            get
            {
                switch (Type)
                {
                    case "int":
                        return "int";
                    case "numeric":
                        return "decimal";
                    case "bit":
                        return "bool";
                    case "nvarchar":
                        return "string";
                    case "timestamp":
                        return "byte[]";
                    case "datetime":
                        return "DateTime";
                    default:
                        throw new ApplicationException($"The type '{Type}' cannot be mapped");
                }
            }
        }

        public string XsdType
        {
            get
            {
                switch (Type)
                {
                    case "int":
                        return "int";
                    case "numeric":
                        return "decimal";
                    case "bit":
                        return "boolean";
                    case "nvarchar":
                        return "string";
                    case "timestamp":
                        return "base64Binary";
                    case "datetime":
                        return "dateTime";
                    default:
                        throw new ApplicationException($"The type '{Type}' cannot be mapped");
                }
            }
        }

        public string NameToCamel
        {
            get
            {
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                var name = textInfo.ToTitleCase(Name);

                if(name.Equals("isdeleted", StringComparison.InvariantCultureIgnoreCase))
                {
                    name = "IsDeleted";
                }

                return name;
            }
        }
    }
}
