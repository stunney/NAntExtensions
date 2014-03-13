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

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

using NAnt.Contrib.Tasks.Perforce;
using System.Text;

namespace NAnt.Extensions.Tasks.Perforce
{
    [TaskName("p4lock")]
    public class P4Lock : P4Base
    {
        #region Private Instance Fields

        private string _changelist;
        private string _file;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Changelist to place the opened files into.
        ///
        /// </summary>
        [TaskAttribute("changelist", Required = false)]
        public string Changelist
        {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// File Type settings.
        /// </summary>
        [TaskAttribute("file", Required = false)]
        public string File
        {
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of P4Base

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments
        {
            get { return getSpecificCommandArguments(); }
        }

        #endregion Override implementation of P4Base

        #region Protected Instance Methods

        protected string getSpecificCommandArguments()
        {
            StringBuilder arguments = new StringBuilder();
            arguments.Append("lock ");

            if (Changelist != null)
            {
                if (Changelist.ToLower() == "default")
                {
                    arguments.Append("-c default ");
                }
                else
                {
                    arguments.Append(string.Format("-c {0} ", NAnt.Contrib.Tasks.Perforce.Perforce.GetChangelistNumber(User, Client, Changelist, true)));
                }
            }

            if (File != null)
            {
                arguments.Append(string.Format(" {0} ", File));
            }

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
