using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Microsoft.Web.WebView2.WinForms;
using System.IO;

namespace enviarwhatsap
{
    public partial class whatsapp : Form
    {
        private string numero;
        private string mensaje;
        private string rutaArchivo;
        private bool chatProcesado = false;

        public whatsapp(string numero, string mensaje, string rutaArchivo)
        {
            InitializeComponent();
            this.numero = numero;
            this.mensaje = mensaje;
            this.rutaArchivo = rutaArchivo;

            // Asegúrate de que progressBar1 exista en el diseñador
            // Configúralo en el diseñador: Style = Continuous, Minimum=0, Maximum=100, Value=0
            if (progressBar1 != null)
            {
                progressBar1.Visible = false;
                progressBar1.Value = 0;
            }

            // Inicializa WebView2 si no está en el diseñador
            if (webView21 == null)
            {
                webView21 = new WebView2();
                webView21.Dock = DockStyle.Fill;
                this.Controls.Add(webView21);
            }
        }

        private async void whatsapp_Load(object sender, EventArgs e)
        {
            await EnviarMensajeChat();
        }

        private async Task EnviarMensajeChat()
        {
            string normalizedPath = Path.GetFullPath(rutaArchivo);

            if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
            {
                MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            try
            {
                if (progressBar1 != null)
                {
                    progressBar1.Visible = true;
                    progressBar1.Value = 5;   // Inicio
                }

                await webView21.EnsureCoreWebView2Async(null);

                webView21.NavigationCompleted -= WebView21_NavigationCompleted;
                webView21.NavigationCompleted += WebView21_NavigationCompleted;

                string url = $"https://web.whatsapp.com/send?phone={numero}&text={Uri.EscapeDataString(mensaje)}";
                webView21.CoreWebView2.Navigate(url);

                if (progressBar1 != null) progressBar1.Value = 15;  // Navegación iniciada
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetProgress();
                this.Close();
            }
        }

        private async void WebView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            webView21.NavigationCompleted -= WebView21_NavigationCompleted;

            if (!e.IsSuccess)
            {
                MessageBox.Show($"Navegación fallida: {e.WebErrorStatus}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetProgress();
                this.Close();
                return;
            }

            if (progressBar1 != null) progressBar1.Value = 30;  // Página cargada (QR o chat)

            if (chatProcesado) return;
            chatProcesado = true;

            // Espera el chat (#side)
            int intentos = 0;
            const int maxIntentos = 20;

            while (intentos < maxIntentos)
            {
                string resultado = await webView21.CoreWebView2.ExecuteScriptAsync(
                    "document.querySelector('div#side') ? 'ok' : 'esperando'");

                if (resultado?.Contains("ok") == true)
                {
                    if (progressBar1 != null) progressBar1.Value = 60;  // Chat detectado

                    await Task.Delay(1200);
                    await PasteFile(Path.GetFullPath(rutaArchivo));
                    ResetProgress();
                    this.Close();
                    return;
                }

                if (progressBar1 != null)
                {
                    progressBar1.Value = 30 + (intentos * 2);  // Progreso mientras espera chat (hasta ~70)
                }

                await Task.Delay(2000);
                intentos++;
            }

            MessageBox.Show("No se detectó el chat después de 40 segundos.\n¿Escaneaste el QR?",
                "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ResetProgress();
            this.Close();
        }

        private async Task PasteFile(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                double fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

                if (fileSizeMB > 100)
                {
                    MessageBox.Show("Archivo > 100 MB (límite recomendado).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int pasteDelay = fileSizeMB < 1 ? 1500 : fileSizeMB < 10 ? 4000 : 7000;
                int sendDelay = fileSizeMB < 1 ? 2500 : fileSizeMB < 10 ? 5000 : 9000;

                if (progressBar1 != null) progressBar1.Value = 75;  // Preparando pegado
                webView21.Enabled = true;
                this.Activate();
                
                webView21.Focus();
                await Task.Delay(600);
              
                StringCollection files = new StringCollection { filePath };
                Clipboard.SetFileDropList(files);
               
                await Task.Delay(700);
               
                SendKeys.SendWait("^v");
              
                await Task.Delay(pasteDelay);

                if (progressBar1 != null) progressBar1.Value = 85;  // Archivo pegado, esperando botón

                // Clic en botón Enviar (selectores actualizados 2026)
                string script = @"
                    function clickSend() {
                        const selectors = [
                            '[data-testid=""send""]',
                            'span[data-icon=""send""]',
                            'button > span[data-icon=""send""]',
                            '[data-icon=""send""]',
                            'div[role=""button""][aria-label*=""Enviar""]',
                            'div[role=""button""][data-testid=""send""]'
                        ];

                        for (let sel of selectors) {
                            let btn = document.querySelector(sel);
                            if (btn && btn.offsetParent !== null) {
                                btn.click();
                                return 'ok';
                            }
                        }
                        return 'no';
                    }

                    let tries = 0;
                    const interval = setInterval(() => {
                        if (clickSend() === 'ok' || tries >= 12) {
                            clearInterval(interval);
                        }
                        tries++;
                    }, 900);
                ";

                await webView21.CoreWebView2.ExecuteScriptAsync(script);
                webView21.Enabled = false;

                if (progressBar1 != null) progressBar1.Value = 92;  // Enviando...

                await Task.Delay(sendDelay + 3000);  // Espera subida + envío

                if (progressBar1 != null) progressBar1.Value = 100;

                MessageBox.Show("Archivo enviado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ResetProgress();
            }
        }

        private void ResetProgress()
        {
            if (progressBar1 != null)
            {
                progressBar1.Visible = false;
                progressBar1.Value = 0;
            }
        }

        private void webView21_Click(object sender, EventArgs e) { }
    }
}