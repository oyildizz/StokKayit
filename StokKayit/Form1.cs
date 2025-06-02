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
using System.Drawing.Printing;
using System.Globalization;


namespace StokKayit
{
    public partial class Form1 : Form
    {
        private PrintDocument printDoc;
        private Font font;
        private int currentY;
        private bool printSalesPrice = true; // Satış fiyatı yazdırılacak mı?

        public Form1()
        {
            InitializeComponent();
            checkBox1.Checked = false;
            listele();
            CalculateAndDisplayTotals();

      
            printDoc = new PrintDocument();
            printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);
            font = new Font("Arial", 10);

 

        }
        SqlConnection bagla = new SqlConnection("Data Source=DESKTOP-JK33KA7;Initial Catalog=stoktakipp;Integrated Security=True;Encrypt=False");

        public void verilerigoster(string veriler)
        {
            SqlDataAdapter da = new SqlDataAdapter(veriler,bagla);
            DataSet ds = new DataSet();
            da.Fill(ds);
            dataGridView1.DataSource = ds.Tables[0];
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            verilerigoster("select * from stoktakiptablosuu");
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (bagla.State == ConnectionState.Closed)
                    bagla.Open();

                // Verileri al ve toplammaliyet hesapla
                string urunKodu = textBox1.Text;
                decimal alisFiyati = Convert.ToDecimal(textBox2.Text);
                decimal satisFiyati = Convert.ToDecimal(textBox3.Text);
                int miktar = Convert.ToInt32(textBox4.Text);
                decimal toplamMaliyet = alisFiyati * miktar;

                SqlCommand komut = new SqlCommand("INSERT INTO stoktakiptablosuu (ürünkodu, alışfiyatı, satışfiyatı, miktarı, toplammaliyet) VALUES (@ürünkodu, @alışfiyatı, @satışfiyatı, @miktarı, @toplammaliyet)", bagla);

                komut.Parameters.AddWithValue("@ürünkodu", urunKodu);
                komut.Parameters.AddWithValue("@alışfiyatı", alisFiyati);
                komut.Parameters.AddWithValue("@satışfiyatı", satisFiyati);
                komut.Parameters.AddWithValue("@miktarı", miktar);
                komut.Parameters.AddWithValue("@toplammaliyet", toplamMaliyet); // BU SATIR EKSİKTİ!

                int result = komut.ExecuteNonQuery();

                if (result > 0)
                    MessageBox.Show("Veri başarıyla eklendi.");
                else
                    MessageBox.Show("Veri eklenemedi.");

                verilerigoster("SELECT * FROM stoktakiptablosuu");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
            finally
            {
                if (bagla.State == ConnectionState.Open)
                    bagla.Close();

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();

                CalculateAndDisplayTotals();
            }
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                textBox1.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                textBox2.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                textBox3.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
                textBox4.Text = dataGridView1.CurrentRow.Cells[3].Value.ToString();
            }
            else
            {
                MessageBox.Show("Lütfen geçerli bir satır seçin.");
            }

           

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Alış fiyatı veya miktar değiştiğinde toplam maliyeti hesapla
            if (e.RowIndex >= 0 && (e.ColumnIndex == 1 || e.ColumnIndex == 3)) // Alış fiyatı veya miktar sütunu
            {
                CalculateRowTotal(e.RowIndex);
            }
        }
        private void CalculateRowTotal(int rowIndex)
        {
            // DataGridView'den ilgili satırı al
            var row = dataGridView1.Rows[rowIndex];

            // Alış fiyatı ve miktar değerlerini al
            if (decimal.TryParse(row.Cells["alışfiyatı"].Value?.ToString(), out decimal alışfiyatı) &&
                int.TryParse(row.Cells["miktarı"].Value?.ToString(), out int miktarı))
            {
                // Toplam maliyeti hesapla
                decimal toplamMaliyet = alışfiyatı * miktarı;

                // Toplam maliyeti güncelle
                row.Cells["ToplamMaliyet"].Value = toplamMaliyet;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Seçilen satırdaki malzeme kodunu al
            string t1 = textBox1.Text;

            // Kullanıcıya silme işlemi için onay sorusu göster
            DialogResult dialogResult = MessageBox.Show("Bu ürünü stoktan silmek istediğinizden emin misiniz?", "Ürün Silme", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                // Kullanıcı 'Evet' dedi, ürünü sil
                bagla.Open();
                SqlCommand komut = new SqlCommand("DELETE FROM stoktakiptablosuu WHERE ürünkodu = @ürünkodu", bagla);
                komut.Parameters.AddWithValue("@ürünkodu", t1); // Parametreyi doğru şekilde ekleyelim
                komut.ExecuteNonQuery();
                bagla.Close();

                // Silme işleminden sonra verileri listele
                listele();
                CalculateAndDisplayTotals();

                // Kullanıcıya bilgilendirme mesajı göster
                MessageBox.Show("Ürün başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Kullanıcı 'Hayır' dedi, silme işlemini iptal et
                MessageBox.Show("Ürün silme işlemi iptal edildi.", "İptal", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void listele()
        {
            bagla.Open();
            SqlDataAdapter da = new SqlDataAdapter("Select *from stoktakiptablosuu", bagla);
            DataTable tablo = new DataTable();
            da.Fill(tablo);
            dataGridView1.DataSource = tablo;
            bagla.Close();

            dataGridView1.Columns["alışfiyatı"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["satışfiyatı"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["toplammaliyet"].DefaultCellStyle.Format = "C2";
        }

        private void button4_Click(object sender, EventArgs e)
        {

            String t1 = textBox1.Text; //ürünkodu
            decimal t2 = Convert.ToDecimal(textBox2.Text); // alış fiyatı
            decimal t3 = Convert.ToDecimal(textBox3.Text); // satış fiyatı
            decimal t4 = Convert.ToInt32(textBox4.Text); // miktar

            // Toplam maliyeti hesapla (alış fiyatı * miktar)
            decimal toplamMaliyet = t2 * t4;

            bagla.Open();
            SqlCommand komut = new SqlCommand("UPDATE stoktakiptablosuu SET ürünkodu = @ürünkodu, alışfiyatı = @alışfiyatı, satışfiyatı = @satışfiyatı, miktarı = @miktarı, toplammaliyet = @toplammaliyet,tarih = GETDATE()  WHERE ürünkodu = @ürünkodu", bagla);

            // Parametre ekleme (SQL Injection'dan kaçınmak için)
            komut.Parameters.AddWithValue("@ürünkodu", t1);
            komut.Parameters.AddWithValue("@alışfiyatı", t2);
            komut.Parameters.AddWithValue("@satışfiyatı", t3);
            komut.Parameters.AddWithValue("@miktarı", t4);
            komut.Parameters.AddWithValue("@toplammaliyet", toplamMaliyet);

            komut.ExecuteNonQuery();
            bagla.Close();
            listele();
            CalculateAndDisplayTotals();
        }

        private void CalculateAndDisplayTotals()
        {
            // DataGridView1'deki verileri kontrol et
            DataTable dt = (DataTable)dataGridView1.DataSource;

            decimal toplamMaliyet = 0;
            int toplamMiktar = 0;

            // Toplamları hesapla
            foreach (DataRow row in dt.Rows)
            {
                // "toplammaliyet" sütunu için değer kontrolü
                decimal toplamMaliyetSatir = 0;
                if (row["toplammaliyet"] != DBNull.Value && decimal.TryParse(row["toplammaliyet"].ToString(), out toplamMaliyetSatir))
                {
                    // Eğer geçerli bir decimal ise, değer alınır.
                }
                // "miktarı" sütunu için değer kontrolü
                int miktar = 0;
                if (row["miktarı"] != DBNull.Value && int.TryParse(row["miktarı"].ToString(), out miktar))
                {
                    // Eğer geçerli bir int ise, değer alınır.
                }

                // Toplam maliyet ve miktarı güncelle
                toplamMaliyet += toplamMaliyetSatir;
                toplamMiktar += miktar;
            }

            // Toplam değerleri Label veya TextBox gibi kontrollerde göster
            lblToplamMiktar.Text = toplamMiktar.ToString(); // Toplam miktarı bir Label'da göster
            lblToplamMaliyet.Text = toplamMaliyet.ToString("C"); // Toplam maliyeti para birimi formatında göster
        }
        private void OnDataChanged(object sender, EventArgs e)
        {
            // Veriler güncellendiğinde toplamları yeniden hesapla
            CalculateAndDisplayTotals();
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"Stok Takip - Hoşgeldiniz: {Program.GirisYapanKullanici} ({Program.GirisYapanRol})";

            if (Program.GirisYapanRol != "Admin")
            {
                button2.Enabled = false; // Ekle
                button3.Enabled = false; // Sil
                button4.Enabled = false; // Güncelle
                button4.Enabled = false; // Satış
            }
        }

        private void txtUrunKoduAra_TextChanged(object sender, EventArgs e)
        {
            string arananKod = txtUrunKoduAra.Text.Trim(); // Arama kutusundaki metni al
            DataView dv;

            // DataGridView'in veri kaynağını DataView olarak al
            if (dataGridView1.DataSource is DataTable dt)
            {
                dv = dt.DefaultView; // DataTable'den DataView'e geçiş
            }
            else if (dataGridView1.DataSource is DataView dataView)
            {
                dv = dataView;
            }
            else
            {
                MessageBox.Show("Geçerli bir veri kaynağı bulunamadı.");
                return;
            }

            // Eğer arama kutusu boşsa tüm verileri göster
            if (string.IsNullOrEmpty(arananKod))
            {
    
                dv.RowFilter = string.Empty; // Filtreyi temizle

            }
            else
            {
                // Girilen metni aramak için filtre uygula
                dv.RowFilter = $"[ürünkodu] LIKE '%{arananKod}%'";
            }

            // DataGridView'in veri kaynağını güncelle
            dataGridView1.DataSource = dv;
        }

        private void buttonSatışYap_Click(object sender, EventArgs e)
        {
     
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Seçilen ürünü ve satış miktarını al
            string ürünkodu = textBox1.Text;
            int satışMiktarı = Convert.ToInt32(textBoxSatışMiktarı.Text); // Satış miktarını girdiğimiz TextBox

            // Mevcut stok miktarını ve alış fiyatını al
            decimal mevcutStokMiktarı = 0;
            decimal alışFiyatı = 0;
            bagla.Open();
            SqlCommand stokKomut = new SqlCommand("SELECT miktarı, alışfiyatı FROM stoktakiptablosuu WHERE ürünkodu = @ürünkodu", bagla);
            stokKomut.Parameters.AddWithValue("@ürünkodu", ürünkodu);
            SqlDataReader reader = stokKomut.ExecuteReader();
            if (reader.Read())
            {
                mevcutStokMiktarı = Convert.ToInt32(reader["miktarı"]);
                alışFiyatı = Convert.ToDecimal(reader["alışfiyatı"]);
            }
            bagla.Close();

            // Satış işlemi mevcut stoktan fazla olamaz
            if (satışMiktarı > mevcutStokMiktarı)
            {
                MessageBox.Show("Yeterli stok yok! Satış miktarını kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Yeni stok miktarını hesapla
            decimal yeniStokMiktarı = mevcutStokMiktarı - satışMiktarı;

            // Toplam maliyeti hesapla (alış fiyatı * satılan miktar)
            decimal toplamMaliyet = alışFiyatı * yeniStokMiktarı;

            // Satış sonrası yeni stok miktarını ve toplam maliyeti veritabanında güncelle
            bagla.Open();
            SqlCommand güncelleKomut = new SqlCommand("UPDATE stoktakiptablosuu SET miktarı = @yeniStok, toplammaliyet = @toplammaliyet WHERE ürünkodu = @ürünkodu", bagla);
            güncelleKomut.Parameters.AddWithValue("@yeniStok", yeniStokMiktarı);
            güncelleKomut.Parameters.AddWithValue("@toplammaliyet", toplamMaliyet);
            güncelleKomut.Parameters.AddWithValue("@ürünkodu", ürünkodu);

            güncelleKomut.ExecuteNonQuery();
            bagla.Close();

            // Veritabanındaki stok bilgilerini güncelledikten sonra listeyi yeniden göster
            listele();
            CalculateAndDisplayTotals();

            // Kullanıcıya başarı mesajı
            MessageBox.Show("Satış başarıyla yapıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Satış miktarını sıfırlayalım
            textBoxSatışMiktarı.Clear();
        }

        private void lblToplamMaliyet_Click(object sender, EventArgs e)
        {

        }

        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            int y = 20; // Başlangıç Y konumu
            int x = 20; // Başlangıç X konumu

            // Sütun genişlikleri
            int[] columnWidths = new int[] { 120, 110, 110, 110, 110 };

            // Başlıkları yazdırıyoruz
            e.Graphics.DrawString("Ürün Kodu", font, Brushes.Black, x, y);
            e.Graphics.DrawString("Alış Fiyatı", font, Brushes.Black, x + columnWidths[0] + 10, y);

            // Eğer satış fiyatı yazdırılacaksa, başlıkları ekliyoruz
            int salesPriceColumnWidth = 0;  // Initialize sales price column width to 0
            if (!checkBox1.Checked)  // Only add the Sales Price column if checkbox is unchecked
            {
                e.Graphics.DrawString("Satış Fiyatı", font, Brushes.Black, x + columnWidths[0] + columnWidths[1] + 20, y);
                salesPriceColumnWidth = columnWidths[2]; // Sales price column takes up space
            }

            e.Graphics.DrawString("Miktar", font, Brushes.Black, x + columnWidths[0] + columnWidths[1] + salesPriceColumnWidth + 30, y);
            e.Graphics.DrawString("Toplam Maliyet", font, Brushes.Black, x + columnWidths[0] + columnWidths[1] + salesPriceColumnWidth + columnWidths[3] + 40, y);

            y += 20; // Başlıkları yazdırdıktan sonra satır aralığı

            // Toplam maliyet ve toplam stok hesaplamaları
            decimal toplamMaliyet = 0;
            decimal toplamStok = 0;

            // Verileri yazdırıyoruz
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ürünkodu"].Value != null)
                {
                    // Ürün Kodu
                    e.Graphics.DrawString(row.Cells["ürünkodu"].Value.ToString(), font, Brushes.Black, x, y);

                    // Alış Fiyatı
                    e.Graphics.DrawString(Convert.ToDecimal(row.Cells["alışfiyatı"].Value).ToString("C2", CultureInfo.GetCultureInfo("tr-TR")), font, Brushes.Black, x + columnWidths[0] + 10, y);

                    // Eğer satış fiyatı yazdırılacaksa, satış fiyatını yazdırıyoruz
                    if (!checkBox1.Checked)  // Only print Sales Price if checkbox is unchecked
                    {
                        e.Graphics.DrawString(Convert.ToDecimal(row.Cells["satışfiyatı"].Value).ToString("C2", CultureInfo.GetCultureInfo("tr-TR")), font, Brushes.Black, x + columnWidths[0] + columnWidths[1] + 20, y);
                    }

                    // Miktar
                    e.Graphics.DrawString(row.Cells["miktarı"].Value.ToString(), font, Brushes.Black, x + columnWidths[0] + columnWidths[1] + salesPriceColumnWidth + 30, y);

                    // Toplam Maliyet
                    e.Graphics.DrawString(Convert.ToDecimal(row.Cells["toplamMaliyet"].Value).ToString("C2", CultureInfo.GetCultureInfo("tr-TR")), font, Brushes.Black, x + columnWidths[0] + columnWidths[1] + salesPriceColumnWidth + columnWidths[3] + 40, y);

                    // Toplam maliyeti ve toplam stok hesapla
                    toplamMaliyet += Convert.ToDecimal(row.Cells["toplamMaliyet"].Value);
                    toplamStok += Convert.ToDecimal(row.Cells["miktarı"].Value);

                    y += 20; // Bir sonraki satır için y konumunu artırıyoruz
                }
            }

            // Alt kısmına toplam stok ve toplam maliyet bilgilerini yazdırıyoruz
            y += 20; // Sonraki bilgi için boşluk bırakıyoruz

            // Toplam stok önce, toplam maliyet sonra olacak şekilde yazdırıyoruz
            e.Graphics.DrawString($"Toplam Stok: {toplamStok}", font, Brushes.Black, x, y);  // Toplam stok yazdır
            e.Graphics.DrawString($"Toplam Maliyet: {toplamMaliyet:C2}", font, Brushes.Black, x + 200, y);  // Toplam maliyet yazdır

            y += 20; // Alt kısımdaki bilgileri yazdırdıktan sonra son satır için boşluk bırakıyoruz
        }



        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDoc;

                // Event handler'ı kontrol et
                printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);  // Burada bağladığınızdan emin olun.

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();  // Yazdırma işlemi başlat
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazdırma işlemi sırasında hata oluştu: " + ex.Message);
            }
        }
        private void lblToplamMiktar_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            printSalesPrice = checkBox1.Checked;
        }

        private void textBoxSatışMiktarı_TextChanged(object sender, EventArgs e)
        {

        }

    }
  
}       
