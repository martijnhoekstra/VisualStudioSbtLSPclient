using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;

namespace ScalaSbtLSP
{

    [ContentType("scala")]
    [Export(typeof(ILanguageClient))]
    public class ScalaLanguageClient : ILanguageClient
    {
        public string Name => "Scala Language Extension";

        public IEnumerable<string> ConfigurationSections => null;

        private string connectionToken = null;

        public object InitializationOptions => connectionToken == null ? null : new
        {
            token = connectionToken
        };

        public IEnumerable<string> FilesToWatch => null;

        public async Task<TokenFile> ReadToken(SbtPortfile portfile, CancellationToken token)
        {
            if (portfile.tokenFilePath == null)
            {
                return null;
            }
            else
            {
                using (var str = File.OpenRead(portfile.tokenFilePath))
                using (var reader = new StreamReader(str, Encoding.UTF8))
                {
                    var data = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<TokenFile>(data);
                }
            }
        }

        private static async Task<SbtPortfile> ReadPortFile(FileInfo portfileLocation, CancellationToken token)
        {
            using (var str = portfileLocation.OpenRead())
            using (var read = new StreamReader(str, Encoding.UTF8))
            {
                var data = await read.ReadToEndAsync();
                return JsonConvert.DeserializeObject<SbtPortfile>(data);
            }
        }

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();
            Debugger.Launch();
            Debugger.Break();
            ProcessStartInfo sbtStartInfo = new ProcessStartInfo();
            string workingdirectory = @"C:\Users\marti\scala\test\";
            sbtStartInfo.WorkingDirectory = workingdirectory;
            sbtStartInfo.FileName = "sbt";
            sbtStartInfo.RedirectStandardInput = false;
            sbtStartInfo.RedirectStandardOutput = false;
            sbtStartInfo.UseShellExecute = true;
            sbtStartInfo.CreateNoWindow = false;

            Process process = new Process();
            process.StartInfo = sbtStartInfo;

            if (process.Start())
            {
                var portfilelocation = await WaitForPortfile(workingdirectory, token);
                var portfile = await ReadPortFile(portfilelocation, token);
                var tokenfile = await ReadToken(portfile, token);
                if (tokenfile != null)
                {
                    connectionToken = tokenfile.token;
                }

                var connectionuri = portfile.uri;
                if (connectionuri.StartsWith("local:"))
                {

                    //if on Windows, named pipe. If on Unix, local socket
                    //if on Unix, we're not running this, because no VS
                    //so it's a named pipe.
                    string pipename = String.Concat(connectionuri.Split(':').Skip(1).ToArray());
                    using (var pipe = new NamedPipeClientStream(".", pipename, PipeDirection.InOut))
                    {
                        return new Connection(pipe, pipe);
                    }
                }
                else if (connectionuri.StartsWith("tcp:"))
                {
                    //tcp
                    var endpoint = CreateIPEndPoint(tokenfile.uri);

                    using (var tcpclient = new TcpClient())
                    using (var stream = tcpclient.GetStream())
                    {
                        var connection = new Connection(stream, stream);
                        await tcpclient.ConnectAsync(endpoint.Address, endpoint.Port);
                        return connection;
                    }
                }
                //if we got to here, we got an unknown protocal, and we can't connect
                //fall through
            }


            return null;
        }

        private static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out int port))
            {
                throw new FormatException("Invalid port");
            }
            else
            {
                return new IPEndPoint(ip, port);
            }
        }

        private async Task<FileInfo> WaitForPortfile(String basepath, CancellationToken token)
        {
            var subpath = "project\target\active.json";
            var file = Path.Combine(basepath, subpath);
            var result = new FileInfo(file);
            while (!result.Exists)
            {
                if (token.IsCancellationRequested) return await Task.FromCanceled<FileInfo>(token);
                await Task.Delay(new TimeSpan(0, 0, 0, 0, 500), token);
            }
            return result;
        }

        public async Task OnLoadedAsync()
        {
            await StartAsync?.InvokeAsync(this, EventArgs.Empty);
        }

        public async Task OnServerInitializeFailedAsync(Exception e)
        {
            await Task.CompletedTask;
        }

        public async Task OnServerInitializedAsync()
        {
            await Task.CompletedTask;
        }
    }
}