using _IL2CPP;
using BSupport;
using MinHook;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Spoof_HWID
{
    [BManager.Attributes.ModuleInfo("HWID Spoofer", "1.0", "BlazeBest")]
    unsafe public class Plugin : BManager.VRModule
    {
        public static readonly string src_file = "HardWareID.txt";

        static Plugin()
        {
            if (File.Exists(src_file))
            {
                _fakeDeviceId = new IL2String_utf8(File.ReadAllText(src_file));
            }
            if (string.IsNullOrWhiteSpace(_fakeDeviceId?.ToString()))
            {
                _fakeDeviceId = new IL2String_utf8(CalculateHash<SHA1>(Guid.NewGuid().ToString()));
                File.WriteAllText(src_file, _fakeDeviceId.ToString(), Encoding.UTF8);
            }
            _fakeDeviceId.Static = true;
        }

        public delegate IntPtr _UnityEngine_SystemInfo();
        public static void Main()
        {
            try
            {
                IL2Method method = IL2CPP.AssemblyList["UnityEngine.CoreModule"].GetClass("SystemInfo", "UnityEngine").GetProperty("deviceUniqueIdentifier").GetGetMethod();
                if (method != null)
                {
                    hook.CreateHook<_UnityEngine_SystemInfo>(*(IntPtr*)method.Pointer, UnityEngine_SystemInfo);
                }
                else
                    throw new NullReferenceException();

                hook.EnableHooks();
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{ColorConsole.TextRGB(255, 0, 0)}[HWID Spoofer][Exception]{ColorConsole.Normal} {ex}");
            }
        }



        public static IL2String _fakeDeviceId;
        private static IntPtr UnityEngine_SystemInfo()
        {
            return _fakeDeviceId.Pointer;
        }

        public static string CalculateHash<T>(string input) where T : HashAlgorithm
        {
            byte[] array = CalculateHash<T>(Encoding.UTF8.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public static byte[] CalculateHash<T>(byte[] buffer) where T : HashAlgorithm
        {
            byte[] result;
            using (T t = typeof(T).GetMethod("Create", new Type[0]).Invoke(null, null) as T)
            {
                result = t.ComputeHash(buffer);
            }
            return result;
        }

        public static HookEngine hook = new HookEngine();
    }
}
