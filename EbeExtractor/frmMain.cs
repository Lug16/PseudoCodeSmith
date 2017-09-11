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
using EbeExtractor.DataAccess;
using EbeExtractor.FileProcessor;

namespace EbeExtractor
{
    public partial class frmMain : Form
    {
        ConnectionHandler connection = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (dgvTables.Rows.Count > 0)
            {
                folderBrowserDialog1.ShowDialog();

                var location = folderBrowserDialog1.SelectedPath;

                textBox1.Text = location;

                btnExtract.Enabled = true;
            }
            else
            {
                MessageBox.Show("No data to extract, please check some tables", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            connection = new ConnectionHandler(txtConnectionString.Text);

            try
            {
                dgvTables.AutoGenerateColumns = false;
                dgvTables.DataSource = connection.GetTableList(textBox2.Text);
                dgvTables.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            try
            {
                int filesGenerated = 0;

                foreach (DataGridViewRow row in dgvTables.Rows)
                {
                    var checkbox = (DataGridViewCheckBoxCell)row.Cells[0];

                    if (checkbox.Value == checkbox.TrueValue)
                    {
                        filesGenerated++;
                        var table = row.Cells[1].Value.ToString();
                        var schema = row.Cells[2].Value.ToString();

                        var tableInfo = connection.GetTableInfo(table, schema);

                        FileHandler.GenerateFiles(schema, table, textBox1.Text, tableInfo);
                    }
                }

                MessageBox.Show(string.Format("Done! {0} EBE files generated successfully", filesGenerated), "Yeah!!!", MessageBoxButtons.OK);

                connection.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connection != null)
                connection.Dispose();
        }
    }
}
