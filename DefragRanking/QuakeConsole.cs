using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;

namespace DefragRanking
{
    class QuakeConsole
    {
        //Find Windows
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        //EnumChildWindow
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpCallbackFunction, string lpParam);
        
        [DllImport("user32.dll")]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpCallbackFunction, ref SearchData data);

        //EnumWindows
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpCallbackFunction, ref SearchData data);

        //GetClassName de un control o ventana
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        
        //GetWindowsText
        [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
        private extern static int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

        //GetWindowLong para comparar su estyle WS_STYLE ES_READONLY
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //SendMessage Esta uso para enviar Strings con /
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        //SendMessage para Leer de la consola
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [Out] StringBuilder lParam);

        # region Windows API declarations
        private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);
        #endregion

        private const   int      WM_USER         = 0x400;
        private const   int      WM_COPYDATA     = 0x4A;
        private const   int      WM_KEYDOWN      = 0x100;
        private const   int      WM_KEYUP        = 0x101;
        private const   int      WM_SYSKEYDOWN   = 0x104;
        private const   int      WM_SYSKEYUP     = 0x105;
        private const   int      WM_CHAR         = 0x0102;         // 0x102: slash - 0x105: Minusculas
        private const   int      WM_SETTEXT      = 0x000c;
        private const   int      WM_GETTEXT      = 0x000D;
        private const   int      WM_GETTEXTLENGTH = 0x000E;
        private const   int      GWL_STYLE       = (-16);          //para recuperar el style de un control
        private const   UInt32   ES_READONLY     = 0x800;          //atributo del tipo style de un control

        private const   string   TXTCONTROL      = "Q3Edit";
        private const   string   LBLCONTROL      = "Q3Label";

        private         IntPtr   handleQ3wnd     = IntPtr.Zero;
        private         IntPtr   handleQ3Txt     = IntPtr.Zero;  //Control Edit sin ES_READONLY
        private         IntPtr   handleQ3Lbl      = IntPtr.Zero;   //Control Edit con style = ES_READONLY 
        private         IntPtr   handleQ3Clr     = IntPtr.Zero;

        private volatile String  Consola           = "123";
        private         bool     bStopProgram;

        public QuakeConsole() { }

        public QuakeConsole(string className,string windowName) 
        {
            this.handleQ3wnd  = FindWindow(className, windowName);
            this.handleQ3Txt  = getControl(TXTCONTROL);  //Encuentra los handles al textbox y al label
            this.handleQ3Lbl  = getControl(LBLCONTROL);
        }

        public QuakeConsole(IntPtr hWnd) 
        {
            this.handleQ3wnd = hWnd;
            this.handleQ3Txt = getControl(TXTCONTROL);  //Encuentra los handles al textbox y al label
            this.handleQ3Lbl  = getControl(LBLCONTROL);
        }

        public List<IntPtr> getQuakeServers(string className, string windowName)
        {
            SearchData sd = new SearchData { className=className, windowName=windowName };
            EnumWindows(new EnumWindowsProc(EnumWindow), ref sd);
            
            return sd.hWnds;
        }
        
        private static bool EnumWindow(IntPtr hWnd, ref SearchData data)
        {
            if (data.windowName == null)    data.windowName = "";
            if (data.className  == null)    data.className = "";
            StringBuilder sb = new StringBuilder(1024);
            GetClassName(hWnd, sb, sb.Capacity);
            if (sb.ToString().StartsWith(data.className))
            {
                sb = new StringBuilder(1024);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().StartsWith(data.windowName))
                {
                    data.hWnds.Add(hWnd);
                    return true;    // Found one wnd, keep looking
                }
            }
            return true;
        }

        private IntPtr getControl(string controlName)
        {
            if (!this.handleQ3wnd.Equals(IntPtr.Zero))
            {
                SearchData sd = new SearchData() { lpParam = controlName };
                EnumChildWindows(this.handleQ3wnd, new EnumWindowsProc(EnumWindowControls), ref sd);
                return sd.hWnds[0];
            }
            return IntPtr.Zero;
        }

        //Funcion callback x cada hijo encuentra los handle a Edit y Label
        //Retorno: true -> siguie buscando --> false se detiene
        private static bool EnumWindowControls(IntPtr hWnd, ref SearchData sd)
        {
            StringBuilder className = new StringBuilder(100);
            //Get the window class name
            GetClassName(hWnd, className, className.Capacity);

            if (!className.ToString().Equals("Edit")) return true;
            switch (sd.lpParam)
            {
                case TXTCONTROL:
                    {
                        if ((ES_READONLY & GetWindowLong(hWnd, GWL_STYLE)) != 0) return true;
                        sd.hWnds.Add(hWnd);
                        return false;
                    }
                case LBLCONTROL:
                    {
                        if ((ES_READONLY & GetWindowLong(hWnd, GWL_STYLE)) == 0) return true;
                        sd.hWnds.Add(hWnd);
                        return false;
                    }
            }
            return true;
        }

        //Metodo para enviar string a la consola
        public Boolean sendStringToConsole(string msg)
        {
            Boolean result = false;

            if (!this.handleQ3wnd.Equals(IntPtr.Zero))
            {
                for (int i = 0; i < msg.Length; i++)
                    SendMessage(this.handleQ3Txt, WM_CHAR, (int)msg[i], 0); //manda cada char del string

                SendMessage(this.handleQ3Txt, WM_CHAR, 13, 1); //manda un Enter
                result = true;
            }
            return result;
        }

        //Metodo para limpiar la consola del juego.
        public Boolean clearConsole()
        {
            Boolean result = false;
            if (!this.handleQ3wnd.Equals(IntPtr.Zero))
            {
                SendMessage(this.handleQ3Lbl, WM_SETTEXT, 0, 0);
                result = true;
            }
            return result;
        }

        //Metodo para Scanear un string en la consola
        public void getConsole()
        {
            StringBuilder console = new StringBuilder();
            Int32 size;

            // Get the size of the string required to hold the window title/control text. 
            size = SendMessage(this.handleQ3Lbl, WM_GETTEXTLENGTH, 0, 0);

            // If the return is 0, there is no title. 
            if (size > 0)
            {
                console = new StringBuilder(size + 1);
                SendMessage(this.handleQ3Lbl, WM_GETTEXT, console.Capacity, console);
                Consola = console.ToString();
                //Console.WriteLine(Consola);
                clearConsole();
                getCommand();
            }
        }

        //Metodo que scanea un comando
        private void getCommand()
        {
            string[] words = Regex.Split(Consola, "\xD\xA");

            foreach (string line in words)
            {
                if (line.StartsWith("say:"))
                {
                    string[] parts = line.Split(':');
                    if (parts[2].Trim().StartsWith("!"))
                    {
                        
                    }
                }
            }              
            
        }

        public void initialize()
        {
            while (!bStopProgram)
            {
                //Console.WriteLine(this.handleQ3Txt);
                //Console.WriteLine("entre");
                //sendStringToConsole("123");
                getCommand();

                Thread.Sleep(1000);
            }
        }

        public void requestStopProgram()
        {
            this.bStopProgram = true;
        }

    }

    public class SearchData
    {
        public string className;
        public string windowName;
        public List<IntPtr> hWnds = new List<IntPtr>();
        public string lpParam;

    }
}
