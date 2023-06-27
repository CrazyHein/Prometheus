using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Spire.Pdf.Annotations;
using Syncfusion.Windows.Controls.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console
{
    class DebugConsole
    {
        [DllImport("kernel32")]
        private static extern bool AllocConsole();
        [DllImport("kernel32")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("User32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);
        [DllImport("User32.dll")]
        private static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        private static string __TIMESTAMP_FORMAT = @"MM/dd/yy HH:mm:ss fff";
        private static string __INFO_PROMPT = @"[INFO]";
        private static string __EXCEPTION_PROMPT = @"[ERRO]";
        private DebugConsole() { }
        public static IntPtr CreateConsole()
        {
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd == IntPtr.Zero)
            {
                AllocConsole();
                hWnd = GetConsoleWindow();
            }
            
            if (hWnd != IntPtr.Zero)
            {
                IntPtr menu = GetSystemMenu(hWnd, IntPtr.Zero);
                RemoveMenu(menu, 0xF060, 0x00);
            }

            return hWnd;
        }

        private static string PrintPromptString(string cat, ConsoleColor color = ConsoleColor.DarkGreen)
        {
            var c = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Blue;
            string prompt = $"{DateTime.Now.ToString(__TIMESTAMP_FORMAT)} ";
            string ucat = $"{cat}";
            System.Console.Write(prompt);
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(ucat);
            System.Console.ForegroundColor = c;
            return prompt+cat;
        }

        public static void WriteInfo(string info)
        {
            PrintPromptString(__INFO_PROMPT);
            System.Console.WriteLine(info);
        }

        public static void WriteOperatingRecord(OperatingRecord rd)
        {
            PrintPromptString(__INFO_PROMPT);
            System.Console.WriteLine(rd.DetailedDescription);
        }

        public static void WriteException(Exception ex, OperatingRecord rd)
        {
            PrintPromptString(__EXCEPTION_PROMPT, ConsoleColor.Red);

            System.Console.WriteLine(ex.Message);
            System.Console.WriteLine(rd.DetailedDescription);
        }
    }
}
