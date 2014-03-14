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
using NAnt.Core.Types;
using NAnt.Extensions.Functions.Microsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NAnt.Extensions.Tasks.Microsoft
{
    [TaskName("unsign")]
    public class UnsignFiles : Task
    {
        [BuildElement("items")]
        public FileSet Items { get; set; }

        [System.Runtime.InteropServices.DllImport("Imagehlp.dll")]
        private static extern bool ImageRemoveCertificate(IntPtr handle, int index);

        protected virtual void UnsignFile(FileInfo file)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite))
            {
                ImageRemoveCertificate(fs.SafeFileHandle.DangerousGetHandle(), 0);
                fs.Close();
            }
        }

        protected override void ExecuteTask()
        {
            foreach (string file in Items.FileNames)
            {
                if (SigningFunctions.IsSigned(file))
                {
                    UnsignFile(new FileInfo(file));
                }                
            }
        }
    }
}