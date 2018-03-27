using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using ScalaLSP.Common;
//using ScalaLSP.Common;

namespace ScalaLSP.Metals
{

    [ContentType("scala")]
    [Export(typeof(ILanguageClient))]
    public class MetalsClient : ILanguageClient
    {
        public string Name => "MetaLS Scala Language Extension";

        private Process metals;

        public IEnumerable<string> ConfigurationSections => null;

        private string connectionToken = null;

        public object InitializationOptions => connectionToken == null ? null : new
        {
            token = connectionToken
        };

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            try
            {
                await Task.Yield();
                var basedir = Util.GetWorkingDirectory();
                var conn = basedir.Fold(bd => {
                    var process = MetalsConnection.StartMetals(bd);
                    metals = process;
                    return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
                }, () => null, () => null);
                return conn; 
            }
            catch (Exception e)
            {
                var whathappened = e;
            }
            var iscancelled = token.IsCancellationRequested;

            return null;
        }


        private System.Threading.Tasks.Task ScalaLanguageClient_StopAsync(object sender, EventArgs args)
        {
            metals.Kill();
            return System.Threading.Tasks.Task.CompletedTask;
        }


        public async System.Threading.Tasks.Task OnLoadedAsync()
        {
            await StartAsync?.InvokeAsync(this, EventArgs.Empty);
        }

        public async System.Threading.Tasks.Task OnServerInitializeFailedAsync(Exception e)
        {
            throw new Exception("On Server Initizile Failed: " + e.Message, e);
        }

        public async System.Threading.Tasks.Task OnServerInitializedAsync()
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
    }
}