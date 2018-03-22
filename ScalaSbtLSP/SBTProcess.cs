using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScalaLSP.SbtServer
{
    public class SBTProcess
    {
        public static readonly string PortfilePath = @"project\target\active.json";
        public static async Task<Process> StartSBT(string basedir)
        {
            var sbtcmd = "sbt";
            ProcessStartInfo sbtStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = basedir,
                FileName = "cmd.exe",

                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            var sbt = new Process
            {
                StartInfo = sbtStartInfo
            };
            //return new Connection(new MemoryStream(), new MemoryStream());
            if (sbt.Start())
            {
                await sbt.StandardInput.WriteLineAsync(sbtcmd);
                return sbt;
            }
            else
            {
                return null;
            }
        }

        public static async Task<FileInfo> WaitForPortfile(String basepath, CancellationToken token)
        {
            
            var file = Path.Combine(basepath, PortfilePath);
            var result = new FileInfo(file);
            while (!result.Exists)
            {
                if (token.IsCancellationRequested) return await Task.FromCanceled<FileInfo>(token);
                await Task.Delay(new TimeSpan(0, 0, 0, 0, 500), token);
            }
            return result;
        }

        public static async Task<SbtPortfile> ReadPortFile(FileInfo portfileLocation, CancellationToken token)
        {
            using (var str = portfileLocation.OpenRead())
            using (var read = new StreamReader(str, Encoding.UTF8))
            {
                var data = await read.ReadToEndAsync();
                return JsonConvert.DeserializeObject<SbtPortfile>(data);
            }
        }

        public static async Task<TokenFile> ReadToken(SbtPortfile portfile, CancellationToken token)
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

        public static async Task<Connection> ConnectTCPAsync(string connectionuri)
        {
            var endpoint = CreateIPEndPoint(connectionuri);
            //no using, no dispose
            var tcpclient = new TcpClient();
            var stream = tcpclient.GetStream();
            var connection = new Connection(stream, stream);
            await tcpclient.ConnectAsync(endpoint.Address, endpoint.Port);
            return connection;
        }

        public static async Task<Connection> ConnectNamedPipeAsync(string connectionuri)
        {
            //if on Windows, named pipe. If on Unix, local socket
            //if on Unix, we're not running this, because no VS
            //so it's a named pipe.
            string pipename = String.Concat(connectionuri.Split(':').Skip(1).ToArray());
            //no using block to avoid disposing pipe
            var pipe = new NamedPipeClientStream(".", pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipe.ConnectAsync();
            var connection = new Connection(pipe, pipe);
            return connection;
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
    }
}
