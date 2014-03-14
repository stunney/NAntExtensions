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
using NAnt.Core;
using NAnt.Core.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NAnt.Extensions.Tasks.Path
{
    [TaskName("delete-oldest-subdirectories")]
    public class DirectoryCleanupTask : NAnt.Core.Task
    {
        class DateCompareFileSystemInfo : System.Collections.Generic.IComparer<FileSystemInfo>
        {
            /// <summary>
            /// Compare the last dates of the File infos
            /// </summary>
            /// <param name="fi1">First FileInfo to check</param>
            /// <param name="fi2">Second FileInfo to check</param>
            /// <returns></returns>
            public int Compare(FileSystemInfo fi1, FileSystemInfo fi2)
            {
                int result;
                if (fi1.CreationTime == fi2.CreationTime)
                {
                    result = 0;
                }
                else if (fi1.CreationTime < fi2.CreationTime)
                {
                    result = 1;
                }
                else
                {
                    result = -1;
                }

                return result;
            }
        }


        #region Public Instance Properties

        [TaskAttribute("maxChildrenToKeep", Required = true)]
        public virtual int MaxToKeep { get; set; }

        [TaskAttribute("directory", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual DirectoryInfo DirectoryName { get; set; }

        [TaskAttribute("debug", Required = false)]
        [BooleanValidator()]
        public virtual bool DebugTask { get; set; }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask()
        {

            if (DebugTask && !System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

            Log(Level.Info, "Set Up {0} {1}.", DirectoryName, MaxToKeep);

            try
            {
                DirectoryInfo[] childFolders = DirectoryName.GetDirectories();
                DateCompareFileSystemInfo comparer = new DateCompareFileSystemInfo();

                Array.Sort(childFolders, comparer);

                if (childFolders.Length > MaxToKeep)
                {
                    for (int i = MaxToKeep; i < childFolders.Length; i++)
                    {
                        System.Console.WriteLine("Deleting {0}", childFolders[i].FullName);
                        childFolders[i].Delete(true);
                    }
                }

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