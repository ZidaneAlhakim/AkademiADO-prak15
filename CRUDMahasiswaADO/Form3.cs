using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form3 : Form
    {
        private readonly string connectionString = "Data Source=LAPTOP-M60LBIQK\\ZIDANEAS; Initial Catalog=DBAkademikADO; Integrated Security=True";
        private readonly SqlConnection conn;
        private SqlDataAdapter da;
        private DataTable dtMahasiswa;
        private DataTable dtProdi;

        public Form3()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            // Mengatur agar DateTimePicker hanya menampilkan format Tahun
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            // Mengatur style ComboBox prodi
            cmbProdi.DropDownStyle = ComboBoxStyle.DropDownList;
            btnCetak.Enabled = false;

            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                // Mengambil KodeProdi dan NamaProdi dari database
                SqlCommand cmd = new SqlCommand("SELECT KodeProdi, NamaProdi FROM ProgramStudi", conn);
                cmd.CommandType = CommandType.Text;
                dtProdi = new DataTable();
                da = new SqlDataAdapter(cmd);
                da.Fill(dtProdi);

                // Memasukkan data ke ComboBox
                cmbProdi.DataSource = dtProdi;
                cmbProdi.DisplayMember = "NamaProdi"; // Teks yang dilihat oleh user (misal: "Teknik Informatika")
                cmbProdi.ValueMember = "KodeProdi";   // Nilai yang akan dipakai untuk filter (misal: "TI")
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal Load data prodi: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                // Memanggil Stored Procedure untuk filter data
                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@inProdi", SqlDbType.VarChar, 50).Value = cmbProdi.SelectedValue.ToString();
                cmd.Parameters.Add("@inTglMsuk", SqlDbType.VarChar, 4).Value = dtpTanggalMasuk.Value.Year.ToString();

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                dataGridView1.DataSource = dtMahasiswa;

                // Validasi tombol cetak aktif jika data ditemukan
                if (dtMahasiswa.Rows.Count > 0)
                {
                    btnCetak.Enabled = true;
                }
                else
                {
                    btnCetak.Enabled = false;
                    MessageBox.Show("Data tidak ditemukan");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal Load data mahasiswa: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnCetak_Click(object sender, EventArgs e)
        {
            // Membuka Form4 (Report Viewer) dan mengirimkan parameter filter prodi serta tahun
            Form4 frm4 = new Form4(cmbProdi.SelectedValue.ToString(), dtpTanggalMasuk.Value);
            frm4.Show();
            this.Hide();
        }
    }
}