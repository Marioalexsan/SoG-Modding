using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SoG.GrindScriptLauncher
{
    class Program
    {
        static Assembly SoG;
        static MethodInfo SoGMain;

        static Assembly GrindScript;
        static MethodInfo GSInit;

        static void LogErrorAndQuit(string error)
        {
            Console.WriteLine(error);
            Console.WriteLine("Hit Enter to exit.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        static void LaunchGrindScript()
        {
            try
            {
                GSInit.Invoke(null, new object[0]);
            }
            catch (Exception e)
            {
                LogErrorAndQuit("Exception during GrindScript Init call: " + e);
            }
        }

        static void LaunchSoG()
        {
            try
            {
                SoGMain.Invoke(null, new object[] { new string[0] });
            }
            catch (Exception e)
            {
                LogErrorAndQuit("Exception during SoG Main call: " + e);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Loading Assemblies");

                SoG = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\Secrets Of Grindea.exe");
                GrindScript = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\GrindScript.dll");

                SoGMain = SoG.DefinedTypes.First(t => t.FullName == "SoG.Program").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
                GSInit = GrindScript.DefinedTypes.First(t => t.FullName == "SoG.GrindScript.GrindScript").GetMethod("InitializeInLauncher", BindingFlags.Static | BindingFlags.Public);

                Console.WriteLine("Initializing GrindScript");
                LaunchGrindScript();

                Console.WriteLine("Launching SoG");
                new Thread(LaunchSoG).Start();
            }
            catch (Exception e)
            {
                LogErrorAndQuit("Exception during Launcher execution: " + e);
            }
        }
    }
}