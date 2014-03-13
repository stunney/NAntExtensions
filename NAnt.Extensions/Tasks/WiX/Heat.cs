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

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Extensions.Tasks.WiX
{
    public enum EHarvestType : int
    {
        dir = 0,
        file = 1,
        payload = 2,
        perf = 3,
        project = 4,
        reg = 5,
        website = 6,
    }

    [TaskName("wix_heat")]
    public class WiXHeatTask : NAnt.Core.Tasks.ExecTask
    {
        #region Public Instance Properties

        [TaskAttribute("allWarningsAsErrors", Required = false)]
        [BooleanValidator()]
        public virtual bool AllWarningsAsErrors { get; set; }

        [TaskAttribute("warningsAsErrors", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string WarningsAsErrors { get; set; }

        [TaskAttribute("wixvar", Required = false)]
        [BooleanValidator()]
        public virtual bool WixVar { get; set; }

        [TaskAttribute("var", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Var { get; set; }

        [TaskAttribute("template", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Template { get; set; }

        [TaskAttribute("xsltransform", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string TransformXSLTFilename { get; set; }

        [TaskAttribute("supressAllWarnings", Required = false)]
        public virtual bool SupressAllWarnings { get; set; }

        [TaskAttribute("supressWarnings", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string SupressWarning { get; set; }

        [TaskAttribute("svb6", Required = false)]
        [BooleanValidator()]
        public virtual bool SVB6 { get; set; }

        [TaskAttribute("suid", Required = false)]
        [BooleanValidator()]
        public virtual bool SUID { get; set; }

        [TaskAttribute("sreg", Required = false)]
        [BooleanValidator()]
        public virtual bool SREG { get; set; }

        [TaskAttribute("srd", Required = false)]
        [BooleanValidator()]
        public virtual bool SRD { get; set; }

        [TaskAttribute("sfrag", Required = false)]
        [BooleanValidator()]
        public virtual bool SFRAG { get; set; }

        [TaskAttribute("scom", Required = false)]
        [BooleanValidator()]
        public virtual bool SCOM { get; set; }

        [TaskAttribute("projectName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ProjectName { get; set; }

        [TaskAttribute("platform", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Platform { get; set; }

        [TaskAttribute("out", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Out { get; set; }

        [TaskAttribute("noLogo", Required = false)]
        [BooleanValidator()]
        public virtual bool NoLogo { get; set; }

        [TaskAttribute("keepEmptyDirectories", Required = false)]
        [BooleanValidator()]
        public virtual bool KeepEmptyDirectories { get; set; }

        [TaskAttribute("generateGuidsNow", Required = false)]
        [BooleanValidator()]
        public virtual bool GenerateGuidsNow { get; set; }

        [TaskAttribute("generate", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Generate { get; set; }

        [TaskAttribute("extensionClassAssemblyFQCN", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ExtensionClassAssemblyFullyQualifiedClassName { get; set; }

        [TaskAttribute("directoryName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string DirectoryName { get; set; }

        [TaskAttribute("directoryId", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string DirectoryId { get; set; }

        [TaskAttribute("configuration", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Configuration { get; set; }

        [TaskAttribute("componentGroupName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string ComponentGroupName { get; set; }

        [TaskAttribute("autoGenerateGuidsAtCompileTime", Required = false)]
        [BooleanValidator()]
        public virtual bool AutoGenerateGuidsAtCompileTime { get; set; }

        [TaskAttribute("harvestSource", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string HarvestSource { get; set; }

        protected EHarvestType _harvestType = EHarvestType.dir;
        [TaskAttribute("harvestType", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual EHarvestType HarvestType
        {
            get { return _harvestType; }
            set
            {
                if (!Enum.IsDefined(typeof(EHarvestType), value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid enumeration value for harvestType.", value));
                }

                _harvestType = value;
            }
        }

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

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask()
        {

            if (DebugTask && !System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

            Log(Level.Info, "Set Up");

            try
            {
                base.FileName = "heat.exe";

                const string boolFormat = @" {0}";
                const string stringFormat = @" {0} {1}";

                string arguments = string.Empty;
                if (AllWarningsAsErrors)
                {
                    arguments += string.Format(boolFormat, @"-wxall");
                }

                if (!string.IsNullOrEmpty(WarningsAsErrors))
                {
                    string[] warnings = WarningsAsErrors.Split(',');
                    foreach (string s in warnings)
                    {
                        arguments += string.Format(@" -wx{0}", s);
                    }
                }

                if (!string.IsNullOrEmpty(SupressWarning))
                {
                    string[] warnings = WarningsAsErrors.Split(',');
                    foreach (string s in warnings)
                    {
                        arguments += string.Format(@" -sw{0}", s);
                    }
                }

                if (WixVar)
                {
                    arguments += string.Format(boolFormat, @"-wixvar");
                }

                if (!string.IsNullOrEmpty(Var))
                {
                    arguments += string.Format(stringFormat, @"-var", Var);
                }

                if (base.Verbose)
                {
                    arguments += string.Format(boolFormat, @"-v");
                }

                if (!string.IsNullOrEmpty(Template))
                {
                    arguments += string.Format(stringFormat, @"-template", Template);
                }

                if (!string.IsNullOrEmpty(TransformXSLTFilename))
                {
                    arguments += string.Format(stringFormat, @"-t", TransformXSLTFilename);
                }

                if (SupressAllWarnings)
                {
                    arguments += string.Format(boolFormat, @"-swall");
                }

                if (SVB6)
                {
                    arguments += string.Format(boolFormat, @"-svb6");
                }

                if (SUID)
                {
                    arguments += string.Format(boolFormat, @"-suid");
                }

                if (SREG)
                {
                    arguments += string.Format(boolFormat, @"-sreg");
                }

                if (SRD)
                {
                    arguments += string.Format(boolFormat, @"-srd");
                }

                if (SFRAG)
                {
                    arguments += string.Format(boolFormat, @"-sfrag");
                }

                if (SCOM)
                {
                    arguments += string.Format(boolFormat, @"-scom");
                }

                if (!string.IsNullOrEmpty(ProjectName))
                {
                    arguments += string.Format(stringFormat, @"-projectname", ProjectName);
                }

                if (!string.IsNullOrEmpty(Platform))
                {
                    arguments += string.Format(stringFormat, @"-platform", Platform);
                }

                if (NoLogo)
                {
                    arguments += string.Format(boolFormat, @"-nologo");
                }

                if (KeepEmptyDirectories)
                {
                    arguments += string.Format(boolFormat, @"-ke");
                }

                if (GenerateGuidsNow)
                {
                    arguments += string.Format(boolFormat, @"-gg");
                }

                if (!string.IsNullOrEmpty(Generate))
                {
                    arguments += string.Format(stringFormat, @"-generate", Generate);
                }

                if (!string.IsNullOrEmpty(ExtensionClassAssemblyFullyQualifiedClassName))
                {
                    arguments += string.Format(stringFormat, @"-ext", ExtensionClassAssemblyFullyQualifiedClassName);
                }

                if (!string.IsNullOrEmpty(DirectoryName))
                {
                    arguments += string.Format(stringFormat, @"-dr", DirectoryName);
                }

                if (!string.IsNullOrEmpty(DirectoryId))
                {
                    arguments += string.Format(stringFormat, @"-directoryid", DirectoryId);
                }

                if (!string.IsNullOrEmpty(Configuration))
                {
                    arguments += string.Format(stringFormat, @"-configuration", Configuration);
                }

                if (!string.IsNullOrEmpty(ComponentGroupName))
                {
                    arguments += string.Format(stringFormat, @"-cg", ComponentGroupName);
                }

                if (AutoGenerateGuidsAtCompileTime)
                {
                    arguments += string.Format(boolFormat, @"-ag");
                }

                base.CommandLineArguments = string.Format(@"{0} ""{1}""{2} -out {3}", HarvestType, HarvestSource, arguments, Out);

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