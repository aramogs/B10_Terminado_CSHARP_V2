using EjemploSAP1.partes;
using EjemploSAP1.rabbitMq;
using EjemploSAP1.util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EjemploSAP1
{
    public partial class ValidarEtiqueta : Form
    {


        public string HOST_NAME; // = "192.168.0.7";
        
        private Producer producer = new Producer();

        
        public ValidarEtiqueta()
        {

            InitializeComponent();
            txtPart.Text = Program.validacion;
            val = Program.validacion;
        }


        public string val = "";

        public ValidarEtiqueta(string v)
        {

            InitializeComponent();
            textBox3.Text = v;     
        }


        public Form1 f1;

        public ValidarEtiqueta(Form1 f)
        {
            InitializeComponent();

            txtPart.Text = Program.validacion;
            f1 = f;
        }



        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Enter)
            {
                string s1 =  textBox3.Text;
                string s2 = "S" +  txtPart.Text;

                if ( ( s2.Substring(0, 1) == "S" || s2.Substring(0, 1) == "s") && s1.ToLower() == s2.ToLower() )
                {
        
                    Parte p = new Parte();
                    
                    p.nSAP = Program.parte.nSAP;
                    p.impresora = Program.myConf.impresora;
                    p.estacion = "" + Program.myConf.nostattion;
                    p.pckd = Program.parte.pckd;
                    p.fecha = "";
                    p.nSerieserie = 0;

                    p.nVali = txtPart.Text;


                    string json = JsonConvert.SerializeObject(p);


                    producer.SendMessage(System.Text.Encoding.UTF8.GetBytes(json));
                    this.ReturnValue1 = "Something";
                    this.DialogResult = DialogResult.OK;
                    
                }
                else
                {

                    label3.Text = "Validación, el número de serie no corresponde, intende de nuevo";
                }
            }


            if (e.KeyCode == Keys.Enter || Program.myConf.noCalcel == textBox3.Text)
            {

                this.ReturnValue1 = "Something";
                this.DialogResult = DialogResult.Cancel;
                this.Close();

            }

        }

        private void ValidarEtiqueta_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        public string ReturnValue1 { get; set; }


        private void ValidarEtiqueta_FormClosed(object sender, FormClosedEventArgs e)
        {
                f1.newTransaction();
        }

        private void ValidarEtiqueta_Load(object sender, EventArgs e)
        {
            WinAPI.SiempreEncima(this.Handle.ToInt32());
        }

        private void txtPart_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
