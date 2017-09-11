using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbeExtractor.DataAccess
{
    class ConnectionHandler : IDisposable
    {
        private const string TABLELISTFORMAT = "SELECT TABLE_NAME, TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' and TABLE_NAME like '%{0}%' order by TABLE_SCHEMA,TABLE_NAME";
        private const string TABLEINFOFORMAT = "SELECT " +
               "c.COLUMN_NAME," +
               "c.ORDINAL_POSITION," +
               "c.IS_NULLABLE," +
               "c.DATA_TYPE," +
               "c.NUMERIC_PRECISION," +
               "c.CHARACTER_MAXIMUM_LENGTH," +
               "CASE WHEN(k.COLUMN_NAME IS NULL) THEN 0 ELSE 1 END IS_PRIMARY " +
               "FROM" +
               "    INFORMATION_SCHEMA.COLUMNS c " +
               "LEFT JOIN " +
               "(" +
               "    SELECT COLUMN_NAME" +
               "    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE" +
               "    WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA +'.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1" +
               "    AND TABLE_NAME = '{0}' AND TABLE_SCHEMA = '{1}'" +
                ")k ON k.COLUMN_NAME = c.COLUMN_NAME " +
                "WHERE " +
               "c.TABLE_NAME = '{0}'" +
                "ORDER BY " +
                "c.ORDINAL_POSITION ASC; ";

        private static SqlConnection _connection;

        public static SqlConnection Connection
        {
            get
            {
                if (_connection?.ConnectionString == string.Empty)
                {
                    if (!string.IsNullOrEmpty(_connectionString))
                        _connection = new SqlConnection(_connectionString);
                }
                if (_connection == null)
                {
                    if (!string.IsNullOrEmpty(_connectionString))
                        _connection = new SqlConnection(_connectionString);
                    else
                        _connection = new SqlConnection();
                }

                return _connection;
            }
            private set { }
        }

        private static string _connectionString;

        public static string ConnectionString
        {
            get { return _connectionString; }
            private set { _connectionString = value; }
        }


        public ConnectionHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataSet GetTableList(string filter)
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();

            var dataset = new DataSet();
            var command = string.Format(TABLELISTFORMAT, filter);

            var adapter = new SqlDataAdapter(command, Connection);

            adapter.Fill(dataset);

            return dataset;
            //}
        }

        public List<ColumnInfo> GetTableInfo(string table, string schema)
        {
            var stringBuilder = new StringBuilder();
            var command = string.Format(TABLEINFOFORMAT, table, schema);

            var adapter = new SqlDataAdapter(command, Connection);
            var dataset = new DataSet();
            adapter.Fill(dataset);

            var result = dataset.Tables[0].AsEnumerable().Select(dataRow => new ColumnInfo
            {
                Name = dataRow.Field<string>("COLUMN_NAME"),
                Position = dataRow.Field<int>("ORDINAL_POSITION"),
                IsNullable = dataRow.Field<string>("IS_NULLABLE") == "YES" ? true : false,
                Type = dataRow.Field<string>("DATA_TYPE"),
                NumericPrecision = dataRow.Field<object>("NUMERIC_PRECISION") != null ? int.Parse(dataRow.Field<object>("NUMERIC_PRECISION").ToString()) : (int?)null,
                MaxLength = dataRow.Field<object>("CHARACTER_MAXIMUM_LENGTH") != null ? int.Parse(dataRow.Field<object>("CHARACTER_MAXIMUM_LENGTH").ToString()) : 0,
                IsPrimary = dataRow.Field<int>("IS_PRIMARY") > 0
            }).ToList();

            return result;
        }

        public void Dispose()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }

            Connection.Dispose();
        }
    }
}
