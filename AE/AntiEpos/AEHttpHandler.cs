using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntiEpos
{
    public class AEHttpHandler : DelegatingHandler
    {
        public bool DoNotPrintInDebug { get; set; }

        public AEHttpHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!DoNotPrintInDebug && Debugger.IsAttached)
            {
                await Console.Out.WriteLineAsync("!! Request begin, data:");
                await Console.Out.WriteLineAsync(request.ToString());
                await Console.Out.WriteLineAsync("!! Request content:");
                await Console.Out.WriteLineAsync(request.Content is null ? "no content" : await request.Content.ReadAsStringAsync());
                await Console.Out.WriteLineAsync("!! Request end --");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
