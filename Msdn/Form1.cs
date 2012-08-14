using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Msdn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1.Click += (Object c, EventArgs e) =>
            {
                try
                {
                    this.Enabled = false;
                    var Url = new Uri(textBox1.Text);
                    var Extraer = new Extractor(Url);
                    MessageBox.Show("Terminó");
                    this.Enabled = true;
                }
                catch(Exception)
                {
                    this.Enabled = true;
                    MessageBox.Show("Ocurrió un error, intente de nuevo");
                }
            }; 

        }
    }
}