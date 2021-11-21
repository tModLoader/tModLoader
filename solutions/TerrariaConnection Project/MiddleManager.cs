using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TerrariaTranslator
{
    class Program
    {
        static void Main(string[] Args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            var info = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = "TerrariaTranslator.dll",
				WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(info);

            Environment.Exit(0);
        }
    }
}
