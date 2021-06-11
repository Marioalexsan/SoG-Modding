using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SoG.ModLauncher
{
    class SuspendedInjection
    {
        private static byte leByte(uint number, int index)
        {
            return (byte)(number >> ((3 - index) * 8));
        }

        private static byte leByte(int number, int index)
        {
            return (byte)(number >> ((3 - index) * 8));
        }

        private static byte leByte(IntPtr number, int index)
        {
            return (byte)((int)number >> ((3 - index) * 8));
        }

        private string _path;
        private IntPtr _hProcess;
        private IntPtr _hThread;

        public SuspendedInjection(string path)
        {
            _path = path;
        }

        public void BeginProcess()
        {
            StartSuspended();
            Inject();
            Resume();
        }

        private void StartSuspended()
        {
            WinApi.StartupInfo sInfo = new WinApi.StartupInfo();
            WinApi.ProcessInformation pInfo = new WinApi.ProcessInformation();
            WinApi.CreateProcessA(_path, null, IntPtr.Zero, IntPtr.Zero, false, WinApi.CreationFlags.CreateNewConsole | WinApi.CreationFlags.CreateSuspended, IntPtr.Zero, null, ref sInfo, ref pInfo);

            _hProcess = pInfo.hProcess;
            _hThread = pInfo.hThread;
        }

        private void Inject()
        {
            WinApi.Wow64Context threadContext = new WinApi.Wow64Context();
            threadContext.ContextFlags = (uint)WinApi.ContextFlags.Context_All;

            WinApi.GetThreadContext(_hThread, ref threadContext);

            uint oldEip = threadContext.Eip;
            IntPtr newEntryPoint = WinApi.VirtualAllocEx(_hProcess, IntPtr.Zero, 1024, WinApi.AllocationType.Commit, WinApi.MemoryProtection.ExecuteReadWrite);

            IntPtr dllName = WinApi.VirtualAllocEx(_hProcess, IntPtr.Zero, 128, WinApi.AllocationType.Commit, WinApi.MemoryProtection.ExecuteReadWrite);

            byte[] lpBuffer = new ASCIIEncoding().GetBytes("ModLoader.dll");
            WinApi.WriteProcessMemory(_hProcess, dllName, lpBuffer, lpBuffer.Length, out _);

            IntPtr loadLibFromBase = WinApi.GetProcAddress(WinApi.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr loadLib = (IntPtr)((int)loadLibFromBase - (int)newEntryPoint - 11);

            byte[] code = new byte[]
            {
                0x68, leByte(oldEip, 3), leByte(oldEip, 2), leByte (oldEip, 1), leByte(oldEip, 0),
                0x9C,
                0x60,
                0x68, leByte(dllName, 3), leByte(dllName, 2), leByte (dllName, 1), leByte(dllName, 0),
                0xE8, leByte(loadLib, 3), leByte(loadLib, 2), leByte (loadLib, 1), leByte(loadLib, 0),
                0x61,
                0x9D,
                0xC3
            };

            WinApi.WriteProcessMemory(_hProcess, newEntryPoint, code, 256, out _);
            threadContext.Eip = (uint)newEntryPoint;
            WinApi.SetThreadContext(_hThread, ref threadContext);
        }

        private void Resume()
        {
            WinApi.ResumeThread(_hThread);
        }
    }
}
