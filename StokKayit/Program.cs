using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokKayit
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        public static string GirisYapanKullanici = "";
        public static string GirisYapanRol = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm()); // Giriş formundan başlasın
        }
    }
}
