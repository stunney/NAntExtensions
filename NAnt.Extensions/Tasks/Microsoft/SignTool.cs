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
using System.Threading;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAnt.Extensions.Functions.Microsoft;

namespace NAnt.Extensions.Tasks.Microsoft
{
    [TaskName("signtool")]
    public class SignTask : Task
    {
        #region Public Instance Properties
        [TaskAttribute("filename")]
        public string FileName { get; set; }

        [TaskAttribute("description")]
        [StringValidatorAttribute(AllowEmpty = false)]
        public string Description { get; set; }

        [TaskAttribute("pagehashes")]
        [BooleanValidator()]
        public bool PageHashes { get; set; }

        [TaskAttribute("retries")]
        [Int32Validator(MinValue = 0)]
        public int Retries { get; set; }

        [TaskAttribute("signtoolpath")]
        [StringValidatorAttribute(AllowEmpty = false)]
        public string SignToolPath { get; set; }

        [TaskAttribute("additionalcertificate")]
        [StringValidatorAttribute(AllowEmpty = false)]
        public string AdditionalCertificate { get; set; }

        [TaskAttribute("subjectname")]
        [StringValidatorAttribute(AllowEmpty = false)]
        public string SubjectName { get; set; }

        [TaskAttribute("url")]
        [StringValidatorAttribute(AllowEmpty = false)]
        public string URL { get; set; }

        [TaskAttribute("timestampurl")]
        [StringValidatorAttribute(AllowEmpty = false)]
        public string TimestampURL { get; set; }

        [BuildElement("items")]
        public FileSet Items { get; set; }
        #endregion Public Instance Properties

        #region Default Constructor
        public SignTask()
        {
            Retries = 2;
            SignToolPath = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin";
            TimestampURL = @"http://timestamp.verisign.com/scripts/timstamp.dll";
            SubjectName = @"MyCompany.com";
            AdditionalCertificate = @"VeriSign Class 3 Public Primary Certification Authority - G5.cer";
            PageHashes = false;
        }
        #endregion Default Constructor

        #region Protected Instance Methods
        protected void SignFiles(string files)
        {
            Process myProcess = new Process();
            try
            {
                // Build the argument string
                StringBuilder args = new StringBuilder(@"sign /a ");
                args.Append(@"/n """ + SubjectName + @""" ");
                args.Append(@"/du """ + URL + @""" ");
                args.Append(@"/t """ + TimestampURL + @""" ");
                // Optional arguments
                if (Verbose) args.Append(@"/v ");
                if (PageHashes) args.Append(@"/ph /ac """ + AdditionalCertificate + @""" ");
                if (!String.IsNullOrEmpty(Description)) args.Append(@"/d """ + Description + @""" ");
                // File(s) to be signed
                args.Append(files.ToString());

                if (Verbose)
                    Log(Level.Info, "Args: " + args.ToString());

                myProcess.StartInfo.FileName = SignToolPath + @"\signtool.exe";
                myProcess.StartInfo.Arguments = args.ToString();
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardOutput = true;

                int exitCode = 1; // Do not assume success to enter for loop
                int maxAttempts = Retries + 1;
                for (int attempt = 1; attempt <= maxAttempts && exitCode != 0; attempt++)
                {
                    if (Verbose)
                        Log(Level.Info, "Signing attempt {0}", attempt);
                    if (Retries > 0 && attempt == maxAttempts)
                    {
                        Log(Level.Warning, "Last attempt. Sleeping for a few seconds to possibly let Verisign wake up.");
                        Thread.Sleep(3000);
                    }

                    myProcess.Start();
                    myProcess.WaitForExit();

                    // Read the standard output of the spawned process. 
                    StreamReader myStreamReader = myProcess.StandardOutput;
                    while (!myStreamReader.EndOfStream)
                    {
                        Log(Level.Info, myStreamReader.ReadLine());
                    }

                    exitCode = myProcess.ExitCode;
                    myProcess.Close();
                }

                // Check for errors
                if (exitCode == 2)
                    throw new BuildException("Failed to communicate with timestamp server.", Location);
                else if (exitCode != 0)
                    throw new BuildException("Failed to sign. Error " + exitCode, Location);
            }
            catch (Win32Exception e)
            {
                throw new BuildException("Unable to find signtool.exe in " + SignToolPath, Location, e);
            }
        }
        #endregion Protected Instance Methods

        #region Override implementation of Task
        protected override void ExecuteTask()
        {
            // Validate the parameters
            if (string.IsNullOrEmpty(FileName) && (Items == null || Items.FileNames.Count == 0))
                throw new BuildException("Must specify @filename or <items>.", Location);
            if (!string.IsNullOrEmpty(FileName) && Items != null && Items.FileNames.Count > 0)
                throw new BuildException("Cannot specify @filename and <items>.", Location);

            if (Verbose)
            {
                Log(Level.Info, "Description: {0}", Description);
                Log(Level.Info, "Page Hashes: {0}", PageHashes);
                Log(Level.Info, "Retries: {0}", Retries);
                Log(Level.Info, "SignTool Path: {0}", SignToolPath);
                Log(Level.Info, "Additional Certificate: {0}", AdditionalCertificate);
                Log(Level.Info, "Subject Name: {0}", SubjectName);
                Log(Level.Info, "Timestamp URL: {0}", TimestampURL);
                Log(Level.Info, "URL: {0}", URL);
                Log(Level.Info, "FileName: {0}", FileName);
            }

            StringBuilder filenames = new StringBuilder();
            filenames.Capacity = 1000;
            if (Items != null)
            {
                // Construct the file list
                int count = 0;
                foreach (string file in Items.FileNames)
                {
                    if (!SigningFunctions.IsSigned(file))
                    {
                        filenames.Append(@"""");
                        filenames.Append(file);
                        filenames.Append(@""" ");
                        count++;
                    }
                    // Limit the number of files being signed in any single call to SignTool
                    if (filenames.Length > 1000)
                    {
                        SignFiles(filenames.ToString());
                        filenames.Remove(0, filenames.Length);
                    }
                }
                // Sign any remaining files
                if (filenames.Length > 0)
                    SignFiles(filenames.ToString());
                Log(Level.Info, "Signed {0} files", count);
            }
            else
            {
                if (!SigningFunctions.IsSigned(FileName))
                {
                    // A single file is to be signed
                    filenames.Append(@"""");
                    filenames.Append(FileName);
                    filenames.Append(@""" ");
                    SignFiles(filenames.ToString());
                }
                else
                    Log(Level.Info, "Skipping {0}. It is already signed", FileName);
            }
        }
        #endregion Override implementation of Task
    }
}