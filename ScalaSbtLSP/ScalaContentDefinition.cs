using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

/*
 * While doing things doesn't appear to this classes strong suit
 * apparently this has to exist, and does something.
 *  ¯\_(ツ)_/¯
 */

namespace ScalaLSP.SbtServer
{
    public class ScalaContentDefinition
    {
        #pragma warning disable CS0649

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
