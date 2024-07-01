using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console
{
    class DebugConsole
    {
        [DllImport("kernel32")]
        private static extern bool AllocConsole();
        [DllImport("kernel32")]
        private static extern bool FreeConsole();
        [DllImport("kernel32")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("User32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);
        [DllImport("User32.dll")]
        private static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        private static string __TIMESTAMP_FORMAT = @"MM/dd/yy HH:mm:ss fff";
        private static string __INFO_PROMPT = @"[INFO]";
        private static string __EXCEPTION_PROMPT = @"[ERRO]";
        private static string __FILE_TIMESTAMP_FORMAT = @"MMddyyHHmmssfff";

        private static IntPtr __debug_console = IntPtr.Zero;
        private static FileSecurity? __debug_log_security;
        private static FileStream? __debug_log = null;
        private static int __debug_log_size_limit = 1024 * 1024;
        private static int __debug_log_flush_limit = 1024 * 4;
        private DebugConsole() { }
        public static void CreateConsole(bool enableDebugConsole, bool enableDebugLog, int logSizeLimit, int logBufferSize)
        {
            if (enableDebugConsole)
            {
                __debug_console = GetConsoleWindow();
                if (__debug_console == IntPtr.Zero)
                {
                    AllocConsole();
                    __debug_console = GetConsoleWindow();
                }

                if (__debug_console != IntPtr.Zero)
                {
                    IntPtr menu = GetSystemMenu(__debug_console, IntPtr.Zero);
                    RemoveMenu(menu, 0xF060, 0x00);
                }
            }
            else
            {
                if(__debug_console != IntPtr.Zero)
                {
                    FreeConsole();
                    __debug_console = IntPtr.Zero;
                }
            }
            if (enableDebugLog)
            {
                try
                {
                    if (__debug_log == null)
                    {
                        var identity = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                        var accessRule = new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow);
                        __debug_log_security = new FileSecurity();
                        __debug_log_security.AddAccessRule(accessRule);

                        if (!System.IO.Directory.Exists("DebugConsole"))
                            System.IO.Directory.CreateDirectory("DebugConsole");
                        FileInfo info = new FileInfo($"./DebugConsole/{DateTime.Now.ToString(__FILE_TIMESTAMP_FORMAT)}.dbg");
                        __debug_log_size_limit = logSizeLimit;
                        __debug_log_flush_limit = logBufferSize;
                        __debug_log = info.Create(FileMode.Create, System.Security.AccessControl.FileSystemRights.WriteData, FileShare.Read, __debug_log_flush_limit, FileOptions.SequentialScan, __debug_log_security);
                    }
                }
                catch
                {
                    __debug_log = null;
                }
            }
            else
            {
                if (__debug_log != null)
                {
                    __debug_log.Close();
                    __debug_log.Dispose();
                    __debug_log = null;
                }
            }
        }

        public static void Flush()
        {
            __debug_log?.Flush();
        }

        private static string PrintPromptString(string cat, ConsoleColor color = ConsoleColor.DarkGreen)
        {
            string prompt = $"{DateTime.Now.ToString(__TIMESTAMP_FORMAT)} ";
            string ucat = $"{cat}";
            if (__debug_console != IntPtr.Zero)
            {
                var c = System.Console.ForegroundColor;
                System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.Write(prompt);
                System.Console.ForegroundColor = color;
                System.Console.WriteLine(ucat);
                System.Console.ForegroundColor = c;
            }
            if(__debug_log != null)
            {
                __debug_log.Write(Encoding.UTF8.GetBytes(prompt + cat + "\r\n"));
            }
            return prompt+cat;
        }

        private static void PrintMessageString(string msg)
        {
            if (__debug_console != IntPtr.Zero)
            {
                System.Console.WriteLine(msg);
            }
            if (__debug_log != null)
            {
                __debug_log.Write(Encoding.UTF8.GetBytes(msg + "\r\n"));
                if (__debug_log.Length >= __debug_log_size_limit)
                {
                    __debug_log.Flush();
                    __debug_log.Close();
                    __debug_log.Dispose();
                    try
                    {
                        FileInfo info = new FileInfo($"./DebugConsole/{DateTime.Now.ToString(__FILE_TIMESTAMP_FORMAT)}.dbg");
                        __debug_log = info.Create(FileMode.Create, System.Security.AccessControl.FileSystemRights.WriteData, FileShare.Read, __debug_log_flush_limit, FileOptions.SequentialScan, __debug_log_security);
                    }
                    catch
                    {
                        __debug_log = null;
                    }
                }
            }
        }

        public static void WriteInfo(string info)
        {
            PrintPromptString(__INFO_PROMPT);
            PrintMessageString(info);
        }
        public static void WriteException(string ex)
        {
            PrintPromptString(__EXCEPTION_PROMPT, ConsoleColor.Red);
            PrintMessageString(ex);
        }
        public static void WriteException(Exception ex)
        {
            PrintPromptString(__EXCEPTION_PROMPT, ConsoleColor.Red);
            PrintMessageString(ex.Message);
        }

        public static void WriteOperatingRecord(OperatingRecord rd)
        {
            PrintPromptString(__INFO_PROMPT);
            PrintMessageString(rd.DetailedDescription);
        }

        public static void WriteException(Exception ex, OperatingRecord rd)
        {
            PrintPromptString(__EXCEPTION_PROMPT, ConsoleColor.Red);

            PrintMessageString(ex.Message);
            PrintMessageString(rd.DetailedDescription);
        }
    }
}
