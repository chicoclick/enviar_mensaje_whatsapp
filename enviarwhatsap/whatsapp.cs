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
            if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
            {
                MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Inicializa WebView2
            await webView21.EnsureCoreWebView2Async(null);

            // Configurar evento DragDrop de WebView2 (para arrastres manuales)
           /* webView21.DragDrop += async (s, e) =>
            {
                e.Effect = DragDropEffects.Copy;
               // e.Handled = true;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0 && Path.GetExtension(files[0]).ToLower().Equals(".pdf"))
                    {
                        await PasteFile(files[0]);
                    }
                }
            };
            webView21.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            };*/

            // Navega a WhatsApp Web
            string url = $"https://web.whatsapp.com/send?phone=+529992963226&text={Uri.EscapeDataString("Hola")}";
            webView21.CoreWebView2.Navigate(url);
            await Task.Delay(15000); // Espera para escanear QR
            // Paso 1: Verificar autenticación de WhatsApp Web
            string authCheck = await webView21.ExecuteScriptAsync("document.querySelector('div#side') ? 'Chat encontrado' : 'No autenticado'");
            if (authCheck.Contains("No autenticado"))
            {
                MessageBox.Show("Por favor, escanea el código QR en WhatsApp Web.", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Copiar y pegar el archivo
            await PasteFile(normalizedPath);
            //await drag_and_drop(normalizedPath);

        }
        private async Task PasteFile(string filePath)
        {
            try
            {
              

                // Paso 2: Dar foco a WebView2
                webView21.Focus();
                await Task.Delay(500);

                // Paso 3: Copiar el archivo al portapapeles
                StringCollection files = new StringCollection { filePath };
                Clipboard.SetFileDropList(files);
                await Task.Delay(500);

                // Paso 4: Simular pegado (Ctrl+V) en el área de chat
                await webView21.ExecuteScriptAsync(@"
            setTimeout(() => {
                var chatArea = document.querySelector('#main') || document.querySelector('div[role=""main""]');
                if (chatArea) {
                    chatArea.focus();
                    console.log(JSON.stringify({ message: 'Área de chat enfocada', element: true }));
                } else {
                    console.log(JSON.stringify({ message: 'Área de chat no encontrada', element: false }));
                }
            }, 1000);
        ");
                this.Activate();
                SendKeys.SendWait("^v"); // Ctrl+V
                await Task.Delay(3000); // Esperar a que el archivo se cargue
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


    