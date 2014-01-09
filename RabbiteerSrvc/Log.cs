using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    public class Log
    {
        public static bool EnableDebug {get; set;} 
        public static bool IsEventLog {get; set;}

        private static string EventSource = "Rabbiteer";
        private static string EventLogName = "APPLICATION";


        static Log() {
            IsEventLog = false;
#if (DEBUG)
            EnableDebug = true;
#else
            EnableDebug = false;
#endif
        }

        public static void Debug(string format, params object[] args)
        {
            Out(0, format, args);
        }
        public static void Info(string format, params object[] args)
        {
            Out(1, format, args);
        }
        public static void Warn(string format, params object[] args)
        {
            Out(2, format, args);
        }
        public static void Error(string format, params object[] args)
        {
            Out(3, format, args);
        }

        private static void Out(int level, string format, params object[] args)
        {
            if (!EnableDebug && level == 0) return;
            string msg = String.Format(format, args);
            if (IsEventLog)
            {
                // we assume the event log already exists from installation.
                EventLogEntryType t = EventLogEntryType.Information;
                // debug mas to Information
                if (level == 2)
                {
                    t = EventLogEntryType.Warning;
                }
                else if (level == 3)
                {
                    t = EventLogEntryType.Error;
                }
                EventLog.WriteEntry(EventSource, msg, t);
            }
            else
            {
                String t = "INFO";
                if (level == 0) {
                    t = "DEBUG";
                }
                else if (level == 1)
                {
                    t = "INFO";
                } else if (level == 2)
                {
                    t = "WARN";
                }
                else if (level == 3)
                {
                    t = "ERROR";
                }
                msg = String.Format("{0}: {1}", t, msg);
                Console.WriteLine(msg);
            }
        }

    }
}
