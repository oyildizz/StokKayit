using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StokKayit
{
    public partial class LoginForm : Form
    {
        SqlConnection bagla = new SqlConnection("Data Source=DESKTOP-JK33KA7;Initial Catalog=stoktakipp;Integrated Security=True;Encrypt=False");

        public LoginForm()
        {
            InitializeComponent();
        }

     

        private void button5_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txtKullaniciAdi.Text;
            string sifre = txtSifre.Text;

            SqlCommand cmd = new SqlCommand("SELECT * FROM Kullanicilar WHERE KullaniciAdi=@kadi AND Sifre=@sifre", bagla);
            cmd.Parameters.AddWithValue("@kadi", kullaniciAdi);
            cmd.Parameters.AddWithValue("@sifre", sifre);

            bagla.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                Program.GirisYapanKullanici = dr["KullaniciAdi"].ToString();
                Program.GirisYapanRol = dr["Rol"].ToString();

                this.Hide();
                Form1 f = new Form1();
                f.Show();
            }
            else
            {
                MessageBox.Show("Hatalı giriş!");
            }

            bagla.Close();
        }
    }
}
