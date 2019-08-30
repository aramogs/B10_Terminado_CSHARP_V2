using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EjemploSAP1.util
{
    class Turno
    {
        
        public static string getTurno()
        {

            var date = DateTime.Now;            

            if (date.Hour >= 7 && date.Hour < 15)
                return "T1";

            if (date.Hour >= 15 && date.Hour < 23)
                return "T2";

            return "T3";
        }

    }
}
