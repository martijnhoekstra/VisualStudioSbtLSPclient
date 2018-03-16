using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalaSbtLSP
{
  public class ScalaContentDefinition
  {
    [Export]
    [Name("scala")]
    [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
    internal static ContentTypeDefinition ScalaContentTypeDefinition;


    [Export]
    [FileExtension(".scala")]
    [ContentType("scala")]
    internal static FileExtensionToContentTypeDefinition ScalaFileExtensionDefinition;
  }
}
