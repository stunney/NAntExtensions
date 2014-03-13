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
using System.Text;
using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Extensions.Functions
{
    /// <summary>
    /// 
    /// </summary>
    [FunctionSet("regex", "Regex")]
    public class RegexFunctions : FunctionSetBase
    {
        public RegexFunctions(Project project, PropertyDictionary properties)
            : base(project, properties) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regexPattern"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        [Function("searchAndReplaceRegex")]
        public static string SearchAndReplace(string input, string regexPattern, string replacement)
        {
            RegexOptions options = RegexOptions.Compiled | RegexOptions.Multiline;// | RegexOptions.IgnoreCase | RegexOptions.Compiled;RegexOptions.None; //| 

            return Regex.Replace(input, regexPattern, replacement, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Function("cleanSQLFileForCustomAction")]
        public static string CleanSQLFileForCustomAction(string input)
        {
            //Remove comments
            input = SearchAndReplace(input, @"\-\-.*", string.Empty);
            input = SearchAndReplace(input, @"(.*)\/\*(.*)\*\/(.*)", @"$1$3");

            //Newlines here are intentional!
            //Get rid of extra tabs and empty lines
            input = SearchAndReplace(input, @"\r?\n\r?\n", @"
");
            input = SearchAndReplace(input, @"\r?\n\r?\n", @"
");
            input = SearchAndReplace(input, @"\r?\n\r?\n", @"
");
            input = SearchAndReplace(input, @"\r?\n\r?\n", @"
");
            input = SearchAndReplace(input, @"\r?\n\r?\n", @"
");
            input = SearchAndReplace(input, @"\n\n\n", @"");
            input = SearchAndReplace(input, @"GO\n", @"");
            input = SearchAndReplace(input, @"GO", @"");
            input = SearchAndReplace(input, @"\r?\nvalues", @"values");
            input = SearchAndReplace(input, @"values\r?\n", @"values");
            input = SearchAndReplace(input, @"\r?\n,", @",");
            input = SearchAndReplace(input, @",\r?\n", @",");

            return input;
        }
    }
}