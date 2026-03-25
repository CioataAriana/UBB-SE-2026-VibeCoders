using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MovieShop
{
    internal static class Helpers
    {
        public static string GetExecutionDirectory() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        public static string GetProjectDirectory() => Regex.Replace(GetExecutionDirectory(), @"\\bin.*$", "");
    }
}
