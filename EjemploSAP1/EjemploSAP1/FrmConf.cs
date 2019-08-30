using EjemploSAP1.partes;
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
    public partial class FrmConf : Form
    {

       

        public FrmConf()
        {
            InitializeComponent();


            txtPassMsg.PasswordChar = '*';
            txtPassCache.PasswordChar = '*';
            txtPassMySQL.PasswordChar = '*';
            
            string json = Form1.ReadAllText("station.conf");

            
            dataGridView1.DataSource = Program.myConf.mapSAP1.Values.ToList<Parte>();
            
            txtNoStation.Text = Program.myConf.nostattion + "";
            txtPrinterStation.Text = Program.myConf.impresora;
            txtNoCancel.Text = Program.myConf.noCalcel;

            txtServerMsg.Text = Program.myConf.serverMsg;
            txtUserMsg.Text = Program.myConf.userMsg;
            txtPassMsg.Text = Program.myConf.passMsg;

            txtDias.Text = "" + Program.myConf.dias;

            txtRefresh.Text = "" + Program.myConf.refresh;

            txtEmpresa.Text = Program.myConf.empresa;

            txtServerCache.Text = Program.myConf.serverCache;
            txtUserCache.Text = Program.myConf.userCache;
            txtPassCache.Text = Program.myConf.passCache;

            
            txtServerMySQL.Text = Program.myConf.serverMySQL;
            txtUserMySQL.Text = Program.myConf.userMySQL;
            txtPassMySQL.Text = Program.myConf.passMySQL;
            txtDb.Text = Program.myConf.db;
            txtTableMySQL.Text = Program.myConf.table;


            radioButton1.Checked = !Program.myConf.bartender;
            radioButton2.Checked = Program.myConf.bartender;

            txtLinkWs.Text = Program.myConf.linkWs;
            txtDbCustomes.Text = Program.myConf.dbCustomers;

            txtBarTender.Text = Program.myConf.impresoraBartender;


            txtPlatform.Text = Program.myConf.platform;

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FrmConf_Load(object sender, EventArgs e)
        {
            
        }


        public void saveFileConf(string json, string path) {

            Form1.WriteAllText(json, path);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Program.myConf != null)
            {
                Program.myConf.serverCache = txtServerCache.Text;
                Program.myConf.userCache = txtUserCache.Text;
                Program.myConf.passCache = txtPassCache.Text;

                Program.myConf.dias = Int16.Parse(txtDias.Text);

                string json = JsonConvert.SerializeObject(Program.myConf);

                Console.WriteLine("myConf ---> " + json);
                saveFileConf(json, "station.conf");
            }
            else
                Program.myConf = new Conf();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Program.myConf != null)
            {
                Program.myConf.serverMsg = txtServerMsg.Text;
                Program.myConf.userMsg = txtUserMsg.Text;
                Program.myConf.passMsg = txtPassMsg.Text;

                Program.myConf.queueName = txtQueue.Text;

                string json = JsonConvert.SerializeObject(Program.myConf);
                saveFileConf(json, "station.conf");
            }
            else
                Program.myConf = new Conf();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Program.myConf != null)
            {
                Program.myConf.nostattion = Int16.Parse(txtNoStation.Text);
                Program.myConf.noCalcel = txtNoCancel.Text;

                Program.myConf.refresh = Int16.Parse(txtRefresh.Text);

                Program.myConf.empresa = txtEmpresa.Text;

                Program.myConf.platform = txtPlatform.Text;
                string json = JsonConvert.SerializeObject(Program.myConf);
                saveFileConf(json, "station.conf");
                
            }
            else
                Program.myConf = new Conf();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            int sp = Int16.Parse(txtPack.Text);
            string nParte = textBox1.Text;

            Parte p = new Parte(sp, nParte);

            Program.myConf.mapSAP1.Add("" + sp, p);

            dataGridView1.DataSource = null;


            dataGridView1.DataSource = Program.myConf.mapSAP1.Values.ToList<Parte>();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Program.myConf.mapSAP1.Remove(txtClient.Text);
            dataGridView1.DataSource = null;

            dataGridView1.DataSource = Program.myConf.mapSAP1.Values.ToList<Parte>();
            
        }

        private void btnSaveMySQL_Click(object sender, EventArgs e)
        {
            if (Program.myConf != null)
            {
                Program.myConf.serverMySQL = txtServerMySQL.Text;
                Program.myConf.userMySQL = txtUserMySQL.Text;
                Program.myConf.passMySQL = txtPassMySQL.Text;
                Program.myConf.db = txtDb.Text;
                Program.myConf.table = txtTableMySQL.Text;
                
                string json = JsonConvert.SerializeObject(Program.myConf);

                Console.WriteLine("myConf ---> " + json);
                saveFileConf(json, "station.conf");
            }
            else
                Program.myConf = new Conf();
        }

        private void button6_Click(object sender, EventArgs e)
        {


            if (Program.myConf != null)
            {
                Program.myConf.impresora = txtPrinterStation.Text;


                Program.myConf.linkWs = txtLinkWs.Text;
                Program.myConf.dbCustomers = txtDbCustomes.Text;

                
                Program.myConf.bartender = !radioButton1.Checked;

                Program.myConf.impresoraBartender = txtBarTender.Text;

                string json = JsonConvert.SerializeObject(Program.myConf);
                saveFileConf(json, "station.conf");
                
            }
            else
                Program.myConf = new Conf();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            
        }
    }
}
