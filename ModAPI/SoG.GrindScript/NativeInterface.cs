using System;

namespace SoG.GrindScript
{
    public static class NativeInterface
    {
        public static int Initialize(string argument)
        {
            Console.WriteLine("Hello world from NativeInterface::Initialize");
            Console.Error.WriteLine("Hello world from NativeInterface::Initialize via stderr");
            GrindScript.InitializeInLauncher();
            return 1;
        }
    }
}
