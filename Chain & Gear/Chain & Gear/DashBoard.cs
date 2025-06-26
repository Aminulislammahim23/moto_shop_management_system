using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WFADBG;

namespace Chain___Gear
{
    public partial class DashBoard : UserControl
    {
        private DataAccess Da { get; set; }
        public DashBoard()
        {
            InitializeComponent();
            //LoadChartData(); // Load chart on initialization
        }


/*
        private void LoadChartData()
        {
            string serial = this.productSerialTxt.Text.Trim();
            if (string.IsNullOrEmpty(serial))
            {
                MessageBox.Show("Please enter a valid product serial.");
                return;
            }

            string sql = "SELECT * FROM Productlist WHERE Productserial = '" + serial + "';";
            var ds = this.Da.ExecuteQuery(sql); // Assuming Da is your DataAccess object

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("No data found for the given product serial.");
                return;
            }

            DataTable dt = ds.Tables[0];

            chart1.Series.Clear();
            Series series = chart1.Series.Add("Product Stats");
            series.ChartType = SeriesChartType.Column;

            foreach (DataRow row in dt.Rows)
            {
                // You must choose meaningful columns for chart values (example below)
                string xValue = row["ProductName"].ToString(); // or "Date", "Category", etc.
                decimal yValue = Convert.ToDecimal(row["Stock"]); // or "Price", "Sales", etc.

                series.Points.AddXY(xValue, yValue);
            }

            chart1.ChartAreas[0].AxisX.Title = "Product";
            chart1.ChartAreas[0].AxisY.Title = "Stock";
        }
*/
    }

}
