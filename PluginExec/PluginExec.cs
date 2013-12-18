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
        // TODO: max lines settings
        // TODO: can we use async to keep rm from hanging while the process is executing? Probably, BUT it's > VS 2012 only :(
        internal StringBuilder outputStr;
        // measure settings
        internal string ExecFile;
        internal string Arguments;
        internal bool writeOut;
        internal string outPath;

        internal Measure()
        {
            outputStr = new StringBuilder();
        }

        internal void Reload(Rainmeter.API rm, ref double maxValue)
        {
            // two basic options: what to run and arguments
            ExecFile = rm.ReadString("ExecFile", "");
            Arguments = rm.ReadString("Arguments", "");
            // we would like to try to write the output to a file, so we can parse it, etc.
            outPath = rm.ReadPath("WriteToFile", null);
            if (outPath.Equals(null))  // do not write output to a file
            {
                writeOut = false;
            }
            else    // try to write to a file
            {
                try
                {
                    if (!File.Exists(outPath))
                    {
                        // create the file and close the FileStream, just to check for errors
                        File.Create(outPath).Close();   
                    }
                    writeOut = true;
                }
                catch (Exception ex)
                {
                    writeOut = false;
                    outPath = null;
                    Rainmeter.API.Log(API.LogType.Error, "Exec: " + ex.Message);
                }
            }
        }

        // it would be nice to methodize the contents of update so that an execution could be triggered by a !CommandMeasure bang
        internal double Update()
        {
            int lines = 0;
            Process proc = null;
            StreamReader outstream = null;
            try
            {
                proc = Process.Start(makepsi());
                outstream = proc.StandardOutput;
                while (!outstream.EndOfStream)
                {
                    outputStr.AppendLine(outstream.ReadLine());
                    lines++;
                }
            }
            catch (Exception ex)
            {
                outputStr.AppendLine(ex.Message);
            }
            finally // always close, even if there is an exception...
            {
                if (proc != null)
                {
                    proc.Close();
                }
                if (outstream != null)
                {
                    outstream.Close();
                }
            }

            if (writeOut)   // conditional attempt to write to a file
            {
                bool worked = writeToFile();
            }

            return (double)lines;   // might as well
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
            psi.UseShellExecute = false;    // must be false to redirect standard output
            psi.CreateNoWindow = true;
            psi.ErrorDialog = true;
            psi.RedirectStandardOutput = true;

            psi.FileName = ExecFile;
            psi.Arguments = Arguments;

            return psi;
        }

        private bool writeToFile()
        {
            StreamWriter writer = null;
            try
            {
                using (writer = new StreamWriter(outPath))
                {
                    writer.Write(outputStr);
                }
                writer.Close();
            }
            catch (Exception)
            {
                if (writer != null)
                {
                    writer.Close();
                }
                return false;
            }
            return true;
        }

    }

    /* Plugin class binds 'Measure' methods from above to the plugin API */
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