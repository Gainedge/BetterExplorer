using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AlexPilotti.FTPS.Client
{
    public static class PathCheck
    {
        static char replacementChar = '_';

        /// <summary>
        /// Replaces all invalid characters found in the provided name
        /// </summary>
        /// <param name="fileName">A file name without directory information</param>
        /// <returns></returns>
        public static string GetValidLocalFileName(string fileName)
        {
            return ReplaceAllChars(fileName, Path.GetInvalidFileNameChars(), replacementChar);
        }

        private static string ReplaceAllChars(string str, char[] oldChars, char newChar)
        {
            StringBuilder sb = new StringBuilder(str);
            foreach (char c in oldChars)
                sb.Replace(c, newChar);
            return sb.ToString();
        }
    }
}
