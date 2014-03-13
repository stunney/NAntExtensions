using System;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;

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
                System.Security.Cryptography.X509Certificates.X509Certificate certificate = new System.Security.Cryptography.X509Certificates.X509Certificate(filePath);
                // An exception would be thrown if the file was not signed
                return true;
            }
            catch (Exception e) { }
            return false;
        }
    }
}
