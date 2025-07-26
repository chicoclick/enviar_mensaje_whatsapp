using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace enviarwhatsap
{
    public partial class iniciowhatsapp : Form
    {
        public iniciowhatsapp()
        {
            InitializeComponent();
        }

        private async void iniciowhatsapp_Load(object sender, EventArgs e)
        {
            // Inicializa WebView2
            await webView21.EnsureCoreWebView2Async(null);

            // Navega a WhatsApp Web
            string url = $"https://web.whatsapp.com";
            webView21.CoreWebView2.Navigate(url);
        }
    }
}
