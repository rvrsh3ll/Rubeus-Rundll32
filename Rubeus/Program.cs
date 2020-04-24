using Rubeus.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Rubeus
{
    public class Program
    {
        // global that specifies if ticket output should be wrapped or not
        public static bool wrapTickets = true;

        private static void FileExecute(string commandName, Dictionary<string, string> parsedArgs)
        {
            // execute w/ stdout/err redirected to a file

            string file = parsedArgs["/consoleoutfile"];
           

            MainExecute(commandName, parsedArgs);

       
        }
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        private static void MainExecute(string commandName, Dictionary<string, string> parsedArgs)
        {
            IntPtr ptr = GetForegroundWindow();

            int u;

            GetWindowThreadProcessId(ptr, out u);

            Process process = Process.GetProcessById(u);

            AttachConsole(process.Id);
            // main execution logic

            Info.ShowLogo();

            try
            {
                var commandFound = new CommandCollection().ExecuteCommand(commandName, parsedArgs);

                // show the usage if no commands were found for the command name
                if (commandFound == false)
                    Info.ShowUsage();
            }
            catch (Exception e)
            {
                Console.WriteLine("\r\n[!] Unhandled Rubeus exception:\r\n");
                Console.WriteLine(e);
            }
        }
       


        public static string MainString(string command)
        {
            
            // helper that executes an input string command and returns results as a string
            //  useful for PSRemoting execution

            string[] args = command.Split();

            var parsed = ArgumentParser.Parse(args);
            if (parsed.ParsedOk == false)
            {
                Info.ShowLogo();
                Info.ShowUsage();
                return "Error parsing arguments: ${command}";
            }

            var commandName = args.Length != 0 ? args[0] : "";

        

            MainExecute(commandName, parsed.Arguments);
            return command;
        }
        [DllExport]
        static void RunRubeus()
        {
            String commandName = "triage";

            MainString(commandName);
        }
    }
}
