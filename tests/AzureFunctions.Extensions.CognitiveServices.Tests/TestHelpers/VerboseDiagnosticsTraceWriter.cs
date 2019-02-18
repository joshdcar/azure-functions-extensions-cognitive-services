using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Tests.TestHelpers
{
    public class VerboseDiagnosticsTraceWriter : TraceWriter
    {

        public VerboseDiagnosticsTraceWriter() : base(TraceLevel.Verbose)
        {

        }
        public override void Trace(TraceEvent traceEvent)
        {
            Debug.WriteLine(traceEvent.Message);
        }
    }
}
