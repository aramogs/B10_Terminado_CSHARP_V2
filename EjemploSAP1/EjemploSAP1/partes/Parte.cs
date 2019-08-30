using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EjemploSAP1.partes
{
    class Parte
    {


        public string estacion ;

        public string impresora ;

        public string impresoraBartender; 

        public string nSAP { get; set; } 

        public int pckd { get; set; } 

        public string nParte { get; set; } 

        public int nProveedor ; 

        public string fecha ; 

        public int nSerieserie ;



        public string nVali = "";

        public string empresa = "5210";

        public string cust  { get; set; }
        public string plat { get; set; }


        public bool bartender = false;




        public Parte()
        {
            nProveedor = 19906610;
        }


        public Parte(int pk, string nP) {

            nProveedor = 19906610;
            nParte = nP;
            pckd = pk;
        }


        public Parte(string sap, int pk, string nP)
        {
            nSAP = sap;
            nProveedor = 19906610;
            nParte = nP;
            pckd = pk;
        }

    }
}
