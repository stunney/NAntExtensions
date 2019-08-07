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
using System.Linq;
using System.Management.Automation;

namespace NAnt.Extensions.Functions.Microsoft
{
    [FunctionSet("signtool", "Signtool")]
    public class SigningFunctions : FunctionSetBase
    {
        public SigningFunctions(Project project, PropertyDictionary dictionary): base(project, dictionary)
        {
        }

        [Function("is-signed")]
        public static bool IsSigned(string filePath)
        {
            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddCommand("Get-AuthenticodeSignature", true);
                ps.AddParameter("FilePath", filePath);
                return ((Signature)ps.Invoke().First().BaseObject).Status.Equals(SignatureStatus.Valid);
            }
        }
    }
}
