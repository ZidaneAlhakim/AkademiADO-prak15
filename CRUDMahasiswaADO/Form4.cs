using System;
using System.Data;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form4 : Form
    {
        // 15. Deklarasi objek DAL
        DAL dbLogic = new DAL();

        // Pastikan variabel ini sama persis dengan nama file Crystal Report kamu
        private ListMahasisaw listMahasiswa = new ListMahasisaw();

        private string prodi;
        private DateTime tglmasuk;

        // 16. Ubah Constructor Report
        public Form4(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();

            this.prodi = Prodi;
            this.tglmasuk = TglMasuk;

            try
            {
                // Load data rekap menggunakan dbLogic
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, tglmasuk);

                // Beri nama tabel sesuai dataset (agar tidak minta login/kosong)
                dtMahasiswa.TableName = "Data";

                listMahasiswa.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = listMahasiswa;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {
        }
    }
}