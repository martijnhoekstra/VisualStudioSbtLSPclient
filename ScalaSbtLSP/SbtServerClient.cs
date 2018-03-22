using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using ScalaLSP.Common;

namespace ScalaLSP.SbtServer
{

    [ContentType("scala")]
    [Export(typeof(ILanguageClient))]
    public class SbtServerClient : ILanguageClient
    {
        public string Name => "SBT Server Scala Language Extension";

        private Process sbt;

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
            Task<Connection> noConnection() => System.Threading.Tasks.Task.FromResult<Connection>(null);
            try
            {
                await System.Threading.Tasks.Task.Yield();
                var workingdirectoryresult = Util.GetWorkingDirectory();
                return await workingdirectoryresult.Fold(str => ConnectToWorkingDirectoryAsync(str, token), noConnection, noConnection);
            }
            catch (Exception e)
            {
                var whathappened = e;
            }
            var iscancelled = token.IsCancellationRequested;

            return null;
        }

        private async Task<Connection> ConnectToWorkingDirectoryAsync(string workingdirectory, CancellationToken token)
        {
            string connectionuri = await WaitForSBTStartAsync(workingdirectory, token);
            if (connectionuri == null) return null;
            else
            {
                StopAsync += ScalaLanguageClient_StopAsync;
                return await ConnectPortfileURLAsync(connectionuri);
            }
        }

        private async Task<Connection> ConnectPortfileURLAsync(string connectionuri)
        {
            Connection connection = null;
            if (connectionuri.StartsWith("local:"))
            {
                connection = await SBTProcess.ConnectNamedPipeAsync(connectionuri);
            }
            else if (connectionuri.StartsWith("tcp:"))
            {
                connection = await SBTProcess.ConnectTCPAsync(connectionuri);
            }
            return connection;
        }

        private async Task<string> WaitForSBTStartAsync(string workingdirectory, CancellationToken token)
        {
            sbt = await SBTProcess.StartSBT(workingdirectory);
            if (sbt == null) return null;
            else
            {
                var portfilelocation = await SBTProcess.WaitForPortfile(workingdirectory, token);
                var portfile = await SBTProcess.ReadPortFile(portfilelocation, token);
                var tokenfile = await SBTProcess.ReadToken(portfile, token);
                if (tokenfile != null)
                {
                    connectionToken = tokenfile.token;
                }

                var connectionuri = portfile.uri;
                return connectionuri;
            }
        }

        private System.Threading.Tasks.Task ScalaLanguageClient_StopAsync(object sender, EventArgs args)
        {
            sbt.Kill();
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