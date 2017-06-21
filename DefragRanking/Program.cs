using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DefragRanking
{
    class Program
    {
        static void Main(string[] args)
        {

            //Verificar validaciones antes de lanzar los servidores
            QuakeConsole q3 = new QuakeConsole();   
            List<IntPtr> servers = new List<IntPtr>();
            servers = q3.getQuakeServers("Q3 WinConsole",null);     //className = Q3 WinConsole ,  windowsName = Quake 3 Console

            foreach (IntPtr server in servers)
            {
                IntPtr tmp = server;
                QuakeConsole defragRanking = new QuakeConsole(tmp);

                Thread threadConsole = new Thread(defragRanking.initialize);
                threadConsole.Start();
                while (!threadConsole.IsAlive) ;
                Console.WriteLine("=========================================");
                Console.WriteLine("Defrag Ranking se inicio correctamente...");
            }
           


            
            /*
            QuakeConsole Q3Console = new QuakeConsole("Q3 WinConsole",null); 
            Thread threadConsole = new Thread(Q3Console.initialize);
            threadConsole.Start();
            while (!threadConsole.IsAlive) ;
            Console.WriteLine("Defrag Ranking se inicio correctamente...");
            */
            Console.ReadLine();
        }
    }
}
