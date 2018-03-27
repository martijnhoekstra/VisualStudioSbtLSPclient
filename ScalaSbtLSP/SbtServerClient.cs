using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
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

        public IEnumerable<string> ConfigurationSections => null;

        private string connectionToken = null;

        public object InitializationOptions => connectionToken == null ? null : new
        {
            token = connectionToken
        };

        public IEnumerable<string> FilesToWatch => new List<String>() { @"**/src/**/scala/**/*.scala" };

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            Task<Connection> noConnection() => Task.FromResult<Connection>(null);
            try
            {
                await Task.Yield();
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
            var (connectionuri, ctoken) = await SBTProcess.WaitForConnectionUriAndTokenAsync(workingdirectory, token);
            if (connectionuri == null) return null;
            else
            {
                connectionToken = ctoken;
                return await ConnectPortfileURLAsync(connectionuri);
            }
        } 

        private static async Task<Connection> ConnectPortfileURLAsync(string connectionuri)
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


        public async Task OnLoadedAsync()
        {
            var workingdirectoryresult = Util.GetWorkingDirectory();
            var sbt = await workingdirectoryresult.Fold(SBTProcess.StartSBT, () => throw new Exception("Visual Studio is not in folder mode"), () => throw new Exception("solution folder was null"));
            if(sbt == null)
            {
                throw new Exception("SBT failed to start");
            }
            StopAsync += (object sender, EventArgs args) =>
            {
                sbt.Kill();
                return Task.CompletedTask;
            };
            await StartAsync?.InvokeAsync(this, EventArgs.Empty);
        }

        public Task OnServerInitializeFailedAsync(Exception e) => Task.FromException(new Exception("On Server Initizile Failed: " + e.Message, e));

        public async Task OnServerInitializedAsync()
        {
            await Task.CompletedTask;
        }
    }
}