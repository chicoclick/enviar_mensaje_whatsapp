using Microsoft.Web.WebView2.Core;
using System;
using System.Dynamic;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace enviarwhatsap
{
    public partial class ventana_envio : Form
    {
        private string numero;
        private string mensaje;
        private string rutaArchivo;

        public ventana_envio(string numero, string mensaje, string rutaArchivo)
        {
            InitializeComponent();
            this.numero = numero;
            this.mensaje = mensaje;
            this.rutaArchivo = rutaArchivo;
        }

        private async void ventana_envio_Load(object sender, EventArgs e)
        {
            // Inicializa WebView2
            await webView21.EnsureCoreWebView2Async(null);

            // Configura manejador para mensajes de la consola
            webView21.CoreWebView2.WebMessageReceived += (sender, args) =>
            {
                MessageBox.Show(args.WebMessageAsJson, "Mensaje de JavaScript");
            };

            // Navega a WhatsApp Web con el número y mensaje
            string url = $"https://web.whatsapp.com/send?phone={numero}&text={Uri.EscapeDataString(mensaje)}";
            webView21.CoreWebView2.Navigate(url);

            // Esperar a que la navegación se complete
            TaskCompletionSource<bool> navigationCompleted = new TaskCompletionSource<bool>();
            EventHandler<CoreWebView2NavigationCompletedEventArgs> navigationHandler = null;
            navigationHandler = (s, args) =>
            {
                if (args.IsSuccess)
                {
                    navigationCompleted.SetResult(true);
                }
                else
                {
                    navigationCompleted.SetResult(false);
                    MessageBox.Show("Error al cargar WhatsApp Web.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                webView21.NavigationCompleted -= navigationHandler;
            };
            webView21.NavigationCompleted += navigationHandler;

            // Esperar a que la navegación termine
            bool navigationSuccess = await navigationCompleted.Task;
            if (!navigationSuccess)
            {
                return;
            }

            // Espera adicional para que WhatsApp Web cargue completamente
            await Task.Delay(2000); // 20 segundos para carga lenta

            // Verifica si WhatsApp Web está autenticado
            /*string authCheck = await webView21.CoreWebView2.ExecuteScriptAsync("document.querySelector('div._3FRCZ') ? 'Chat encontrado' : 'No autenticado';");
            if (authCheck.Contains("No autenticado"))
            {
                MessageBox.Show("Por favor, escanea el código QR en WhatsApp Web para autenticarte.", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }*/

            // Enviar el mensaje de texto
            await EnviarMensajeTexto();

            // Si hay un archivo PDF, súbelo
            if (!string.IsNullOrEmpty(rutaArchivo) && File.Exists(rutaArchivo))
            {
                await SubirArchivo2();
            }

            // Cierra la ventana después de completar las acciones
            await Task.Delay(40000);
            this.Close();
        }
        private async Task EnviarMensajeTexto()
        {
            // Ejecuta JavaScript para hacer clic en el botón de enviar
            await webView21.ExecuteScriptAsync(@"
                setTimeout(function() {
                    var sendButton = document.querySelector('button[aria-label=""Enviar""]');
                    if (sendButton) {
                        sendButton.click();
                    } else {
                        console.log('Botón de enviar mensaje no encontrado');
                    }
                }, 5000);
            ");
            await Task.Delay(15000); // Esperar a que el mensaje se envíe
        }

        private async Task SubirArchivo()
        {
            // Validar que la ruta del archivo sea válida
            if (!File.Exists(rutaArchivo))
            {
                MessageBox.Show($"El archivo no existe en la ruta: {rutaArchivo}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Normalizar la ruta del archivo
            string normalizedPath = Path.GetFullPath(rutaArchivo);
            if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
            {
                MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Paso 1: Hacer clic en el botón de adjunto (clip) y seleccionar "Documento"
            await webView21.ExecuteScriptAsync(@"
                setTimeout(function() {
                    var attachButton = document.querySelector('div[title=""Adjuntar""]') || document.querySelector('div[aria-label=""Adjuntar""]');
                    if (attachButton) {
                        attachButton.click();
                        setTimeout(function() {
                            // Seleccionar la opción de 'Documento'
                            var docOption = Array.from(document.querySelectorAll('input[type=""file""]')).find(input => input.accept.includes('pdf'));
                            if (docOption) {
                                docOption.click();
                            } else {
                                console.log('Campo de entrada de archivo para documentos no encontrado');
                            }
                        }, 2000);
                    } else {
                        console.log('Botón de adjunto no encontrado');
                    }
                }, 2000);
            ");

            // Paso 2: Esperar a que el cuadro de diálogo de selección de archivo esté listo
            await Task.Delay(5000); // Aumentado para asegurar que el diálogo aparezca

            // Paso 3: Enfocar la ventana actual y enviar la ruta del archivo
            this.Activate();
            SendKeys.SendWait(normalizedPath);
            await Task.Delay(1500); // Aumentado para asegurar que la ruta se escriba
            SendKeys.SendWait("{ENTER}");

            // Paso 4: Esperar a que el archivo se cargue y enviar
            await Task.Delay(6000); // Aumentado para archivos grandes
            await webView21.ExecuteScriptAsync(@"
                setTimeout(function() {
                    var sendFileButton = document.querySelector('button[aria-label=""Enviar""]');
                    if (sendFileButton) {
                        sendFileButton.click();
                    } else {
                        console.log('Botón de enviar archivo no encontrado');
                    }
                }, 2000);
            ");
        }

        private async Task SubirArchivo2()
        {
            string normalizedPath = Path.GetFullPath(rutaArchivo);
            if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
            {
                MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Paso 1: Haz clic en el botón de adjunto (clip)
            await webView21.ExecuteScriptAsync(@"
                 setTimeout(function() {
                     var attachButton = document.querySelector('div[title=""Adjuntar""]');
                        
                     if (attachButton) attachButton.click();
                     setTimeout(function() {
                            console.log(attachButton);
                         var docOption = document.querySelector('input[type=""file""]');

                                console.log(docOption);
                         if (docOption) docOption.click();
                     }, 100);
                 }, 500);
             ");

            


            // Paso 2: Espera a que aparezca el cuadro de diálogo de selección de archivo
            await Task.Delay(5000);

            // Paso 3: Usa SendKeys para escribir la ruta del archivo y presionar Enter
            SendKeys.Send(normalizedPath);
            await Task.Delay(4000);
            
            SendKeys.SendWait("{ENTER}");
            

            // Paso 4: Espera a que el archivo se cargue y haz clic en enviar
            await Task.Delay(5000); // Ajustar según el tamaño del archivo
            /* await webView21.ExecuteScriptAsync(@"
                 setTimeout(function() {
                     var sendFileButton = document.querySelector('button[aria-label=""Enviar""]');
                     if (sendFileButton) sendFileButton.click();
                 }, 2000);
             ");*/



            // Paso 5: Hacer clic en el botón de enviar (ícono de avión de papel)
            /* await webView21.CoreWebView2.ExecuteScriptAsync(@"
                 setTimeout(function() {
                     var sendFileButton = document.querySelector('div[aria-label=""Enviar""]');
                     if (sendFileButton) {
                         sendFileButton.click();
                         console.log('Botón de enviar archivo (div[aria-label=""Enviar""]) clicado');
                     } else {
                         console.log('Botón de enviar archivo (div[aria-label=""Enviar""]) no encontrado. Buscando alternativa...');
                         var alternativeButton = document.querySelector('span[data-icon=""wds-ic-send-filled""]');
                         if (alternativeButton) {
                             alternativeButton.click();
                             console.log('Botón alternativo (span[data-icon=""wds-ic-send-filled""]) clicado');
                         } else {
                             console.log('Botón alternativo no encontrado');
                         }
                     }
                 }, 2000);
             ");*/

            // Paso 6: Hacer clic en el botón de enviar (ícono de avión de papel)
            await webView21.CoreWebView2.ExecuteScriptAsync(@"
                setTimeout(function() {
                    var sendFileButton = document.querySelector('div[aria-label=""Enviar""]');
                    if (sendFileButton) {
                        sendFileButton.click();
                        console.log('Botón de enviar archivo (div[aria-label=""Enviar""]) clicado');
                    } else {
                        console.log('Botón de enviar archivo (div[aria-label=""Enviar""]) no encontrado. Buscando alternativa...');
                        var alternativeButton = document.querySelector('span[data-icon=""wds-ic-send-filled""]');
                        if (alternativeButton) {
                            alternativeButton.click();
                            console.log('Botón alternativo (span[data-icon=""wds-ic-send-filled""]) clicado');
                        } else {
                            console.log('Botón alternativo no encontrado');
                        }
                    }
                }, 2000);
            ");


        }

        private async Task SubirArchivo4()
        {
            try
            {
                // Validar la ruta del archivo
                string normalizedPath = Path.GetFullPath(rutaArchivo);
                if (!File.Exists(normalizedPath))
                {
                    MessageBox.Show($"El archivo no existe: {rutaArchivo}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!Path.GetExtension(normalizedPath).ToLower().Equals(".pdf"))
                {
                    MessageBox.Show("El archivo debe ser un PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Paso 1: Hacer clic en el botón de adjunto (clip)
                bool attachButtonFound = await WaitForElement("div[title=\"\"Adjuntar\"\"]");
                if (!attachButtonFound)
                {
                    MessageBox.Show("Botón de adjunto no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                await webView21.ExecuteScriptAsync(@"
            setTimeout(function() {
                var attachButton = document.querySelector('button[title=""Adjuntar""]') || 
                                 document.querySelector('button[aria-label=""Adjuntar""]');
                if (attachButton) {
                    // Simular un evento de clic más robusto
                    var clickEvent = new MouseEvent('click', {
                        view: window,
                        bubbles: true,
                        cancelable: true
                    });
                    attachButton.dispatchEvent(clickEvent);
                    console.log(JSON.stringify({ message: 'Botón adjunto clickeado', element: true }));
                } else {
                    console.log(JSON.stringify({ message: 'Botón adjunto no encontrado', element: false }));
                }
            }, 1000);
        ");

                // Paso 2: Esperar a que aparezca el menú de adjuntos
                bool menuFound = await WaitForElement("li[data-animate-dropdown-item=\"true\"]");
                if (!menuFound)
                {
                    MessageBox.Show("Menú de adjuntos no desplegado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Paso 3: Seleccionar la opción de "Documento"
                await webView21.ExecuteScriptAsync(@"
            setTimeout(function() {
                var docOption = Array.from(document.querySelectorAll('li'))
                    .find(li => li.textContent.includes('Documento') && li.querySelector('input[type=""file""][accept=""*""]'));
                if (docOption) {
                    var clickEvent = new MouseEvent('click', {
                        view: window,
                        bubbles: true,
                        cancelable: true
                    });
                    docOption.dispatchEvent(clickEvent);
                    console.log(JSON.stringify({ message: 'Opción de documento clickeada', element: true }));
                } else {
                    console.log(JSON.stringify({ message: 'Opción de documento no encontrada', element: false }));
                }
            }, 1000);
        ");

                // Paso 4: Esperar a que aparezca el diálogo de selección de archivo
                await Task.Delay(3000);

                // Paso 5: Enviar la ruta del archivo
                this.Activate();
                SendKeys.SendWait(normalizedPath);
                await Task.Delay(1000);
                SendKeys.SendWait("{ENTER}");

                // Paso 6: Esperar a que el archivo se cargue
                await Task.Delay(5000);

                // Paso 7: Hacer clic en el botón de enviar
                bool sendButtonFound = await WaitForElement("button[aria-label=\"Enviar\"]");
                if (!sendButtonFound)
                {
                    MessageBox.Show("Botón de enviar no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                await webView21.ExecuteScriptAsync(@"
            setTimeout(function() {
                var sendButton = document.querySelector('button[aria-label=""Enviar""]') || 
                               document.querySelector('span[data-icon=""send""]') ||
                               document.querySelector('div[aria-label=""Enviar""]');
                if (sendButton) {
                    var clickEvent = new MouseEvent('click', {
                        view: window,
                        bubbles: true,
                        cancelable: true
                    });
                    sendButton.dispatchEvent(clickEvent);
                    console.log(JSON.stringify({ message: 'Botón de enviar archivo clickeado', element: true }));
                } else {
                    console.log(JSON.stringify({ message: 'Botón de enviar archivo no encontrado', element: false }));
                }
            }, 2000);
        ");

                // Esperar a que el archivo se envíe
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> WaitForElement(string selector, int timeoutMs = 10000)
        {
            string script = $@"
        new Promise((resolve) => {{
            let start = Date.now();
            let check = () => {{
                if (document.querySelector('{selector}')) {{
                    resolve(true);
                }} else if (Date.now() - start > {timeoutMs}) {{
                    resolve(false);
                }} else {{
                    setTimeout(check, 100);
                }}
            }};
            check();
        }});
    ";
            string result = await webView21.ExecuteScriptAsync(script);
            return result == "true";
        }
        private async Task SubirArchivo3()
        {
            try
            {
                string normalizedPath = Path.GetFullPath(rutaArchivo);
                if (!File.Exists(normalizedPath))
                {
                    MessageBox.Show("El archivo no existe en la ruta especificada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Path.GetExtension(normalizedPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Solo se permiten archivos PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Paso 1: Hacer clic en el botón de adjuntar
                await webView21.ExecuteScriptAsync(@"
            setTimeout(function() {
                var attachButton = document.querySelector('div[title=""Adjuntar""]') || 
                                  document.querySelector('div[aria-label=""Adjuntar""]');
                if (attachButton) {
                    attachButton.click();
                    console.log('Botón Adjuntar clickeado');
                } else {
                    console.error('Botón Adjuntar no encontrado');
                }
            }, 1000);
        ");

                // Esperar a que aparezca el menú
                await Task.Delay(2000);

                // Paso 2: Seleccionar la opción de Documento
                await webView21.ExecuteScriptAsync(@"
            setTimeout(function() {
                // Buscar el input de archivo para documentos
                //var fileInputs = Array.from(document.querySelectorAll('input[type=""file""]'));
                var docInput = document.querySelector(""#app > div > span:nth-child(8) > div > ul > div > div > div:nth-child(1) > li > div"")
                
                if (docInput) {
                    docInput.click();
                    console.log('Input de documento encontrado y clickeado');
                } else {
                    console.error('Input para documentos no encontrado');
                }
            }, 1000);
        ");

                // Esperar a que aparezca el diálogo de selección de archivo
                await Task.Delay(3000);

                // Paso 3: Enviar la ruta del archivo
                this.Activate();
                await Task.Delay(500);
                SendKeys.SendWait(normalizedPath);
                await Task.Delay(1000);
                SendKeys.SendWait("{ENTER}");

                // Esperar a que se cargue el archivo
                await Task.Delay(5000);

                // Paso 4: Enviar el archivo
                await webView21.ExecuteScriptAsync(@"
            setTimeout(function() {
                var sendButton = document.querySelector('button[aria-label=""Enviar""]') || 
                                document.querySelector('span[data-icon=""send""]');
                if (sendButton) {
                    sendButton.click();
                    console.log('Archivo enviado');
                } else {
                    console.error('Botón Enviar no encontrado');
                }
            }, 2000);
        ");

                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}