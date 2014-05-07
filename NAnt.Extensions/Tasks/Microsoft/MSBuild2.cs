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
using NAnt.Core.Tasks;
using NAnt.Core.Types;

using NAnt.Contrib.Tasks;

namespace NAnt.Extensions.Tasks.Microsoft
{
    public class MultiInElement : NAnt.Core.Element
    {
        #region Private Instance Fields

        private FileSet[] _items;

        #endregion Private Instance Fields

        #region Public Instance Properties

        [BuildElementArray("items")]
        public FileSet[] Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public int Count { get { return _items.Length; } }

        public FileSet this[int _index] { get { return _items[_index]; } }

        #endregion Public Instance Properties
    }

    [TaskName("msbuild2")] //Called this to not conflict with the existing msbuild task in NAntContrib.
    public class CustomMSBuildTask : MsbuildTask
    {
        #region Private Instance Fields

        private string _platform;
        private string _configuration;
        private bool _multiThreaded = false;
        private string _errorPropertyName;
        protected bool _debugTask = false;
        private MultiInElement _inElement;

        #endregion Private Instance Fields

        public CustomMSBuildTask()
            : base()
        {
            base.ExeName = "msbuild";
        }

        #region Public Instance Properties

        [TaskAttribute("platform", Required = true)]
        public string Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }

        [BuildElement("in")]
        public MultiInElement InElement
        {
            get { return _inElement; }
            set { _inElement = value; }
        }

        [TaskAttribute("configuration", Required = true)]
        public string Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        [TaskAttribute("multithreadedbuild", Required = true)]
        public bool MultiThreadedBuild
        {
            get { return _multiThreaded; }
            set { _multiThreaded = value; }
        }

        [TaskAttribute("errorpropertyname", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ErrorPropertyName
        {
            get { return _errorPropertyName; }
            set
            {
                if (!((Element)this).Properties.Contains(value)) ((Element)this).Properties.Add(value, null);
                if (((Element)this).Properties.IsReadOnlyProperty(value))
                {
                    throw new BuildException("Property is readonly! :" + value, Location);
                }
                _errorPropertyName = value;
            }
        }

        [TaskAttribute("msbuildpath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public virtual string MSBuildPath
        {
            get
            {
                return base.ExeName;
            }
            set
            {
                if (!value.ToLower().EndsWith("msbuild.exe"))
                {
                    throw new ArgumentException(@"msbuildpath required that ""msbuild.exe"" be at the end of the path specified.");
                }

                base.ExeName = value;
            }
        }

        [TaskAttribute("debug", Required = false)]
        [BooleanValidator()]
        public bool DebugTask
        {
            get { return _debugTask; }
            set { _debugTask = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask()
        {

            if (DebugTask && !System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

            string combination = string.Format("{0}[{1}|{2}]@{3};", ProjectFile, Configuration, Platform, Target);

            Log(Level.Info, string.Format("Calling msbuild {0}", combination));

            if (MultiThreadedBuild) base.Arguments.Add(new Argument("/m"));

            base.Properties.Add(new PropertyTask() { PropertyName = "Configuration", Value = Configuration });
            base.Properties.Add(new PropertyTask() { PropertyName = "Platform", Value = _platform });
            base.NoAutoResponse = false;

            Log(Level.Info, "Set Up");

            try
            {
                if (null != _inElement && 0 < _inElement.Count && 0 < _inElement[0].FileNames.Count)
                {
                    for (int idx = 0; idx < _inElement.Count; idx++)
                    {
                        FileSet fs = _inElement[idx];
                        for (int idx2 = 0; idx2 < fs.FileNames.Count; idx2++)
                        {
                            FileInfo slnFile = new FileInfo(fs.FileNames[idx2]);
                            Log(Level.Info, string.Format("Executing against {0}.", slnFile.FullName));
                            base.ProjectFile = slnFile;
                            base.ExecuteTask();
                        }
                    }
                }
                else
                {
                    //Just assume that the ProjectFile was set properly.
                    base.ExecuteTask();
                }
                Log(Level.Info, "Done!");
            }
            catch (Exception ex)
            {
                Log(Level.Info, "Exception handled");
                if (Properties.Contains(ErrorPropertyName))
                {
                    if (string.IsNullOrEmpty(((Element)this).Properties[ErrorPropertyName])) ((Element)this).Properties[ErrorPropertyName] = string.Empty;

                    ((Element)this).Properties[ErrorPropertyName] += combination;
                }

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