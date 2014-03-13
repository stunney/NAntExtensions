/*
The MIT License (MIT)

Copyright (c) 2013-2014 Dave Tuchlinsky, Stephen Tunney, Canada (stephen.tunney@gmail.com)

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
    [TaskName("wix_light")]
    public class WiXLightTask : NAnt.Core.Tasks.ExecTask
    {
        #region Public Instance Properties

        [TaskAttribute("noTidy", Required = false)]
        [BooleanValidator()]
        public virtual bool NoTidy { get; set; }

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

        [TaskAttribute("extensionClassAssemblyFQCN", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ExtensionClassAssemblyFullyQualifiedClassName { get; set; }

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

        [TaskAttribute("allowIdenticalRows", Required = false)]
        public virtual bool AllowIdenticalRows { get; set; } //-ai

        [TaskAttribute("allowUnresolvedReferences", Required = false)]
        public virtual bool AllowUnresolvedReferences { get; set; } //-au

        [TaskAttribute("bindFilesIntoWixout", Required = false)]
        public virtual bool BindFilesIntoWixout { get; set; } //-bf (requires -xo)

        [TaskAttribute("binderPath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string BinderPath { get; set; } //-b <path>

        [TaskAttribute("out", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Out { get; set; } //-out <path>

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
                if (string.IsNullOrEmpty(base.FileName)) base.FileName = "light.exe";

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

                if (NoTidy) arguments += string.Format(@" -notidy");

                if (!string.IsNullOrEmpty(ExtensionClassAssemblyFullyQualifiedClassName))
                {
                    arguments += string.Format(stringFormat, @"-ext", ExtensionClassAssemblyFullyQualifiedClassName);
                }

                if (base.Verbose)
                {
                    arguments += string.Format(boolFormat, @"-v");
                }

                if (!string.IsNullOrEmpty(BinderPath)) arguments += string.Format(@" -b ""{0}""", BinderPath);

                arguments += string.Format(@" -out ""{0}""", Out);

                foreach (string objectFile in InElement.Items.AsIs)
                {
                    if (!File.Exists(objectFile)) throw new Exception(string.Format("File {0} not found.", objectFile));

                    arguments += string.Format(@" ""{0}""", objectFile);
                }

                base.CommandLineArguments = string.Format(@"{0}", arguments);

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
