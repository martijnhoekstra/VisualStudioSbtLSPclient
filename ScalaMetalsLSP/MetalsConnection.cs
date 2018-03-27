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

namespace ScalaLSP.Metals
{
    public class MetalsConnection
    {
        public static Process StartMetals(string basedir)
        {
            var args = @"-noverify -jar C:\users\martijn\coursier launch --repository bintray:scalameta/maven org.scalameta:metals_2.12:977c902b --main scala.meta.metals.Main";
            ProcessStartInfo sbtStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = basedir,
                FileName = "java",
                Arguments = args,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            var metals = new Process
            {
                StartInfo = sbtStartInfo
            };
            if (metals.Start())
            {
                return metals;
                //return new Connection(metals.StandardInput.BaseStream, metals.StandardOutput.BaseStream);
            }
            else
            {
                return null;
            }
        }
    }
}