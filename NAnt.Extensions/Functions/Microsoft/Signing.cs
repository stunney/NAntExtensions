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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Extensions.Tasks.Microsoft;

namespace NAnt.Extensions.Functions.Microsoft
{
    /// <summary>
    /// 
    /// </summary>
    [FunctionSet("signtool", "Signtool")]
    public class SigningFunctions : NAnt.Core.FunctionSetBase
    {
        public SigningFunctions(Project project, PropertyDictionary dictionary)
            : base(project, dictionary)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [Function("is-signed")]
        public static bool IsSigned(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            try
            {
                Process myProcess = new Process();
                try
                {
                    StringBuilder args = new StringBuilder(@"verify /pa ");
                    args.Append(@"/v ");
                    args.Append(filePath);

                    myProcess.StartInfo.FileName = SignTask.DefaultSignToolPath + @"\signtool.exe";
                    myProcess.StartInfo.Arguments = args.ToString();
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.RedirectStandardOutput = true;

                    int exitCode = 1;
                    myProcess.Start();
                    myProcess.WaitForExit();
                    exitCode = myProcess.ExitCode;
                    myProcess.Close();

                    // Check for errors
                    if (exitCode == 2)
                        throw new BuildException("Failed Execution has completed with warnings.");
                    else if (exitCode != 0)
                        throw new BuildException("Failed to sign. Error " + exitCode);
                }
                catch (Win32Exception e)
                {
                    throw new BuildException("Unable to find signtool.exe in " + SignTask.DefaultSignToolPath, e);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
