using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace enviarwhatsap
{
    public partial class whatsapp : Form
    {
        

        private string numero;
        private string mensaje;
        private string rutaArchivo;
        private int maxAttempts = 15; // Máximo número de intentos para verificar el chat (15 * 2 segundos = 30 segundos máximo)

        public whatsapp(string numero, string mensaje, string rutaArchivo)
        {
            InitializeComponent();
            this.numero = numero;
            this.mensaje = mensaje;
            this.rutaArchivo = rutaArchivo;
        }

        private void whatsapp_Load(object sender, EventArgs e)
        {

            enviarmensajechat();

        }

        private async void enviarmensajechat()
            {
            //string normalizedPath = Path.GetFullPath(@"C:\Users\Manuel Gomez\source\repos\enviarwhatsap\enviarwhatsap\bin\Debug\hola.pdf");
            string normalizedPath = Path.GetFullPath(rutaArchivo);
            //string normalizedPath = Path.GetFullPath(@"C:\Users\Manuel Gomez\Downloads\Vídeo sin título ‐ Hecho con Clipchamp.mp4");
            //Vídeo sin título ‐ Hecho con Clipchamp.mp4
            if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
            {
                MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                //return;
            }
            // Inicializa WebView2
            await webView21.EnsureCoreWebView2Async(null);

            // Navega a WhatsApp Web
            //string url = $"https://web.whatsapp.com/send?phone=+529992963226&text={Uri.EscapeDataString("Hola")}";
            string url = $"https://web.whatsapp.com/send?phone={numero}&text={Uri.EscapeDataString(mensaje)}";
            webView21.CoreWebView2.Navigate(url);
            await Task.Delay(5000); // Espera para escanear QR
                                    // Verificar si el chat está cargado o si sigue en la pantalla de carga
            bool isChatLoaded = false;
            //int maxAttempts = 15; // Máximo número de intentos (30 * 2 segundos = 60 segundos máximo)
            int attempt = 0;

            while (!isChatLoaded && attempt < maxAttempts)
            {
                // Verificar si el elemento del chat (#side) está presente
                string authCheck = await webView21.ExecuteScriptAsync("document.querySelector('div#side') ? 'Chat encontrado' : 'No autenticado'");
                // Verificar si la pantalla de carga está presente
                //string loadingCheck = await webView21.ExecuteScriptAsync("document.querySelector('div._aiwn') ? 'Cargando' : 'No cargando'");

                if (authCheck.Contains("Chat encontrado"))
                {
                    isChatLoaded = true; // El chat está cargado y la pantalla de carga ya no está
                }
                else
                {
                    // Esperar antes de intentar de nuevo
                    await Task.Delay(2000); // Esperar 2 segundos entre intentos
                    attempt++;
                }
            }

            // Verificar si se agotaron los intentos
            if (!isChatLoaded)
            {
                MessageBox.Show("No se pudo cargar el chat de WhatsApp Web o no se autenticó. Por favor, escanea el código QR.", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
               
            }
            else 
            {
                // Proceder a enviar el archivo
                await PasteFile(normalizedPath);
                this.Close();
            }

            
            // Paso 1: Verificar autenticación de WhatsApp Web
            /*string authCheck = await webView21.ExecuteScriptAsync("document.querySelector('div#side') ? 'Chat encontrado' : 'No autenticado'");
            if (authCheck.Contains("No autenticado"))
            {
                //webView21.Visible = true;
                MessageBox.Show("Por favor, escanea el código QR en WhatsApp Web.", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                //return;
            }
            else
            {
                //webView21.Visible = false;
                // Copiar y pegar el archivo
                await PasteFile(normalizedPath);
                this.Close();
            }*/
        }
        private async Task PasteFile(string filePath)
        {
            try
            {
                //area de tamano doc y tiempo recomendado
                // Obtener el tamaño del archivo en bytes
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSizeBytes = fileInfo.Length;
                double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0); // Convertir a MB

                // Validar tamaño máximo de WhatsApp
                if (fileSizeMB > 100)
                {
                    MessageBox.Show("El archivo excede el límite de 100 MB de WhatsApp.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    //return;
                }

                // Calcular tiempos de espera dinámicos basados en el tamaño del archivo
                int pasteDelayMs;
                int sendDelayMs;

                if (fileSizeMB < 1) // Pequeño (< 1 MB)
                {
                    pasteDelayMs = 1000; // 1 segundo para pegar
                    sendDelayMs = 2000;  // 2 segundos para enviar
                }
                else if (fileSizeMB < 10) // Mediano (1–10 MB)
                {
                    pasteDelayMs = 3000; // 3 segundos para pegar
                    sendDelayMs = 4000;  // 4 segundos para enviar
                }
                else // Grande (> 10 MB)
                {
                    pasteDelayMs = 5000; // 5 segundos para pegar
                    sendDelayMs = 6000;  // 6 segundos para enviar
                }

                // Paso 2: Dar foco a WebView2
                webView21.Focus();
                //await Task.Delay(500);

                // Paso 3: Copiar el archivo al portapapeles
                StringCollection files = new StringCollection { filePath };
                Clipboard.SetFileDropList(files);
                await Task.Delay(500);

                this.Activate();
                SendKeys.SendWait("^v"); // Ctrl+V
                await Task.Delay(pasteDelayMs); // Esperar a que el archivo se cargue
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

                // Paso 5: Esperar a que el archivo se envíe
                await Task.Delay(sendDelayMs + 1000); // Añadir un pequeño margen después de enviar

                MessageBox.Show("Archivo enviado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       
    }

}


    