using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2
{

        public unsafe class memcpy
        {
            public delegate void MemCpyFunction(void* des, void* src, uint bytes);

            private static readonly MemCpyFunction MemCpy;

            static memcpy()
            {
                var dynamicMethod = new DynamicMethod
                (
                    "MemCpy",
                    typeof(void),
                    new[] { typeof(void*), typeof(void*), typeof(uint) },
                    typeof(Program)
                );

                var ilGenerator = dynamicMethod.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Ldarg_2);

                ilGenerator.Emit(OpCodes.Cpblk);
                ilGenerator.Emit(OpCodes.Ret);

                MemCpy = (MemCpyFunction)dynamicMethod
                            .CreateDelegate(typeof(MemCpyFunction));
            }
        }
    
    public unsafe class MSVCRT
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy")]
        public static extern void* memcpy(void* dest, void* src, Int32 num);

        [DllImport("msvcrt.dll")]
        public static extern int srand(int seed);

        [DllImport("msvcrt.dll")]
        public static extern int rand();
    }

    public unsafe partial class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetPrivateProfileIntW(string Section, string Key, int Default, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetPrivateProfileStringW(string Section, string Key, string Default, char* ReturnedString, int Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern uint GetPrivateProfileStringA(string Section, string Key, void* Default, sbyte* ReturnedString, int Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPrivateProfileStructW(string Section, string Key, void* lpStruct, int StructSize, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileSectionNamesW(char* ReturnBuffer, int Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileSectionW(string Section, char* ReturnBuffer, int Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileStringW(string Section, string Key, string Value, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileStructW(string Section, string Key, void* lpStruct, int StructSize, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileSectionW(string Section, string String, string FileName);
    }

    public unsafe partial class WinMM
    {
        [DllImport("winmm.dll")]
        public static extern TIME timeGetTime();
    }


    /// <summary>
    /// Simplifies the native 4-byte sized time provided by timeGetTime()
    /// </summary>
    public struct TIME
    {
        private static TIME LastNowTime;

        public readonly uint Time;
        public TIME(uint _Value)
        {
            Time = _Value;
        }
        public TIME(int _Value)
        {
            Time = (uint)_Value;
        }
        public TIME AddMilliseconds(int Add)
        {
            return new TIME((uint)(Time + Add));
        }
        public TIME AddSeconds(int Add)
        {
            return new TIME((uint)(Time + (Add * 1000)));
        }
        public TIME AddMinutes(int Add)
        {
            return new TIME((uint)(Time + (Add * 60000)));
        }
        public TIME AddHours(int Add)
        {
            return new TIME((uint)(Time + (Add * 3600000)));
        }
        public override string ToString()
        {
            return Time.ToString();
        }
        public override int GetHashCode()
        {
            return (int)Time;
        }
        public static TIME Parse(string str)
        {
            return new TIME(uint.Parse(str));
        }

        public static TIME Now
        {
            get
            {
                TIME now = WinMM.timeGetTime();
                if (LastNowTime.Time >= now.Time)
                    throw new ApplicationException("timeGetTime() has been reset.");
                return now;
            }
        }
    }

}
