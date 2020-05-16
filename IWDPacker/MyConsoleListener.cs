using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IWDPacker
{
    class MyConsoleListener : TraceListener
    {
        bool _verbose;

        public MyConsoleListener(bool verbose)
        {
            this.Name = "Trace";
            this._verbose = verbose;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, message, string.Empty);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (String.IsNullOrEmpty(format))
                return;

            if (!_verbose && eventType == TraceEventType.Verbose)
                return;

            string message;
            if (args.Length > 0 && !String.IsNullOrEmpty(args[0].ToString()))
                message = String.Format(format, args);
            else
                message = format;

            Console.WriteLine(message);
        }

        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }
    }
}
