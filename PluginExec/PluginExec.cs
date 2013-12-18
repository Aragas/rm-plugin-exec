using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: Measure.GetString, Plugin.GetString, Measure.ExecuteBang, and
// Plugin.ExecuteBang have been commented out. If you need GetString
// and/or ExecuteBang and you have read what they are used for from the
// SDK docs, uncomment the function(s). Otherwise leave them commented out
// (or get rid of them)!

namespace PluginExec
{
    internal class Measure
    {
        // TODOs: dump to file and max lines settings
        // TODO: should update be irrelevant?
        internal StringBuilder outputStr;
        internal string ExecFile;
        internal string Arguments;

        internal Measure()
        {
            outputStr = new StringBuilder();
        }

        internal void Reload(Rainmeter.API rm, ref double maxValue)
        {
            ExecFile = rm.ReadString("ExecFile", "");
            Arguments = rm.ReadString("Arguments", "");
        }

        internal double Update()
        {
            ProcessStartInfo procinfo = makepsi();
            int lines = 0;
            try
            {
                using (Process proc = Process.Start(procinfo))
                {
                    using (StreamReader outstream = proc.StandardOutput)
                    {
                        while (!outstream.EndOfStream)
                        {
                            outputStr.AppendLine(outstream.ReadLine());
                            lines++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                outputStr.AppendLine(ex.Message);
            }

            return 1.0;
        }

        internal string GetString()
        {
            return outputStr.ToString();
        }

        //internal void ExecuteBang(string args)
        //{
        //}

        private ProcessStartInfo makepsi()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.ErrorDialog = true;
            psi.RedirectStandardOutput = true;

            psi.FileName = ExecFile;
            psi.Arguments = Arguments;

            return psi;
        }

    }

    public static class Plugin
    {
        [DllExport]
        public unsafe static void Initialize(void** data, void* rm)
        {
            uint id = (uint)((void*)*data);
            Measures.Add(id, new Measure());
        }

        [DllExport]
        public unsafe static void Finalize(void* data)
        {
            uint id = (uint)data;
            Measures.Remove(id);
        }

        [DllExport]
        public unsafe static void Reload(void* data, void* rm, double* maxValue)
        {
            uint id = (uint)data;
            Measures[id].Reload(new Rainmeter.API((IntPtr)rm), ref *maxValue);
        }

        [DllExport]
        public unsafe static double Update(void* data)
        {
            uint id = (uint)data;
            return Measures[id].Update();
        }

        [DllExport]
        public unsafe static char* GetString(void* data)
        {
            uint id = (uint)data;
            fixed (char* s = Measures[id].GetString()) return s;
        }

        //[DllExport]
        //public unsafe static void ExecuteBang(void* data, char* args)
        //{
        //    uint id = (uint)data;
        //    Measures[id].ExecuteBang(new string(args));
        //}

        internal static Dictionary<uint, Measure> Measures = new Dictionary<uint, Measure>();
    }
}