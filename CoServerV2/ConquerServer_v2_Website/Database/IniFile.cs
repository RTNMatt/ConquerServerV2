using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Database
{
    // Represents a wrapper for working with INI configuration files using Windows API (via Kernel32 and MSVCRT).
    // Uses unsafe code and stackalloc for direct memory access and performance.
    public unsafe class IniFile
    {
        public string FileName;
        // Constructor accepting a file name
        public IniFile(string _FileName)
        {
            FileName = _FileName;
        }
        // Parameterless constructor initializes FileName to null
        public IniFile()
        {
            FileName = null;
        }

        // Constants used to define maximum buffer sizes for parsing different data types from strings
        public const int
            Int32_Size = 15,
            Int16_Size = 9,
            Int8_Size = 6,
            Bool_Size = 6,
            Double_Size = 20,
            Int64_Size = 22,
            Float_Size = 10;

        // Parsers for converting string to various types
        public static Func<string, int> ToInt32 = new Func<string, int>(int.Parse);
        public static Func<string, uint> ToUInt32 = new Func<string, uint>(uint.Parse);
        public static Func<string, short> ToInt16 = new Func<string, short>(short.Parse);
        public static Func<string, ushort> ToUInt16 = new Func<string, ushort>(ushort.Parse);
        public static Func<string, sbyte> ToInt8 = new Func<string, sbyte>(sbyte.Parse);
        public static Func<string, byte> ToUInt8 = new Func<string, byte>(byte.Parse);
        public static Func<string, bool> ToBool = new Func<string, bool>(bool.Parse);
        public static Func<string, double> ToDouble = new Func<string, double>(double.Parse);
        public static Func<string, long> ToInt64 = new Func<string, long>(long.Parse);
        public static Func<string, ulong> ToUInt64 = new Func<string, ulong>(ulong.Parse);
        public static Func<string, float> ToFloat = new Func<string, float>(float.Parse);

        // Reads a string from the INI file using Unicode
        public string ReadString(string Section, string Key, string Default, int Size)
        {
            char* lpBuffer = stackalloc char[Size]; // allocate a stack buffer
            Kernel32.GetPrivateProfileStringW(Section, Key, Default, lpBuffer, Size, FileName);
            return new string(lpBuffer).Trim('\0'); // remove null characters
        }
        // Overload with default size of 255
        public string ReadString(string Section, string Key, string Default)
        {
            return ReadString(Section, Key, Default, 255);
        }
        // Reads string using ANSI version (for binary-safe reads into void buffers)
        public void ReadString(string Section, string Key, void* Default, void* Buffer, int Size)
        {
            Kernel32.GetPrivateProfileStringA(Section, Key, Default, (sbyte*)Buffer, Size, FileName);
        }
        // Reads binary data (struct) from the INI file; uses fallback if not found
        public bool ReadStruct(string Section, string Key, void* lpDefault, void* lpStruct, int Size)
        {
            if (!Kernel32.GetPrivateProfileStructW(Section, Key, lpStruct, Size, FileName))
            {
                if (lpDefault != null)
                {
                    MSVCRT.memcpy(lpStruct, lpDefault, Size);   // fallback to default
                }
                return false;
            }
            return true;
        }
        // Generic method for reading and converting a string to any type (with parsing function)
        public T ReadValue<T>(string Section, string Key, T Default, Func<string, T> callback)
        {
            try
            {
                return callback.Invoke(ReadString(Section, Key, Default.ToString()));
            }
            catch
            {
                return Default;
            }
        }
        // Overload with buffer size option
        public T ReadValue<T>(string Section, string Key, T Default, Func<string, T> callback, int BufferSize)
        {
            try
            {
                return callback.Invoke(ReadString(Section, Key, Default.ToString(), BufferSize));
            }
            catch
            {
                return Default;
            }
        }

        // Wrapper methods for all primitive types:
        public int ReadInt32(string Section, string Key, int Default)
        {
            return ReadValue<int>(Section, Key, Default, ToInt32, Int32_Size);
        }
        public ulong ReadUInt64(string Section, string Key, ulong Default)
        {
            return ReadValue<ulong>(Section, Key, Default, ToUInt64, Int64_Size);
        }
        public long ReadInt64(string Section, string Key, long Default)
        {
            return ReadValue<long>(Section, Key, Default, ToInt64, Int64_Size);
        }
        public double ReadDouble(string Section, string Key, double Default)
        {
            return ReadValue<double>(Section, Key, Default, ToDouble, Double_Size);
        }
        public uint ReadUInt32(string Section, string Key, uint Default)
        {
            return ReadValue<uint>(Section, Key, Default, ToUInt32, Int32_Size);
        }
        public short ReadInt16(string Section, string Key, short Default)
        {
            return ReadValue<short>(Section, Key, Default, ToInt16, Int16_Size);
        }
        public ushort ReadUInt16(string Section, string Key, ushort Default)
        {
            return ReadValue<ushort>(Section, Key, Default, ToUInt16, Int16_Size);
        }
        public sbyte ReadSByte(string Section, string Key, sbyte Default)
        {
            return ReadValue<sbyte>(Section, Key, Default, ToInt8, Int8_Size);
        }
        public byte ReadByte(string Section, string Key, byte Default)
        {
            return ReadValue<byte>(Section, Key, Default, ToUInt8, Int8_Size);
        }
        public bool ReadBool(string Section, string Key, bool Default)
        {
            return ReadValue<bool>(Section, Key, Default, ToBool, Bool_Size);
        }
        public float ReadFloat(string Section, string Key, float Default)
        {
            return ReadValue<float>(Section, Key, Default, ToFloat, Float_Size);
        }

        // Write a string key-value to the INI file
        public void WriteString(string Section, string Key, string Value)
        {
            Kernel32.WritePrivateProfileStringW(Section, Key, Value, FileName);
        }
        // Generic write method
        public void Write<T>(string Section, string Key, T Value)
        {
            Kernel32.WritePrivateProfileStringW(Section, Key, Value.ToString(), FileName);
        }
        // Writes a binary structure to the INI file
        public void WriteStruct(string Section, string Key, void* lpStruct, int Size)
        {
            Kernel32.WritePrivateProfileStructW(Section, Key, lpStruct, Size, FileName);
        }

        // Retrieves section names (Unicode)
        public string[] GetSectionNames(int BufferSize)
        {
            char* lpBuffer = stackalloc char[BufferSize];
            int Size = Kernel32.GetPrivateProfileSectionNamesW(lpBuffer, BufferSize, FileName);
            if (Size == 0)
                return new string[0];
            return new string(lpBuffer, 0, Size - 1).Split('\0');
        }
        // Retrieves all keys and values in a section
        public string[] GetSection(string Section, int BufferSize)
        {
            char* lpBuffer = stackalloc char[BufferSize];
            int Size = Kernel32.GetPrivateProfileSectionW(Section, lpBuffer, BufferSize, FileName);
            if (Size == 0)
                return new string[0];
            return new string(lpBuffer, 0, Size - 1).Split('\0');
        }
        // Overloads with default buffer size (4096)
        public string[] GetSectionNames()
        {
            return GetSectionNames(4096);
        }
        public string[] GetSection(string Section)
        {
            return GetSection(Section, 4096);
        }
        // Checks if a section exists
        public bool SectionExists(string Section)
        {
            char* temp = stackalloc char[Section.Length + 1];
            int r = Kernel32.GetPrivateProfileSectionW(Section, temp, Section.Length + 1, FileName);
            return (temp[0] != 0);
        }
        // Checks if a key exists in a section
        public bool KeyExists(string Section, string Key)
        {
            const char not_used = (char)0x007F;
            const string not_used_s = "\007F";

            char* temp = stackalloc char[2];
            uint r = Kernel32.GetPrivateProfileStringW(Section, Key, not_used_s, temp, 2, FileName);
            return (r == 0 && temp[0] == 0) || (r > 0 && temp[0] != not_used);
        }

        // Deletes an entire section
        public void DeleteSection(string Section)
        {
            Kernel32.WritePrivateProfileSectionW(Section, null, FileName);
        }
        // Deletes a specific key from a section
        public void DeleteKey(string Section, string Key)
        {
            Kernel32.WritePrivateProfileStringW(Section, Key, null, FileName);
        }
    }
}
