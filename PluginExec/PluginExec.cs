using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Rainmeter;

// TODO: asynchronous stdout and file write, so that (maybe) Rainmeter won't hang when you execute a long command
// STAHP THE THREAD.  (Yikes.)  We need to spawn a thread once on refresh and anytime on demand, BUT
// whenever we start a thread we MUST somehow terminate the old one, so that multiple threads aren't writing to the output string. (!)
namespace PluginExec
{
    internal class Measure
    {
        private static StringBuilder outputStr = new StringBuilder();
        private static readonly object locker = new object();  // locker for shared static field outputStr
        // measure settings
        internal string ExecFile;
        internal string Arguments;
        internal string outPath;
        // instance reference to a secondary thread (get rid of this.  Geez.)
        private Thread procThread = null;

        internal Measure()
        {
        }

        internal void Reload(Rainmeter.API rm, ref double maxValue)
        {
            // two basic options: what to run and arguments
            ExecFile = rm.ReadString("ExecFile", "");
            Arguments = rm.ReadString("Arguments", "");
            // we would like to try to write the output to a file, so we can parse it, etc.
            outPath = rm.ReadPath("WriteToFile", "");   // implement later
#if DEBUG
            Rainmeter.API.Log(Rainmeter.API.LogType.Notice, "Read settings, spawing thread");
#endif
            SpawnProcThread();
        }

        /* this method doesn't actually do anything: we want the process to run only 
         * on refresh or on request through a !CommandMeasure bang
         * TODO: return running state of process: (Running ? 1 : 0)
         */
        internal double Update()
        {
            return 1.0;
        }

        internal string GetString()
        {
            string t = "-1";
            lock (locker)
            {
                t = Measure.outputStr.ToString();
            }
            return t;
        }

        /* Will need this later */
        //internal void ExecuteBang(string args)
        //{
        //}

        /* in theory, this will keep the main thread from hanging while we wait for the process to finish running.  
         * ...Right? Yes. Sort of.
         */
        private void SpawnProcThread()
        {
            Measure.outputStr.Remove(0, outputStr.Length);
            
            procThread = new Thread(delegate()
                {
                    RunProc(ExecFile, Arguments);
                });
            procThread.Start();
            
        }

        /* Starts the process and waits for it to finish.  
         * Redirects asynchronous stdout to an event handler that updates the static StringBuilder.
         */
        private void RunProc(string file, string args)
        {
            Process proc = new Process();
            // set properties
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = file;
            proc.StartInfo.Arguments = args;
            // event handler for asynchronous output
            proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputHandler);
            proc.Start();   // start process
            proc.BeginOutputReadLine(); // start asynch output
            // finish
            proc.WaitForExit();
            proc.Close();
        }

        private void proc_OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            lock (locker)
            {
                Measure.outputStr.AppendLine(outLine.Data);
            }
            
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