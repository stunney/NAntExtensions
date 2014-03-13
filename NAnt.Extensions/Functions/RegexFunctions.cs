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