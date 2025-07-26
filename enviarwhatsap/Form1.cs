using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace enviarwhatsap
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string numero = "+529992963226"; // Cambia por el número deseado
            string mensaje = "¡Hola! Este es un mensaje de prueba." + DateTime.Now.ToString();
            string rutaArchivo = @"C:\Users\Manuel Gomez\source\repos\enviarwhatsap\enviarwhatsap\bin\Debug\hola.pdf"; // O null si solo es texto

            var ventana = new ventana_envio(numero, mensaje, rutaArchivo);


            ventana.ShowDialog(); // Espera a que la ventana se cierre sola
        }

        private void button2_Click(object sender, EventArgs e)
        {

            whatsapp ventana = new whatsapp();


            ventana.ShowDialog(); // Espera a que la ventana se cierre sola

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ventana = new iniciowhatsapp();


            ventana.ShowDialog(); // Espera a que la ventana se cierre sola
        }
    }
}
