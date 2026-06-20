using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CRUDMahasiswaADO
{
    public partial class FormDashboard : Form
    {
        DAL dbLogic = new DAL();
        bool isInitializing = true;
        DataTable dt;
        int button = 0;
        public FormDashboard()
        {
            InitializeComponent();
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbTipe.DropDownStyle = ComboBoxStyle.DropDownList;
            var items = new List<KeyValuePair<string, SeriesChartType>>
        {
        new KeyValuePair<string, SeriesChartType>("Kolom", SeriesChartType.Column),
        new KeyValuePair<string, SeriesChartType>("Pie", SeriesChartType.Pie)
        };

            isInitializing = true;
            cmbTipe.DataSource = items;
            cmbTipe.DisplayMember = "Key";
            cmbTipe.ValueMember = "Value";
            cmbTipe.SelectedIndex = 0;

            isInitializing = false;
            loadDataChart();
        }
        public void loadDataChart()
        {
            chartProdi.Series.Clear();
            chartProdi.Titles.Clear();
            chartProdi.Legends.Clear();
            chartProdi.ChartAreas.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.AxisX.Title = "Program Studi";
            ca.AxisY.Title = "Jumlah Mahasiswa";
            ca.AxisX.LabelStyle.Angle = -45;
            ca.BackColor = Color.Transparent;
            chartProdi.ChartAreas.Add(ca);

            try
            {
                if (button == 1)
                {
                    dt = dbLogic.getDataChartByTahun(dtpTanggalMasuk.Value);
                }
                else
                {
                    dt = dbLogic.getAllDataChart();
                }

                SeriesChartType tipe = (SeriesChartType)cmbTipe.SelectedValue;

                if (tipe == SeriesChartType.Column)
                {
                    Series s = new Series("Mahasiswa");
                    s.ChartType = SeriesChartType.Column;

                    foreach (DataRow row in dt.Rows)
                    {
                        // Pastikan nama kolom "NamaProdi" sesuai dengan query SQL kamu
                        string prodi = row["NamaProdi"].ToString();
                        int jumlah = Convert.ToInt32(row["JmlhMhs"]);
                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
                else
                {
                    Series s = new Series("Jumlah Mahasiswa");
                    s.ChartType = tipe;
                    s.IsValueShownAsLabel = true;
                    s.Label = "#VAL";
                    s.LegendText = "#VALX";

                    foreach (DataRow row in dt.Rows)
                    {
                        string prodi = row["NamaProdi"].ToString();
                        int jumlah = Convert.ToInt32(row["JmlhMhs"]);
                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }

            Title title = new Title("Jumlah Mahasiswa per Program Studi", Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.DarkBlue);
            chartProdi.Titles.Add(title);

            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chartProdi.Legends.Add(legend);
        }

        private void cmbTipe_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isInitializing)
                return;

            if (button == 1)
            {
            }
            else
            {
                loadDataChart();
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            button = 1;
            loadDataChart();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            button = 0;
            loadDataChart();
        }

        private void btnDataMahasiswa_Click(object sender, EventArgs e)
        {
            Form1 frm1 = new Form1();
            frm1.Show();
            this.Hide();
        }
    }

}
