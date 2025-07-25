﻿using System.Data.SqlClient;
using StokTakipOtomasyonu.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace StokTakipOtomasyonu.Forms
{
    public partial class SonIslemlerForm : Form
    {
        public SonIslemlerForm()
        {
            this.Icon = new Icon("isp_logo2.ico");
            InitializeComponent();

            // Tarih picker'ları ve comboBox'ı burada başlat
            dtpBaslangic.Value = DateTime.Today.AddDays(-7); // Varsayılan: Son 7 gün
            dtpBitis.Value = DateTime.Today;
            cmbLimit.Items.AddRange(new object[] { "Son 100 İşlem", "Son 200 İşlem", "Hepsini Göster" });
            cmbLimit.SelectedIndex = 0;

            LoadSonIslemler();
            ConfigureDataGridView();
        }

        private void LoadSonIslemler()
        {
            try
            {
                string tarihBaslangic = dtpBaslangic.Value.Date.ToString("yyyy-MM-dd 00:00:00");
                string tarihBitis = dtpBitis.Value.Date.ToString("yyyy-MM-dd 23:59:59");

                string topStr = "";
                if (cmbLimit.SelectedIndex == 0)
                    topStr = "TOP 100";
                else if (cmbLimit.SelectedIndex == 1)
                    topStr = "TOP 200";
                // Hepsini Göster ise TOP yok

                string query = $@"
SELECT {topStr}
    u.urun_barkod, 
    u.urun_kodu, 
    u.urun_adi, 
    CASE uh.hareket_turu 
        WHEN 'Giris' THEN 'Giriş' 
        WHEN 'Cikis' THEN 'Çıkış' 
    END AS hareket_turu,
    uh.miktar, 
    FORMAT(uh.log_date, 'dd.MM.yyyy HH:mm:ss') AS tarih, 
    k.ad_soyad AS kullanici,
    CASE 
        WHEN uh.islem_turu_id = 0 THEN 'Stok'
        WHEN uh.islem_turu_id = 1 THEN 'Proje'
        WHEN uh.islem_turu_id = 2 THEN 'Hurda/İade'
        ELSE ''
    END AS islem_turu,
    (dk.harf + CAST(dk.numara AS NVARCHAR)) AS depo_konum,
    p.proje_kodu,
    uh.aciklama
FROM urun_hareketleri uh
JOIN urunler u ON uh.urun_id = u.urun_id
JOIN kullanicilar k ON uh.kullanici_id = k.kullanici_id
LEFT JOIN projeler p ON uh.proje_id = p.proje_id
LEFT JOIN depo_konum dk ON uh.depo_konum_id = dk.id
WHERE uh.log_date BETWEEN @baslangic AND @bitis
ORDER BY uh.log_date DESC
";
                // Eğer topStr varsa (TOP 100/200), direkt başa ekliyor

                // Veritabanı bağlantısı (DatabaseHelper yerine doğrudan veya kendi MSSQL uyumlu helper'ını yaz)
                using (SqlConnection conn = DatabaseHelper.GetConnection())

                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@baslangic", tarihBaslangic);
                        cmd.Parameters.AddWithValue("@bitis", tarihBitis);
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Son işlemler yüklenirken hata oluştu: " + ex.Message,
                                "Hata",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns["urun_barkod"].HeaderText = "Barkod";
                dataGridView1.Columns["urun_kodu"].HeaderText = "Ürün Kodu";
                dataGridView1.Columns["urun_adi"].HeaderText = "Ürün Adı";
                dataGridView1.Columns["hareket_turu"].HeaderText = "Hareket Türü";
                dataGridView1.Columns["miktar"].HeaderText = "Miktar";
                dataGridView1.Columns["tarih"].HeaderText = "Tarih";
                dataGridView1.Columns["kullanici"].HeaderText = "Kullanıcı";
                dataGridView1.Columns["islem_turu"].HeaderText = "İşlem Türü";
                dataGridView1.Columns["depo_konum"].HeaderText = "Depo Konumu"; // EKLEDİK
                dataGridView1.Columns["proje_kodu"].HeaderText = "Proje Kodu";
                dataGridView1.Columns["aciklama"].HeaderText = "Açıklama";
            }


        }

        private void btnKapat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnYenile_Click(object sender, EventArgs e)
        {
            LoadSonIslemler();
        }

        private void btnFiltrele_Click(object sender, EventArgs e)
        {
            LoadSonIslemler();
        }

        private void cmbLimit_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSonIslemler();
        }

        private void dtpBaslangic_ValueChanged(object sender, EventArgs e)
        {
            // Otomatik yenilemek istiyorsan:
            // LoadSonIslemler();
        }

        private void dtpBitis_ValueChanged(object sender, EventArgs e)
        {
            // Otomatik yenilemek istiyorsan:
            // LoadSonIslemler();
        }
    }
}
