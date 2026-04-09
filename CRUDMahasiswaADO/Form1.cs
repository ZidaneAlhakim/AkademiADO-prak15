using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions;

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

        private bool IsInputValid()
        {
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
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Mahasiswa wajib diisi!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbJK.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih Jenis Kelamin!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtkodeProdi.Text))
            {
                MessageBox.Show("Kode Prodi wajib diisi!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                // Pop-up Informasi Koneksi
                MessageBox.Show("Koneksi ke database berhasil!", "Status Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                int count = 0;
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
                    count++;
                }
                reader.Close();

                // Pop-up Informasi Jumlah Data yang Dimuat
                MessageBox.Show($"Berhasil memuat {count} data mahasiswa.", "Informasi Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!IsInputValid()) return;

            // POP-UP BARU: Konfirmasi sebelum menyimpan
            DialogResult dialogResult = MessageBox.Show($"Yakin ingin menambah data Mahasiswa:\nNIM: {txtNIM.Text}\nNama: {txtNama.Text}?",
                                        "Konfirmasi Tambah Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No) return;

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
                    MessageBox.Show("Data mahasiswa berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    btnLoad_Click(null, null);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) MessageBox.Show("NIM sudah ada di database!", "Primary Key Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                else if (ex.Number == 547) MessageBox.Show("Kode Prodi tidak terdaftar!", "Foreign Key Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                else MessageBox.Show("Error Database: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!IsInputValid()) return;

            // POP-UP BARU: Konfirmasi sebelum mengubah
            DialogResult dialogResult = MessageBox.Show($"Simpan perubahan untuk NIM: {txtNIM.Text}?",
                                        "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No) return;

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
                    MessageBox.Show("Data berhasil diperbarui!", "Update Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Pilih atau isi NIM yang ingin dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Pop-up Konfirmasi Hapus
            if (MessageBox.Show($"Apakah anda yakin ingin menghapus permanen data NIM: {txtNIM.Text}?",
                "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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
                        MessageBox.Show("Data berhasil dihapus selamanya.", "Hapus Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
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