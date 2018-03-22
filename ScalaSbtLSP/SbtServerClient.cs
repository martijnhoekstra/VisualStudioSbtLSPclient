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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;

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

        public object InitializationOptions {
            get {
                Console.WriteLine("passing initialization object");
                return connectionToken == null ? null : new
                {
                    token = connectionToken
                };
            }
        }

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
            
            try
            {
                await System.Threading.Tasks.Task.Yield();
                string workingdirectory = Util.GetWorkingDirectory();
                var sbtcmd = "sbt";
                ProcessStartInfo sbtStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingdirectory,
                    FileName = "cmd.exe",

                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                sbt = new Process
                {
                    StartInfo = sbtStartInfo
                };
                //return new Connection(new MemoryStream(), new MemoryStream());
                if (sbt.Start())
                {
                    StopAsync += ScalaLanguageClient_StopAsync;
                    await sbt.StandardInput.WriteLineAsync(sbtcmd);
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
                        //no using block to avoid disposing pipe
                        try
                        {
                            var pipe = new NamedPipeClientStream(".", pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
                            await pipe.ConnectAsync();
                            var connection = new Connection(pipe, pipe);
                            return connection;
                        }
                        catch (Exception e)
                        {
                            return null;
                        }



                    }
                    else if (connectionuri.StartsWith("tcp:"))
                    {
                        //tcp
                        var endpoint = CreateIPEndPoint(tokenfile.uri);
                        //no using, no dispose
                        var tcpclient = new TcpClient();
                        var stream = tcpclient.GetStream();
                        {
                            var connection = new Connection(stream, stream);
                            await tcpclient.ConnectAsync(endpoint.Address, endpoint.Port);
                            return connection;
                        }
                    }
                    //if we got to here, we got an unknown protocal, and we can't connect
                    //fall through
                }
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
            sbt.Kill();
            return System.Threading.Tasks.Task.CompletedTask;
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

        private static async Task<FileInfo> WaitForPortfile(String basepath, CancellationToken token)
        {
            var subpath = @"project\target\active.json";
            var file = Path.Combine(basepath, subpath);
            var result = new FileInfo(file);
            while (!result.Exists)
            {
                if (token.IsCancellationRequested) return await System.Threading.Tasks.Task.FromCanceled<FileInfo>(token);
                await System.Threading.Tasks.Task.Delay(new TimeSpan(0, 0, 0, 0, 500), token);
            }
            return result;
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