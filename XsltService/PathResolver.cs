using System;
using System.IO;
using System.Reflection;

namespace XsltService
{
    /// <summary>
    /// Helpful class for handling executing paths.
    /// </summary>
    public static class PathResolver
    {
        /// <summary>
        /// Gets service binary path.
        /// </summary>
        /// <value> Service binary path. </value>
        public static string ExecutablePath
        {
            get
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                string executablePath = new FileInfo(new Uri(currentAssembly.CodeBase).AbsolutePath).Directory?.FullName;

                return executablePath;
            }
        }
    }
}
