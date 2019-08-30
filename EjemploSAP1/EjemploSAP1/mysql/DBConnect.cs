using EjemploSAP1.partes;
using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


/* refWeb: https://www.codeproject.com/Articles/43438/Connect-C-to-MySQL */
namespace EjemploSAP1.mysql
{
    class DBConnect
    {
        private MySqlConnection connection;
        public static string server = "localhost";
        public static string database = "";
        public static string uid = "";
        public static string password = "";
        public static string port = "3306";

        
        public readonly string config =
                string.Format("Server = {0}; Port = {1}; Database = {2}; Uid = {3}; Pwd = {4}; pooling = true; Allow Zero Datetime = False; Min Pool Size = 0; Max Pool Size = 200; ",
                server, port, database, uid, password);

        //Constructor
        private DBConnect()
        {
            Initialize();
        }


        private static DBConnect dBConnect;


        public static DBConnect getInstance() {

            if (dBConnect == null)
                dBConnect = new DBConnect();

            return dBConnect;
        }



        private void Initialize()
        {
            
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + "; SslMode=none";

            connection = new MySqlConnection(connectionString);
            
        }

        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }


        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void Insert(string sql)
        {

        
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }
        }

        public void Update(string sql)
        {
        
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = connection;

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }
        }

        public void Delete(string sql)
        {
        
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        
        public List<Parte> SelectParte(string sql)
        {
            List<Parte> list = new List<Parte>();
           
            bool resu = this.OpenConnection();
            if (resu == true)
            {
            
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    Parte parte = new Parte();
                    parte.nSAP = dataReader["no_sap"] + "";
                    parte.pckd = Int16.Parse(dataReader["pckd"] + "");
                    parte.nParte = dataReader["no_part"] + "";

                    parte.cust = dataReader["cust"] + "";
                    parte.plat = dataReader["plat"] + "";

                    list.Add(parte);
                    
                }

                dataReader.Close();

                this.CloseConnection();

                return list;
            }
            else
            {
                return list;
            }
        }

        public List<string>[] Select(string sql)
        {
            List<string>[] list = new List<string>[4];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();
            list[3] = new List<string>();

            list[4] = new List<string>();
            list[5] = new List<string>();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    list[0].Add(dataReader["id"] + "");
                    list[1].Add(dataReader["no_sap"] + "");
                    list[2].Add(dataReader["pckd"] + "");
                    list[3].Add(dataReader["no_part"] + "");

                    list[4].Add(dataReader["cust"] + "");
                    list[5].Add(dataReader["plat"] + "");

                    Console.WriteLine();

                }

                dataReader.Close();

                this.CloseConnection();

                return list;
            }
            else
            {
                return list;
            }
        }

        public int Count(string sql)
        {
            int Count = -1;

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                Count = int.Parse(cmd.ExecuteScalar() + "");

                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                string path;
                path = "C:\\MySqlBackup" + year + "-" + month + "-" + day +
            "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error , unable to backup!");
            }
        }

        public void Restore()
        {
            try
            {
                string path;
                path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;


                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error , unable to Restore!");
            }
        }
    }
}
