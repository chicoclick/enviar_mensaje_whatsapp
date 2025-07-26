using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace enviarwhatsap
{
    public partial class whatsapp : Form
    {
        public whatsapp()
        {
            InitializeComponent();
        }

        private async void whatsapp_Load(object sender, EventArgs e)
        {
            string normalizedPath = Path.GetFullPath(@"C:\Users\Manuel Gomez\source\repos\enviarwhatsap\enviarwhatsap\bin\Debug\hola.pdf");
            //string normalizedPath = Path.GetFullPath(@"C:\Users\Manuel Gomez\Downloads\Vídeo sin título ‐ Hecho con Clipchamp.mp4");
            //Vídeo sin título ‐ Hecho con Clipchamp.mp4
           if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
            {
                MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Inicializa WebView2
            await webView21.EnsureCoreWebView2Async(null);

            // Navega a WhatsApp Web
            string url = $"https://web.whatsapp.com/send?phone=+529992963226&text={Uri.EscapeDataString("Hola")}";
            webView21.CoreWebView2.Navigate(url);
            await Task.Delay(15000); // Espera para escanear QR
            // Paso 1: Verificar autenticación de WhatsApp Web
            string authCheck = await webView21.ExecuteScriptAsync("document.querySelector('div#side') ? 'Chat encontrado' : 'No autenticado'");
            if (authCheck.Contains("No autenticado"))
            {
                //webView21.Visible = true;
                MessageBox.Show("Por favor, escanea el código QR en WhatsApp Web.", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }else
            {
                //webView21.Visible = false;
            }

            // Copiar y pegar el archivo
            await PasteFile(normalizedPath);
            

        }
        private async Task PasteFile(string filePath)
        {
            try
            {
                // Paso 2: Dar foco a WebView2
                webView21.Focus();
                //await Task.Delay(500);

                // Paso 3: Copiar el archivo al portapapeles
                StringCollection files = new StringCollection { filePath };
                Clipboard.SetFileDropList(files);
                await Task.Delay(500);

                this.Activate();
                SendKeys.SendWait("^v"); // Ctrl+V
                await Task.Delay(1000); // Esperar a que el archivo se cargue
           // Paso 5: Hacer clic en el botón de enviar
                await webView21.CoreWebView2.ExecuteScriptAsync(@"
                setTimeout(function() {
                    
                        var alternativeButton = document.querySelector('span[data-icon=""wds-ic-send-filled""]');
                        if (alternativeButton) {
                            alternativeButton.click();
                            console.log('Botón alternativo (span[data-icon=""wds-ic-send-filled""]) clicado');
                        } else {
                            console.log('Botón alternativo no encontrado');
                        }
                    
                }, 2000);
            ");

                // Esperar a que el archivo se envíe
                await Task.Delay(3000);

                MessageBox.Show("Archivo enviado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       
    }

}


    