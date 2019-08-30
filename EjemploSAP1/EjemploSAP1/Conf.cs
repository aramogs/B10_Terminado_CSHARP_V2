using EjemploSAP1.partes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EjemploSAP1
{
    class Conf
    {

        //public Dictionary<string, int> mapSAP = new Dictionary<string, int>();

        public Dictionary<string, Parte> mapSAP1 = new Dictionary<string, Parte>();


        public Conf() {
            
        }


        //server msg
        public string serverMsg = "localhost";
        public string userMsg = "nadmin";
        public string passMsg = "1234";
        public string queueName;


        //server cache
        public string serverCache = "localhost";
        public string userCache = "";
        public string passCache = "";
        public int dias = 0;


        //server MySQL
        public string serverMySQL = "localhost";
        public string userMySQL = "";
        public string passMySQL = "";
        public string db = "";
        public string table = "";



        //Station
        public int nostattion = 1;
        public string impresora = "impre1";
        public string noCalcel = "";
        public string empresa = "";
        public string prefix = "";
        public string platform = "";


        public int refresh = 1;


        //Bartender
        public bool bartender = false;
        public string linkWs = "";
        public string dbCustomers = "";
        public string impresoraBartender = "impre1";

        public List<Parte> aLista()
        {
            return mapSAP1.Values.ToList<Parte>();
        }
    }


    
}
