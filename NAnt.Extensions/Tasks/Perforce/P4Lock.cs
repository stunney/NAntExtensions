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
