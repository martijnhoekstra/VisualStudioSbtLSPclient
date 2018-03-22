/*
 * Doesn't get "picked up" when in a different assembly.
 * I'm unsure what "getting picked up" even means. 
 * 
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace ScalaLSP.Common
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
*/