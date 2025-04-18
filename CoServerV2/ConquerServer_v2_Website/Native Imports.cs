using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ConquerServer_v2
{
    public unsafe class MSVCRT
    {
        [DllImport("msvcrt.dll")]
        public static extern void* memcpy(void* dst, void* src, int length);
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
}
