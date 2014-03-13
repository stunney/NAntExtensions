/*
The MIT License (MIT)

Copyright (c) 2013-2014 Stephen Tunney, Canada (stephen.tunney@gmail.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Extensions.Tasks.WiX
{
    [TaskName("wix_pyro")]
    public class WiXPyroTask : NAnt.Core.Tasks.ExecTask
    {
        #region Public Instance Properties

        [TaskAttribute("noLogo", Required = false)]
        [BooleanValidator()]
        public virtual bool NoLogo { get; set; }

        [TaskAttribute("allWarningsAsErrors", Required = false)]
        [BooleanValidator()]
        public virtual bool AllWarningsAsErrors { get; set; }

        [TaskAttribute("warningsAsErrors", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string WarningsAsErrors { get; set; }

        [TaskAttribute("supressAllWarnings", Required = false)]
        public virtual bool SupressAllWarnings { get; set; }

        [TaskAttribute("supressWarnings", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string SupressWarning { get; set; }

        [TaskAttribute("out", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Out { get; set; }

        [TaskAttribute("extensionClassAssemblyFQCN", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ExtensionClassAssemblyFullyQualifiedClassName { get; set; }

        [TaskAttribute("noTidy", Required = false)]
        [BooleanValidator()]
        public virtual bool NoTidy { get; set; }

        [TaskAttribute("program", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public new string FileName
        {
            get { return base.FileName; }
            set { base.FileName = value; }
        }

        [TaskAttribute("debug", Required = false)]
        [BooleanValidator()]
        public virtual bool DebugTask { get; set; }

        //**************************************************************
        [TaskAttribute("supressAssemblies", Required = false)]
        [BooleanValidator()]
        public virtual bool SupressAssemblies { get; set; }

        [TaskAttribute("supressFiles", Required = false)]
        [BooleanValidator()]
        public virtual bool SupressFiles { get; set; }

        [TaskAttribute("supressWiXPDBOutput", Required = false)]
        [BooleanValidator()]
        public virtual bool SupressWiXPDBOutput { get; set; }

        [TaskAttribute("supressFileInfo", Required = false)]
        [BooleanValidator()]
        public virtual bool SupressFileInfo { get; set; }

        [TaskAttribute("reuseCAB", Required = false)]
        [BooleanValidator()]
        public virtual bool ResueCABFiles { get; set; } //-reusecab

        [TaskAttribute("updateFileVersionEntries", Required = false)]
        [BooleanValidator()]
        public virtual bool UpdateFileVersionEntries { get; set; } //-fv

        //public override poopie() { return true; } //101!!

        [TaskAttribute("createBinayDeltaPatch", Required = false)]
        [BooleanValidator()]
        public virtual bool CreateBinayDeltaPatch { get; set; } //-delta

        [TaskAttribute("newBindpathForTargetInput", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string NewBindPathForTarget { get; set; } //-bt <path>

        [TaskAttribute("newBindpathForUpgradeInput", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string NewBindPathForUpgrade { get; set; } //-bu <path>

        [TaskAttribute("cabCachePath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual DirectoryInfo CabCachePath { get; set; } //-cc <path>

        [TaskAttribute("pdbOutputFilename", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string PDBOutputFilename { get; set; } //-pdbout <output.wixpdb>

        [TaskAttribute("inputFile", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual FileInfo InputFilename { get; set; }

        [BuildElement("in")]
        public virtual NAnt.Core.Tasks.InElement InElement { get; set; }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask()
        {

            if (DebugTask && !System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

            Log(Level.Info, "Set Up");

            try
            {
                if (string.IsNullOrEmpty(base.FileName)) base.FileName = "pyro.exe";

                const string boolFormat = @" {0}";
                const string stringFormat = @" {0} {1}";

                string arguments = string.Empty;

                if (NoLogo)
                {
                    arguments += string.Format(boolFormat, @"-nologo");
                }

                if (AllWarningsAsErrors)
                {
                    arguments += string.Format(boolFormat, @"-wxall");
                }
                else if (!string.IsNullOrEmpty(WarningsAsErrors))
                {
                    string[] warnings = WarningsAsErrors.Split(',');
                    foreach (string s in warnings)
                    {
                        arguments += string.Format(@" -wx{0}", s);
                    }
                }

                if (SupressAllWarnings)
                {
                    arguments += string.Format(boolFormat, @"-swall");
                }
                else if (!string.IsNullOrEmpty(SupressWarning))
                {
                    string[] warnings = WarningsAsErrors.Split(',');
                    foreach (string s in warnings)
                    {
                        arguments += string.Format(@" -sw{0}", s);
                    }
                }

                if (NoTidy) arguments += string.Format(boolFormat, @"-notidy");

                if (!string.IsNullOrEmpty(ExtensionClassAssemblyFullyQualifiedClassName))
                {
                    arguments += string.Format(@" -ext ""{0}""", ExtensionClassAssemblyFullyQualifiedClassName);
                }

                if (base.Verbose)
                {
                    arguments += string.Format(boolFormat, @"-v");
                }

                if (SupressAssemblies) arguments += string.Format(boolFormat, @"-sa");
                if (SupressFiles) arguments += string.Format(boolFormat, @"-sf");
                if (SupressFileInfo) arguments += string.Format(boolFormat, @"-sh");
                if (SupressWiXPDBOutput) arguments += string.Format(boolFormat, @"-spdb");
                if (ResueCABFiles) arguments += string.Format(boolFormat, @"-reusecab");
                if (UpdateFileVersionEntries) arguments += string.Format(boolFormat, @"-fv");
                if (CreateBinayDeltaPatch) arguments += string.Format(boolFormat, @"-delta");

                if (!string.IsNullOrEmpty(NewBindPathForTarget)) arguments += string.Format(@" -bt ""{0}""", NewBindPathForTarget);
                if (!string.IsNullOrEmpty(NewBindPathForUpgrade)) arguments += string.Format(@" -bu ""{0}""", NewBindPathForUpgrade);

                if (null != CabCachePath && !CabCachePath.Exists) arguments += string.Format(@" -cc ""{0}""", CabCachePath);

                if (!string.IsNullOrEmpty(PDBOutputFilename)) arguments += string.Format(@" -pdbout ""{0}""", PDBOutputFilename);

                if (null == InElement || 0 == InElement.Items.AsIs.Count) { Log(Level.Info, @"Empty items!"); return; }

                foreach (string baselineArg in InElement.Items.AsIs)
                {
                    string[] parts = baselineArg.Split(';');
                    arguments += string.Format(@" -t {0} {1}", parts[0], parts[1]);
                }

                base.CommandLineArguments = string.Format(@"""{0}"" {2} -out ""{1}""", InputFilename, Out, arguments);

                Log(Level.Info, base.CommandLineArguments);

                base.ExecuteTask();
                Log(Level.Info, "Done!");
            }
            catch (Exception ex)
            {
                Log(Level.Info, "Exception handled");

                Exception e = ex;
                while (null != e)
                {
                    Log(Level.Error, ex.Message);
                    Log(Level.Error, ex.StackTrace);

                    e = e.InnerException;
                }

                throw;
            }
        }
        #endregion Override implementation of Task
    }
}