using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Globalization;

namespace EbeExtractor
{
    public partial class Form1 : Form
    {
        SqlConnection connection = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            var location = folderBrowserDialog1.SelectedPath;

            textBox1.Text = location;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(txtConnectionString.Text);
            var dataset = new DataSet();

            using (connection)
            {
                var command = "SELECT TABLE_NAME, TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' and TABLE_NAME like '%fasb%' order by TABLE_SCHEMA,TABLE_NAME";

                var adapter = new SqlDataAdapter(command, connection);

                adapter.Fill(dataset);
            }

            dgvTables.AutoGenerateColumns = false;
            dgvTables.DataSource = dataset;

        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(txtConnectionString.Text);
            connection.Open();
            int filesGenerated = 0;

            foreach (DataGridViewRow row in dgvTables.Rows)
            {
                var checkbox = (DataGridViewCheckBoxCell)row.Cells[0];

                if (checkbox.Value == checkbox.TrueValue)
                {
                    filesGenerated++;
                    var table = row.Cells[1].Value.ToString();
                    var schema = row.Cells[2].Value.ToString();

                    GenerateFiles(schema, table, textBox1.Text);
                }
            }

            connection.Close();
            connection.Dispose();

            MessageBox.Show(string.Format("Done! {0} EBE files generated successfully", filesGenerated), "Yeah!!!", MessageBoxButtons.OK);
        }

        private void GenerateFiles(string schema, string table, string location)
        {
            var tableInfo = GetTableInfo(table, schema);
            var className = GetClassName(table);
            var ebeClassName = $"ABO{className}EBE";
            var ebeDataRowClassName = $"ABO{className}DataRowEBE";
            var ebeDataViewClassName = $"ABO{className}DataViewEBE";

            var ebe = EbeGenerator.GenerateEbe(schema, table, ebeClassName, ebeDataRowClassName, ebeDataViewClassName, tableInfo);

            SaveFile(ebeClassName, ebe);
        }

        private void SaveFile(string ebeClassName, string ebe)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(textBox1.Text + "\\" + ebeClassName + ".cs");
            file.WriteLine(ebe);
            file.Close();
        }

        private string GetClassName(string table)
        {
            var split = table.Split('_');
            var name = string.Empty;
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;

            if (split.Length > 2)
            {
                name = string.Join("", info.ToTitleCase(split[split.Length - 2]), info.ToTitleCase(split.Last()));
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

        private List<ColumnInfo> GetTableInfo(string table, string schema)
        {
            var stringBuilder = new StringBuilder();
            var command = "SELECT " +
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
               $"    AND TABLE_NAME = '{table}' AND TABLE_SCHEMA = '{schema}'" +
                ")k ON k.COLUMN_NAME = c.COLUMN_NAME " +
                "WHERE " +
               $"c.TABLE_NAME = '{table}'" +
                "ORDER BY " +
                "c.ORDINAL_POSITION ASC; ";

            var adapter = new SqlDataAdapter(command, connection);
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
    }
}
