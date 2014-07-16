using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Extensions.Tasks.Microsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NAnt.Extensions.Tasks.Checksum
{
    [TaskName("ValidateChecksum")]
    public class ValidateChecksum : NAnt.Core.Task
    {
        [BuildElement("in", Required = true)]
        public virtual MultiInElement InElement { get; set; }

        [TaskAttribute("algorithm", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual String Algorithm { get; set; }

        [TaskAttribute("debug", Required = false)]
        [BooleanValidator()]
        public bool DebugTask{ get; set; }

        protected override void ExecuteTask()
        {
            if (DebugTask && !System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

            NAnt.Contrib.Functions.FileFunctions contribFileFunctions = new Contrib.Functions.FileFunctions(base.Project, base.Properties);

            if (null != InElement && 0 < InElement.Count && 0 < InElement[0].FileNames.Count)
            {
                for (int idx = 0; idx < InElement.Count; idx++)
                {
                    FileSet fs = InElement[idx];
                    for (int idx2 = 0; idx2 < fs.FileNames.Count; idx2++)
                    {
                        FileInfo checksumFile = new FileInfo(fs.FileNames[idx2]);
                        FileInfo fileToCheck = new FileInfo(NAnt.Core.Functions.PathFunctions.GetFileNameWithoutExtension(checksumFile.FullName));

                        if (!checksumFile.Exists) throw new FileNotFoundException("Can't find checksum file", checksumFile.FullName);
                        if (!fileToCheck.Exists) throw new FileNotFoundException("Can't find file to check", fileToCheck.FullName);

                        string actualChecksum = contribFileFunctions.GetChecksum(fileToCheck.FullName, Algorithm);
                        string expectedChecksum = checksumFile.OpenText().ReadToEnd();

                        if (actualChecksum != expectedChecksum) throw new Exception(string.Format("The checksum generated does not match the contents of the file's counterpart.  Actual [{0}] from [{1}] was expected to be [{2}]", actualChecksum, fileToCheck.FullName, expectedChecksum));
                    }
                }
            }
        }
    }
}
