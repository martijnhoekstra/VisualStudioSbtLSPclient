using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace ScalaLSP.Common
{
    public interface WorkingDirectory
    {
        A Fold<A>(Func<String, A> onDir, Func<A> onNoFolderMode, Func<A> onNoActiveFolder);
    }

    public class Util
    {
        private struct OpenWorkingDirectory : WorkingDirectory
        {
            private string Dir;
            public OpenWorkingDirectory(string dir) { Dir = dir; }

            public A Fold<A>(Func<string, A> onDir, Func<A> onNoFolderMode, Func<A> onNoActiveFolder) => onDir(Dir);
        }
        private struct NotInFolderMode : WorkingDirectory
        {
            public A Fold<A>(Func<string, A> onDir, Func<A> onNoFolderMode, Func<A> onNoActiveFolder) => onNoFolderMode();
        }
        private struct NoActiveFolder : WorkingDirectory
        {
            public A Fold<A>(Func<string, A> onDir, Func<A> onNoFolderMode, Func<A> onNoActiveFolder) => onNoActiveFolder();
        }

        public static WorkingDirectory GetWorkingDirectory()
        {
            var solutiono = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution));
            var solution = solutiono as IVsSolution;

            solution.GetSolutionInfo(out string workingdirectory, out string file, out string ops);
            // dir will contain the solution's directory path (folder in the open folder case)

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object open);
            bool isOpen = (bool)open; // is the solution open?

            // __VSPROPID7 needs Microsoft.VisualStudio.Shell.Interop.15.0.DesignTime.dll
            //solution.GetProperty((int)__VSPROPID7.VSPROPID_IsInOpenFolderMode, out object folderMode);
            //bool isInFolderMode = (bool)folderMode; // is the solution in folder mode?
            //if (!isInFolderMode) return new NotInFolderMode();
            if (workingdirectory == null) return new NoActiveFolder();
            return new OpenWorkingDirectory(workingdirectory);
        }
    }
}
