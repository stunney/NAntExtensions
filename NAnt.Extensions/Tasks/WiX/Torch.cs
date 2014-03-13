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
    [TaskName("wix_torch")]
    public class WiXTorchTask : NAnt.Core.Tasks.ExecTask
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

        [TaskAttribute("targetInput", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual string TargetInputMSIFilename { get; set; }

        [TaskAttribute("updatedInput", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual string UpdatedInputMSIFilename { get; set; }

        [TaskAttribute("adminImage", Required = false)]
        [BooleanValidator()]
        public virtual bool AdminImage { get; set; }

        [TaskAttribute("extractBinariesTo", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ExtractedBinariesPath { get; set; }

        [TaskAttribute("transformationType", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string TransformationType { get; set; }

        [TaskAttribute("validationFlags", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ValidationFlags { get; set; }

        [TaskAttribute("validationErrorsToSupress", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ValidationErrorsToSupress { get; set; }

        [TaskAttribute("noTidy", Required = false)]
        [BooleanValidator()]
        public virtual bool NoTidy { get; set; }

        [TaskAttribute("preserveUnmodifiedContent", Required = false)]
        [BooleanValidator()]
        public virtual bool PreserveUnmodifiedContent { get; set; }

        [TaskAttribute("showPedanticMessages", Required = false)]
        [BooleanValidator()]
        public virtual bool ShowPedanticMessages { get; set; }

        [TaskAttribute("wixFormatInput", Required = false)]
        [BooleanValidator()]
        public virtual bool WiXInputFormat { get; set; }

        [TaskAttribute("wixFormatOutput", Required = false)]
        [BooleanValidator()]
        public virtual bool WiXOutputFormat { get; set; }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask()
        {

            if (DebugTask && !System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

            Log(Level.Info, "Set Up");

            try
            {
                if (string.IsNullOrEmpty(base.FileName)) base.FileName = "torch.exe";

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

                if (AdminImage) arguments += string.Format(@" -a");
                if (NoTidy) arguments += string.Format(@" -notidy");
                if (PreserveUnmodifiedContent) arguments += string.Format(@" -p");
                if (ShowPedanticMessages) arguments += string.Format(@" -pedantic");
                if (WiXInputFormat) arguments += string.Format(@" -xi");
                if (WiXOutputFormat) arguments += string.Format(@" -xo");

                if (!string.IsNullOrEmpty(ExtractedBinariesPath))
                {
                    arguments += string.Format(stringFormat, @"-x ""{0}""", ExtractedBinariesPath);
                }

                if (!string.IsNullOrEmpty(ValidationFlags))
                {
                    string[] flags = ValidationFlags.Split(',');
                    foreach (string f in flags)
                    {
                        switch (f.ToLower())
                        {
                            case "g":
                            case "l":
                            case "r":
                            case "s":
                            case "t":
                            case "u":
                            case "v":
                            case "w":
                            case "x":
                            case "y":
                            case "z":
                                break;
                            default:
                                throw new Exception(string.Format("Bad validation flag value of '{0}' in '{1}'.  Expected any of g,l,r,s,t,u,v,w,x,y,z!  Split on comma is done!", f, ValidationFlags));
                        }
                        arguments += string.Format(stringFormat, @"-val", f);
                    }
                }

                if (!string.IsNullOrEmpty(ValidationErrorsToSupress))
                {
                    string[] errors = ValidationErrorsToSupress.Split(',');
                    foreach (string e in errors)
                    {
                        switch (e.ToLower())
                        {
                            case "a":
                            case "b":
                            case "c":
                            case "d":
                            case "e":
                            case "f":
                                break;
                            default:
                                throw new Exception(string.Format("Bad validationErrorToSupress value of '{0}' in '{1}'.  Expected a through f!  Split on comma is done!", e, ValidationErrorsToSupress));
                        }
                        arguments += string.Format(stringFormat, @"-serr", e);
                    }
                }

                if (!string.IsNullOrEmpty(TransformationType))
                {
                    switch (TransformationType.ToLower())
                    {
                        case "language":
                        case "instance":
                        case "patch":
                            break;
                        default:
                            throw new Exception("Bad transformationType.  language || instance || patch !!!");
                    };
                    arguments += string.Format(stringFormat, @"-t", TransformationType);
                }

                if (!string.IsNullOrEmpty(ExtensionClassAssemblyFullyQualifiedClassName))
                {
                    arguments += string.Format(stringFormat, @"-ext", ExtensionClassAssemblyFullyQualifiedClassName);
                }

                if (base.Verbose)
                {
                    arguments += string.Format(boolFormat, @"-v");
                }

                FileInfo targetMSIFile = new FileInfo(TargetInputMSIFilename);
                FileInfo updatedMSIFile = new FileInfo(UpdatedInputMSIFilename);

                if (!targetMSIFile.Exists) throw new FileNotFoundException(string.Format("Can't find target (RTM) MSI file at [{0}].", targetMSIFile.FullName));
                if (!updatedMSIFile.Exists) throw new FileNotFoundException(string.Format("Can't find updated (Upgrade) MSI file at [{0}].", updatedMSIFile.FullName));

                base.CommandLineArguments = string.Format(@"{0} ""{1}"" ""{2}"" -out {3}", arguments, targetMSIFile.FullName, updatedMSIFile.FullName, Out);

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
