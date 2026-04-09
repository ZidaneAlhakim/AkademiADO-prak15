using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Dibutuhkan untuk validasi Regex

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = "Data Source=LAPTOP-M60LBIQK\\ZIDANEAS; Initial Catalog=DBAkademikADO; Integrated Security=True";
        private readonly SqlConnection conn;

        public Form1()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.Items.Clear();
            cmbJK.Items.Add("L");
            cmbJK.Items.Add("P");

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void ClearForm()
        {
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtkodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            txtNIM.Focus();
        }

        // FUNGSI VALIDASI INPUT (Constraint Application Side)
        private bool IsInputValid()
        {
            // 1. Validasi NIM: Harus Angka dan sesuai panjang database (CHAR 11)
            if (!Regex.IsMatch(txtNIM.Text, @"^\d+$"))
            {
                MessageBox.Show("NIM harus berupa angka saja!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNIM.Focus();
                return false;
            }
            if (txtNIM.Text.Length > 11)
            {
                MessageBox.Show("NIM tidak boleh lebih dari 11 karakter!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 2. Validasi Nama: Tidak boleh kosong
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Mahasiswa wajib diisi!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 3. Validasi Jenis Kelamin: Harus dipilih (Check Constraint L/P)
            if (cmbJK.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih Jenis Kelamin!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 4. Validasi Kode Prodi: Harus diisi dan sesuai panjang (CHAR 4)
            if (string.IsNullOrWhiteSpace(txtkodeProdi.Text))
            {
                MessageBox.Show("Kode Prodi wajib diisi!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (txtkodeProdi.Text.Length > 4)
            {
                MessageBox.Show("Kode Prodi maksimal 4 karakter (Contoh: TI01)!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                MessageBox.Show("Koneksi ke database berhasil!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                dataGridView1.Columns.Add("NIM", "NIM");
                dataGridView1.Columns.Add("Nama", "Nama");
                dataGridView1.Columns.Add("JenisKelamin", "L/P");
                dataGridView1.Columns.Add("TanggalLahir", "Tgl Lahir");
                dataGridView1.Columns.Add("Alamat", "Alamat");
                dataGridView1.Columns.Add("KodeProdi", "Prodi");

                string query = "SELECT NIM, NAMA, JenisKelamin, TanggalLahir, Alamat, KodeProdi FROM Mahasiswa";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dataGridView1.Rows.Add(
                        reader["NIM"].ToString(),
                        reader["NAMA"].ToString(),
                        reader["JenisKelamin"].ToString(),
                        Convert.ToDateTime(reader["TanggalLahir"]).ToShortDateString(),
                        reader["Alamat"].ToString(),
                        reader["KodeProdi"].ToString()
                    );
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            // Jalankan Validasi sebelum eksekusi SQL
            if (!IsInputValid()) return;

            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string query = @"INSERT INTO Mahasiswa (NIM, NAMA, JenisKelamin, TanggalLahir, Alamat, KodeProdi) 
                                VALUES (@NIM, @Nama, @JK, @TanggalLahir, @Alamat, @KodeProdi)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@JK", cmbJK.Text);
                cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@KodeProdi", txtkodeProdi.Text);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Data berhasil ditambahkan!");
                    ClearForm();
                    btnLoad_Click(null, null);
                }
            }
            catch (SqlException ex)
            {
                // Menangani error constraint dari sisi SQL Server
                if (ex.Number == 2627) MessageBox.Show("NIM sudah ada di database (Duplicate Primary Key)!");
                else if (ex.Number == 547) MessageBox.Show("Kode Prodi tidak terdaftar di tabel Program Studi (Foreign Key Error)!");
                else MessageBox.Show("Error Database: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!IsInputValid()) return;

            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string query = @"UPDATE Mahasiswa SET NAMA=@Nama, JenisKelamin=@JK, 
                                TanggalLahir=@TanggalLahir, Alamat=@Alamat, KodeProdi=@KodeProdi 
                                WHERE NIM=@NIM";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@JK", cmbJK.Text);
                cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@KodeProdi", txtkodeProdi.Text);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Data berhasil diupdate!");
                    btnLoad_Click(null, null);
                }
                else
                {
                    MessageBox.Show("Data NIM tersebut tidak ditemukan.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal Update: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNIM.Text))
            {
                MessageBox.Show("Pilih atau isi NIM yang ingin dihapus!");
                return;
            }

            if (MessageBox.Show("Hapus data NIM " + txtNIM.Text + "?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    string query = "DELETE FROM Mahasiswa WHERE NIM=@NIM";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Data berhasil dihapus.");
                        ClearForm();
                        btnLoad_Click(null, null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal Hapus: " + ex.Message);
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                txtNIM.Text = row.Cells["NIM"].Value.ToString();
                txtNama.Text = row.Cells["Nama"].Value.ToString();
                cmbJK.Text = row.Cells["JenisKelamin"].Value.ToString();
                dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells["TanggalLahir"].Value);
                txtAlamat.Text = row.Cells["Alamat"].Value.ToString();
                txtkodeProdi.Text = row.Cells["KodeProdi"].Value.ToString();
            }
        }
    }
}