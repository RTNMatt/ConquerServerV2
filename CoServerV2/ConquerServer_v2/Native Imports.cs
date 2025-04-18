using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace ConquerServer_v2
{
    public unsafe class MSVCRT
    {
        [DllImport("msvcrt.dll")]
        public static extern void* memcpy(void* dst, void* src, int length);
        [DllImport("msvcrt.dll")]
        public static extern void* memset(void* dst, byte fill, int length);
        [DllImport("msvcrt.dll")]
        public static extern void* malloc(int size);
        [DllImport("msvcrt.dll")]
        public static extern void free(void* memblock);
        [DllImport("msvcrt.dll")]
        public static extern void* realloc(void* memblock, int size);
        [DllImport("msvcrt.dll")]
        public static extern int memcmp(void* buf1, void* buf2, int count);
        [DllImport("msvcrt.dll", CharSet = CharSet.Unicode)]
        public static extern FILE* _wfopen(string filename, string mode);
        [DllImport("msvcrt.dll")]
        public static extern void fclose(FILE* hFile);
        [DllImport("msvcrt.dll")]
        public static extern void* fgets(void* u_str, int u_str_size, FILE* hFile);
        [DllImport("msvcrt.dll")]
        public static extern int fgetpos(FILE* hFile, long* pos);
        [DllImport("msvcrt.dll")]
        public static extern int fsetpos(FILE* hFile, long* pos);
        [DllImport("msvcrt.dll")]
        public static extern int fread(void* ptr, int size, int count, FILE* hFile);
        [DllImport("msvcrt.dll")]
        public static extern int fwrite(void* ptr, int size, int count, FILE* hFile);
        [DllImport("msvcrt.dll")]
        public static extern int fseek(FILE* hFile, int offset, SeekOrigin origin);
    }

    public unsafe partial class WinMM
    {
        [DllImport("winmm.dll")]
        public static extern TIME timeGetTime();
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, int* numBytesWritten, IntPtr lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, int* numBytesRead, IntPtr lpOverlapped);
        [DllImport("kernel32.dll")]
        public static extern unsafe int SetFilePointer(SafeFileHandle handle, int lo, int* hi, SeekOrigin origin);
    }

    /// <summary>
    /// The file structure implemented for the file-api functions in msvcrt.dll
    /// </summary>
    public unsafe struct FILE
    {
        public sbyte* _ptr;
        public int _cnt;
        public sbyte* _base;
        public int _flag;
        public int _file;
        public int _charbuf;
        public int _bufsiz;
        public sbyte* _tmpfname;
    };

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

    /// <summary>
    /// Exposes a keep-alive pointer to the managed world.
    /// Pointer created with malloc(), freed with free(), realloced with realloc().
    /// </summary>
    public unsafe class SafePointer
    {
        private bool freed;
        private byte* m_Addr;
        private int m_MemoryInBytes;
        public byte* Addr { get { return m_Addr; } }
        public int MemoryInBytes { get { return m_MemoryInBytes; } }

        public SafePointer(int Size)
        {
            m_Addr = (byte*)MSVCRT.malloc(Size);
            MSVCRT.memset(m_Addr, 0, Size);
            m_MemoryInBytes = Size;
            freed = false;
        }
        public void Realloc(int Size)
        {
            m_Addr = (byte*)MSVCRT.realloc(m_Addr, Size);
            MSVCRT.memset(m_Addr, 0, Size);
            m_MemoryInBytes = Size;
            freed = false;
        }
        public void Free()
        {
            if (!freed)
            {
                MSVCRT.free(Addr);
                freed = true;
            }
        }
        ~SafePointer()
        {
            Free();
        }
    }

    /// <summary>
    /// Implements support to existing .NET classes to allows them to interact
    /// with native functions, and actions more easily
    /// </summary>
    public static unsafe class NativeExtended
    {
        public static void CopyTo(this string s, void* pDest)
        {
            byte* Dest = (byte*)pDest;
            for (int i = 0; i < s.Length; i++)
            {
                Dest[i] = (byte)s[i];
            }
        }
        public static bool CheckBitFlag(this uint value, uint flag)
        {
            return ((value & flag) == flag);
        }
    }
}
