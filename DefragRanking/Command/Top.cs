using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefragRanking.Command
{
    class Top
    {
        private int id;
        private string nameCmd;
        private string map;
        private string physic;
        private Boolean show;


        public Top(string nameCmd, string map, string physic, Boolean show)
        {
            this.nameCmd = nameCmd;
            this.map = map;
            this.physic = physic;
            this.show = show;
        }

        public string getNameCmd()
        {
            return this.nameCmd;
        }

        public int executeCmd()
        {
            Console.WriteLine("ejecuto el comando: " +nameCmd + " " + map + " " + physic + " "+ show.ToString());
            return 1;
        }

    }
}
