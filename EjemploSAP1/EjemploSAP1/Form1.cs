using CSRedis;
using EjemploSAP1.arribo;
using EjemploSAP1.mysql;
using EjemploSAP1.partes;
using EjemploSAP1.rabbitMq;
using EjemploSAP1.util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EjemploSAP1
{
    public partial class Form1 : Form
    {
        
        Thread HiloNT;
        
        private ConsumerFanOut consumerFanOut;
        
        List<string> numerosSAP = new List<string>();

        public static List<string> partes = new List<string>();

        Dictionary<string, string> entradas = new Dictionary<string, string>();

        Dictionary<string, string> listaTempRedis = new Dictionary<string, string>();
        string strLista;

        Dictionary<string, Parte> mapSAP1 = new Dictionary<string, Parte>();


        IRedisClient csredis;

        private void CorrerProceso()
        {
            while (true) {
                while (bande) { 
                    Thread.Sleep(Program.myConf.refresh * 60  * 1000);
                    Parte p = new Parte();

                    p.nSAP = "";
                    

                    json = JsonConvert.SerializeObject(p);

                    try {

                        producer.SendMessageTTL(System.Text.Encoding.UTF8.GetBytes(json));

                        
                    }
                    catch (Exception e) {
                        Console.WriteLine("Error de conn en el RabbitMq");
                    }
                    
                }
            }
        }

        //-------------------------------------------------------

        private delegate void showMessageDelegate();

        public void setCambioTextBox1() {

            textBox1.Text = "";
            textBox1.BackColor = Color.White;
        }

        private void CambioColorLetra()
        {
            
            Thread.Sleep(15 * 1000);

            showMessageDelegate s = new showMessageDelegate(setCambioTextBox1);

            this.Invoke(s);

            textBox1.Text = "";
            textBox1.BackColor = Color.White; 
        }

        //-------------------------------------------------------
        
        public string HOST_NAME;
        public string QUEUE_NAME = "workQueues2";
        
        private Producer producer;
        //-------------------------------------


        DBConnect dBConnect;
        List<Parte> ll;

        public Form1()
        {

            string json = ReadAllText("station.conf");

            Program.myConf = JsonConvert.DeserializeObject<Conf>(json);
            
            if (Program.myConf != null)
            {
        
                csredis = new RedisClient(Program.myConf.serverMsg);

                if (csredis == null)
                    toolStripStatusLabel3.Text = "Cache No Connected";
            }
            

            string partesTemp = ReadAllText("lista.temp");
            

            if (partesTemp != "{}\r\n") { 
                partes = JsonConvert.DeserializeObject<List<string>>(partesTemp);
            }
            else
                WriteAllText("", "lista.temp");


            if (partes != null)
            {
                foreach (string p in partes)
                {
                    if (csredis != null)
                        csredis.Del(p);
                }

                WriteAllText("", "lista.temp");
                partes.Clear();
            }
            else partes = new List<string>();
            

            if (Program.myConf != null)
            {
                consumerFanOut = new ConsumerFanOut(Program.myConf.serverMsg, "stations", "s" + Program.myConf.nostattion);

                consumerFanOut.onMessageReceived += handleMessage;
                consumerFanOut.StartConsuming();
    
            }

            InitializeComponent();

            DBConnect.server = Program.myConf.serverMySQL;
            DBConnect.uid = Program.myConf.userMySQL;
            DBConnect.password = Program.myConf.passMySQL;
            DBConnect.database = Program.myConf.db;


            dBConnect = DBConnect.getInstance();

            String sSQL = "SELECT * FROM " + Program.myConf.db + "." + Program.myConf.table + ";";

            ll = dBConnect.SelectParte("SELECT * FROM " + Program.myConf.db + "." + Program.myConf.table + ";");

            MaximizeBox = false;
            MinimizeBox = false;
            

            try
            {
                producer = new Producer();
            }
            catch (Exception e)
            {
                toolStripStatusLabel1.Text = "Msg No Connected";
                producer = null;
            }


            Program.myConf.mapSAP1.Clear();
            
            foreach (Parte p in ll)
                Program.myConf.mapSAP1.Add(p.nSAP, p);
            
            mapSAP1 = Program.myConf.mapSAP1;

            this.myDelegate = new AddDataDelegate(setText);

            
            ThreadStart delegado = new ThreadStart(CorrerProceso);
            Thread hiloSession = new Thread(delegado);
            
        }

        public void handleMessage(byte[] message)
        {


            try
            {
        
                string msg = System.Text.Encoding.UTF8.GetString(message);

                Accion accion = JsonConvert.DeserializeObject<Accion>(msg);
                
                if (accion.titulo == "Validacion") {


                    AccVslidacion av = JsonConvert.DeserializeObject<AccVslidacion>(accion.dato);
                    
                    
                    Program.validacion = "";
        
        
                    Program.validacion = av.noValidacion;


                    Program.vali = new ValidarEtiqueta(this);

                    
                    var result = Program.vali.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                    
                        HiloNT = new Thread(new ThreadStart(newTransaction));
                        HiloNT.Start();
                    
                    }


                    if (result == DialogResult.Cancel)
                    {
                        cancelarTrabajo();
                        
                    }
                    
                }


            }
            catch (Exception e)
            {

                Console.WriteLine("Error en el objeto");
            }
        }


        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        
        private static string InternalReadAllText(string path, Encoding encoding)
        {
            string result;
            using (StreamReader streamReader = new StreamReader(path, encoding))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

        public static string ReadAllText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                return ""; 
            }
            return InternalReadAllText(path, Encoding.UTF8);
        }



        private static void InternalWriteAllText(string text, string path)
        {

            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.WriteLine(text);
            }
            
        }

        public static void WriteAllText(string text, string path)
        {
            if (text == null || path == null)
            {
                throw new ArgumentNullException("path or text");
            }

            InternalWriteAllText(text, path);
        }

        
        private void button1_Click(object sender, EventArgs e)
        {

            Process scriptProc = new Process();
            scriptProc.StartInfo.FileName = @"cscript";
            string qParams = textBox3.Text + " " + pzas;
            scriptProc.StartInfo.Arguments = " TEST2.vbs " + qParams;
            scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            
            scriptProc.WaitForExit();
            scriptProc.Close();
        }
        
        private ManualResetEvent m_processExited = new ManualResetEvent(false);
        private List<string> m_errorMessages = new List<string>();
        private List<string> m_regularMessages = new List<string>();
        private Process m_process = new Process();

        public void Run(string[] args)
        {
        
            StringBuilder sb = new StringBuilder("");

            sb.Append(" TEST.vbs ");
            m_process.StartInfo.FileName = @"cscript";
            m_process.StartInfo.Arguments = sb.ToString();
            m_process.StartInfo.UseShellExecute = false;

            m_process.Exited += this.ProcessExited;

            m_process.StartInfo.RedirectStandardError = true;
            m_process.StartInfo.RedirectStandardOutput = true;

            m_process.ErrorDataReceived += this.ErrorDataHandler;
            m_process.OutputDataReceived += this.OutputDataHandler;

            m_process.BeginErrorReadLine();
            m_process.BeginOutputReadLine();

            m_process.Start();

            m_processExited.WaitOne();
        }


        public void button2_Click(object sender, EventArgs e)
        {
            newTransaction();
        }


        private void ErrorDataHandler(object sender, DataReceivedEventArgs args)
        {
            string message = args.Data;

            if (message.StartsWith("Error"))
            {
                m_errorMessages.Add(message);
            }
        }

        private void OutputDataHandler(object sender, DataReceivedEventArgs args)
        {
            string message = args.Data;

            m_regularMessages.Add(message);
        }

        private void ProcessExited(object sender, EventArgs args)
        {
            m_processExited.Set();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = @"cscript";
            proc.StartInfo.Arguments = " TEST.vbs ";
            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var errors = new StringBuilder();
            var output = new StringBuilder();
            var hadErrors = false;

            proc.EnableRaisingEvents = true;

            proc.OutputDataReceived += (s, d) => {
                output.Append(d.Data);
            };

            proc.ErrorDataReceived += (s, d) => {
                if (!hadErrors)
                {
                    hadErrors = !String.IsNullOrEmpty(d.Data);
                }
                errors.Append(d.Data);
            };

            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();

            proc.WaitForExit();
            string stdout = output.ToString();
            string stderr = errors.ToString();

            if (proc.ExitCode != 0 || hadErrors)
            {
                MessageBox.Show("My Error: " + stderr);
            }
        }



        public delegate void AddDataDelegate(String myString);
        public AddDataDelegate myDelegate;

        int i;
        public void setText(string str)
        {
            i++;
            label1.Text = "" + i;
        }


        public void getErrorSAPConnection() {
            
            Process proc = new Process();
            proc.StartInfo.FileName = @"cscript";
            proc.StartInfo.Arguments = " TEST2.vbs ";
            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var errors = new StringBuilder();
            var output = new StringBuilder();
            var hadErrors = false;

            proc.EnableRaisingEvents = true;

            proc.OutputDataReceived += (s, d) => {
                output.Append(d.Data);
            };

            proc.ErrorDataReceived += (s, d) => {
                if (!hadErrors)
                {
                    hadErrors = !String.IsNullOrEmpty(d.Data);
                }
                errors.Append(d.Data);
            };

            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
        
            proc.WaitForExit();
        
            string stdout = output.ToString();
            string stderr = errors.ToString();

            if (proc.ExitCode != 0 || hadErrors)
            {
                MessageBox.Show("My Error: " + stderr);
            }
            
        }


        private void button4_Click(object sender, EventArgs e)
        {
            ThreadStart delegado = new ThreadStart(getErrorSAPConnection);
            Thread hilo = new Thread(delegado);
            hilo.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!partes.Contains(textBox4.Text)) {
                partes.Add(textBox4.Text);
            }
            else {
                Console.WriteLine("El numero ya es parte de la lista");
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {


        }

        int pzas = 0;
        int countPzas = 0;
        Parte parte;
        



        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter && textBox3.Text.Count() == 13)
            {

                
                if ( textBox3.Text != "" && mapSAP1.ContainsKey(textBox3.Text) ) 
                {
                    parte = mapSAP1[textBox3.Text];
                    

                    string sqlll2 = "SELECT * FROM " + Program.myConf.db + "." + Program.myConf.table +
                        " where no_sap = '" + parte.nSAP + "';";


                    List<Parte> ll2 = DBConnect.getInstance().SelectParte(sqlll2);
                    
                    parte.cust = ll2.ElementAt(0).cust + "";
                    parte.plat = ll2.ElementAt(0).plat + "";
                    
                    Program.myConf.platform = ll2.ElementAt(0).plat + "";
                    
                    csredis.Set(textBox3.Text, textBox3.Text);
                    pzas = parte.pckd;
                    txtPart.Text = parte.nParte;

                    label4.Text = "" + pzas;
                    textBox3.Enabled = false;
                    label2.Text = "To Be Pckd: " + pzas;

                    textBox4.Enabled = true;
                    textBox4.Focus();
                    
                }
                 
            }

            if (e.KeyCode == Keys.Enter && textBox3.Text.Count() > 13)
            {
                textBox3.Text = "";
            }
        }




        public void cancelarTrabajo() {

            
            textBox3.Text = "";
            
            foreach (string p in partes){
                if (csredis != null)
                    csredis.Del(p);
            }
            

            partes.Clear();

            strLista = JsonConvert.SerializeObject(partes);
            WriteAllText(strLista, "lista.temp");
            
            textBox3.Enabled = true;
            countPzas = 0;
            pzas = 0;

            textBox4.Text = "";
            textBox4.Enabled = false;

            label2.Text = "To Be Pckd: " + pzas;

            label3.Text = "0";
            label4.Text = "0";

            textBox1.Text = "MSG:";

            txtPart.Text = "";

            textBox3.Text = "";
            textBox3.Focus();

        }


        //-------------------------------------

        public void envioAnticipado2()
        {
            
                Parte p = new Parte();
            
                p.bartender = Program.myConf.bartender;

                Program.parte = new Parte();
                Program.parte.nSAP = p.nSAP = textBox3.Text;

                p.impresora = Program.myConf.impresora;
                p.impresoraBartender = Program.myConf.impresoraBartender;

                Program.parte.estacion = p.estacion = "" + Program.myConf.nostattion;
            
                p.pckd = countPzas; 

                p.fecha = "";
                Program.parte.nSerieserie = p.nSerieserie = 4;
                Program.parte.empresa = p.empresa = Program.myConf.empresa;

                Program.parte.cust = p.cust = parte.cust;
                Program.parte.plat = p.plat = parte.plat;
                Program.parte.nParte = p.nParte = parte.nParte;

                textBox4.Enabled = false;

                json = JsonConvert.SerializeObject(p);
            
                producer.SendMessage(System.Text.Encoding.UTF8.GetBytes(json));
            
        }


        public void envioAnticipado() {



            if (  textBox4.Text.Count() >= 28 &&   
                  textBox4.Text.Substring(0, 13).Contains(txtPart.Text.Replace("_", ""))
                )
            {
            
                if (csredis.Get(textBox4.Text) == null
                        &&
                        countPzas < pzas && !textBox3.Enabled
                   )
                {
                    DateTime dt = DateTime.Now; 

                    string s = dt.ToString("yyyyMMddHHmmssff");


                    int d = 1;

                    if (Program.myConf.dias > 1)
                        d = Program.myConf.dias;
                    
                    if (csredis != null)
                        csredis.Set(textBox4.Text, textBox3.Text,
                        d *
                     
                        24 * 
                        60 * 
                        1   
                        );

                    partes.Add(textBox4.Text);

                    strLista = JsonConvert.SerializeObject(partes);
                    WriteAllText(strLista, "lista.temp");
                    countPzas++;

                    if ( countPzas == pzas )
                    {
                        Parte p = new Parte();
                        
                        p.bartender = Program.myConf.bartender;

                        Program.parte = new Parte();
                        Program.parte.nSAP = p.nSAP = textBox3.Text;

                        p.impresora = Program.myConf.impresora;
                        p.impresoraBartender = Program.myConf.impresoraBartender;

                        Program.parte.estacion = p.estacion = "" + Program.myConf.nostattion;
                        p.pckd = pzas;
                        p.fecha = "";
                        Program.parte.nSerieserie = p.nSerieserie = 4;
                        Program.parte.empresa = p.empresa = Program.myConf.empresa;

                        Program.parte.cust = p.cust = parte.cust;
                        Program.parte.plat = p.plat = parte.plat;
                        Program.parte.nParte = p.nParte = parte.nParte;

                        textBox4.Enabled = false;

                        json = JsonConvert.SerializeObject(p);

                        producer.SendMessage(System.Text.Encoding.UTF8.GetBytes(json));
                        
                    }

                    label3.Text = "" + countPzas;
                    textBox1.BackColor = Color.LightGreen;
                    textBox1.ForeColor = Color.Black;
                    textBox1.Text = "MSG: Transaction, Ok";

                    ThreadStart dele = new ThreadStart(CambioColorLetra);
                    Thread h = new Thread(dele);
                    h.Start();

                }
                
                textBox4.Text = "";

            } 
            else
            {
                if (
                    textBox4.Text.Length > 14 &&
            
                    !textBox4.Text.Substring(0, 14).Contains(txtPart.Text.Replace("_", ""))

                )


                {
                    textBox1.BackColor = Color.Red;
                    textBox1.Text = "MSG: No Corresponde el numero de SAP";

                }
                else
                {
                    textBox1.BackColor = Color.White;
                    textBox1.Text = "";
                }
            }

            if ( textBox4.Text.Count() > 27 )
            {

                textBox4.Text = "";
            }

        }
        //-----------------
        

        private void textBox4_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter && textBox4.Text == Program.myConf.noCalcel)
            {
            
                if (csredis != null)
                    csredis.Del(textBox3.Text);

                textBox3.Text = "";
                
                foreach (string p in partes){
                    if (csredis != null)
                        csredis.Del(p);
                }
                

                partes.Clear();

                strLista = JsonConvert.SerializeObject(partes);
                Console.WriteLine();
                WriteAllText("", "lista.temp");


                textBox3.Enabled = true;
                countPzas = 0;
                pzas = 0;

                textBox4.Text = "";

                textBox4.Enabled = false;

                label2.Text = "To Be Pckd: " + pzas;

                label3.Text = "0";
                label4.Text = "0";

                textBox1.Text = "MSG:";

                txtPart.Text = "";

                textBox3.Text = "";
                textBox3.Focus();
            }

            if (       e.KeyCode == Keys.Enter
                    && textBox4.Text.Count() >= 28 &&
                    
                    textBox4.Text.Substring(0, 13).Contains(txtPart.Text.Replace("_", ""))

                )
            {
                
                if ( csredis.Get(textBox4.Text) == null
                        &&
                        countPzas < pzas && !textBox3.Enabled
                   )
                {
                    DateTime dt = DateTime.Now;

                    string s = dt.ToString("yyyyMMddHHmmssff");


                    int d = 1;

                    if (Program.myConf.dias > 1)
                        d = Program.myConf.dias;
                        if (csredis != null)
                        csredis.Set(textBox4.Text, textBox3.Text,
                        d *
                        
                        24 * 
                        60 * 
                        1  
                        );

                        
        partes.Add(textBox4.Text);

                    strLista = JsonConvert.SerializeObject(partes);
                    Console.WriteLine();
                    WriteAllText(strLista, "lista.temp");

                    countPzas++;
                    
                    if (
                        countPzas == pzas
                    
                        )
                    {

                        Parte p = new Parte();

                        p.bartender = Program.myConf.bartender;

                        Program.parte = new Parte();
                        Program.parte.nSAP = p.nSAP = textBox3.Text;

                        p.impresora = Program.myConf.impresora;
                        p.impresoraBartender = Program.myConf.impresoraBartender;

                        Program.parte.estacion =  p.estacion = "" + Program.myConf.nostattion;
                        p.pckd = pzas;
                        p.fecha = "";
                        Program.parte.nSerieserie = p.nSerieserie = 4;
                        Program.parte.empresa = p.empresa = Program.myConf.empresa;

                        Program.parte.cust = p.cust = parte.cust;
                        Program.parte.plat = p.plat = parte.plat;
                        Program.parte.nParte = p.nParte = parte.nParte;

                        textBox4.Enabled = false;

                        json = JsonConvert.SerializeObject(p);

                        producer.SendMessage(System.Text.Encoding.UTF8.GetBytes(json));


                    }

                    label3.Text = "" + countPzas;
                    textBox1.BackColor = Color.LightGreen;
                    textBox1.ForeColor = Color.Black;
                    textBox1.Text = "MSG: Transaction, Ok";

                    ThreadStart dele = new ThreadStart(CambioColorLetra);
                    Thread h = new Thread(dele);
                    h.Start();
                    
                }
                else
                {
                    textBox1.BackColor = Color.Red;
                    textBox1.ForeColor = Color.Silver;
                    textBox1.Text = "MSG: Transaction, el numero ya es parte de la lista o no corresponde el numero de SAP";
                }

                textBox4.Text = "";

            }
            else
            {
                if (
                    textBox4.Text.Length > 14 && 
                    !textBox4.Text.Substring(0, 14).Contains(txtPart.Text.Replace("_", ""))
                
                )
                {
                    textBox1.BackColor = Color.Red;
                    textBox1.Text = "MSG: No Corresponde el numero de SAP";
                }
                else {
                    textBox1.BackColor = Color.White;
                    textBox1.Text = "";
                }
            }

            if(e.KeyCode == Keys.Enter && textBox4.Text.Count() > 27) {

                textBox4.Text = "";
            }
            
        }



        public void newTransaction() {
            if (partes.Count > 0) {
                string turno = "" + Turno.getTurno();
                foreach (string strP in partes) {

                    try
                    {

                        String sqlx = @"INSERT INTO b10.etiquetas (`np`, `turno`, `linea`, `dmc`, `no_serie`, `plataforma`, `fecha`)VALUES ('" +
                            textBox3.Text + "', '" + turno + "', '" + Program.myConf.nostattion + "', '.', '" + strP + "', '" + Program.myConf.platform + "', now() );";

                        DBConnect.getInstance().Insert(sqlx);
                    }
                    catch (Exception ex) {

                        Console.WriteLine(ex.Message);
                    }

                }

                

            }

            partes.Clear();

            WriteAllText("", "lista.temp");

            textBox3.Enabled = true;
            countPzas = 0;
            pzas = 0;
            textBox4.Enabled = false;

            label2.Text = "To Be Pckd: " + pzas;

            label3.Text = "0";
            label4.Text = "0";
            textBox1.Text = "MSG:";
            txtPart.Text = "";
            textBox3.Text = "";
            textBox3.Focus();
        }


        private void button7_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            string s = dt.ToString("yyyyMMddHHmmssff");
            Console.WriteLine(s);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            
        }

        string json;
        private void button9_Click(object sender, EventArgs e)
        {
            Parte p = new Parte();

            p.nSAP = "P7000010143AP";
            p.nProveedor = 1;
            p.pckd = pzas;
            p.fecha = "";
            p.nSerieserie = 4;


            json = JsonConvert.SerializeObject(p);
            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Parte m = JsonConvert.DeserializeObject<Parte>(json);
            
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void label9_DoubleClick(object sender, EventArgs e)
        {
            }

        private void label9_Click(object sender, EventArgs e)
        {
            
        }

        private void goToConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmConf fc = new FrmConf();
            fc.Show();
        }



        private void conectarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try {
                toolStripStatusLabel1.Text = "Conectando . . . ";
                producer = new Producer();
                toolStripStatusLabel1.Text = "";
            }
            catch (Exception ex) {
                toolStripStatusLabel1.Text = "No Connected";
            }
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = "Cache";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(producer != null)
                producer.Dispose();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        bool bande = true;
        private void button11_Click_1(object sender, EventArgs e)
        {
            bande = !bande;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Msg: No corresponde el numero de SAP";
            textBox1.BackColor = Color.Red;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            textBox3.Focus();
        }








        private void button12_Click_1(object sender, EventArgs e)
        {
             

            AccVslidacion v = new AccVslidacion();
            v.noValidacion = "34";
            string cadeV = "";
            cadeV = JsonConvert.SerializeObject(v);
            Console.WriteLine(cadeV);


            Accion a = new Accion();
            string cade = "";
            a.titulo = "Validacion";
            a.dato = cadeV;
            cade = JsonConvert.SerializeObject(a);
            Console.WriteLine(cade);


            Accion aa  = JsonConvert.DeserializeObject<Accion>(cade);


            AccVslidacion av = JsonConvert.DeserializeObject<AccVslidacion>(cadeV);

            Console.WriteLine();

        }


        //------------- MySQL ----------------------

        public OdbcConnection cn;

        public void loadDatMysql()
        {

            cn = new OdbcConnection();

            cn.ConnectionString =
              "Dsn=wamp;" +
              "Uid=root;" +
              "Pwd=;";


            cn.Open();

        }



        public void runQueryMySQL(string sql)
        {


            OdbcCommand command = cn.CreateCommand();
            command.CommandText = sql;

            OdbcDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                
            }


        }

        private void txtPart_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void button13_Click(object sender, EventArgs e)
        {
            envioAnticipado2();
        }
    }
}
