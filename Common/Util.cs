using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalaLSP.Common
{
    public class Util
    {
        public static string GetWorkingDirectory()
        {
            var solutiono = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution));
            var solution = solutiono as IVsSolution;

            solution.GetSolutionInfo(out string workingdirectory, out string file, out string ops);
            // dir will contain the solution's directory path (folder in the open folder case)

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object open);
            bool isOpen = (bool)open; // is the solution open?

            // __VSPROPID7 needs Microsoft.VisualStudio.Shell.Interop.15.0.DesignTime.dll
            solution.GetProperty((int)__VSPROPID7.VSPROPID_IsInOpenFolderMode, out object folderMode);
            bool isInFolderMode = (bool)folderMode; // is the solution in folder mode?
            if (!isInFolderMode) throw new Exception("Not in Folder Mode");
            if (workingdirectory == null) throw new Exception("Working directory not found (is Visual Studio in Folder Mode?)");
            return workingdirectory;
        }

    }
}
