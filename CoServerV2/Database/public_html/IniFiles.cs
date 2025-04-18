/*

#compiler /unsafe

*/

using System;
using System.Runtime.InteropServices;

    public delegate T GenericConvertCallback<T, T2>(T2 value);
    public unsafe partial class Native
    {
        [DllImport("msvcrt.dll")]
        public static extern void* memcpy(void* dest, void* src, uint size);
        [DllImport("msvcrt.dll")]
        public static extern void* memcpy(byte[] dest, byte[] src, int size);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern uint GetPrivateProfileIntA(string Section, string Key, int Default, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern uint GetPrivateProfileStringA(string Section, string Key, string Default, sbyte[] ReturnedString, uint Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPrivateProfileStructA(string Section, string Key, void* lpStruct, uint StructSize, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileStringA(string Section, string Key, string Value, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileStructA(string Section, string Key, void* lpStruct, uint StructSize, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern int GetPrivateProfileSectionNames(sbyte[] ReturnBuffer, uint Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern int GetPrivateProfileSection(string Section, sbyte[] ReturnBuffer, uint Size, string FileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileSectionA(string Section, string String, string FileName);
    }

    public unsafe class IniFile
    {
        public string FileName;
        public IniFile(string _FileName)
        {
            FileName = _FileName;
        }

        public const uint Int32_Size = 15;
        public static GenericConvertCallback<int, string> ToInt32 = new GenericConvertCallback<int, string>(int.Parse);
        public static GenericConvertCallback<uint, string> ToUInt32 = new GenericConvertCallback<uint, string>(uint.Parse);
        public const uint Int16_Size = 9;
        public static GenericConvertCallback<short, string> ToInt16 = new GenericConvertCallback<short, string>(short.Parse);
        public static GenericConvertCallback<ushort, string> ToUInt16 = new GenericConvertCallback<ushort, string>(ushort.Parse);
        public const uint Int8_Size = 6;
        public static GenericConvertCallback<sbyte, string> ToInt8 = new GenericConvertCallback<sbyte, string>(sbyte.Parse);
        public static GenericConvertCallback<byte, string> ToUInt8 = new GenericConvertCallback<byte, string>(byte.Parse);
        public const uint Bool_Size = 6;
        public static GenericConvertCallback<bool, string> ToBool = new GenericConvertCallback<bool, string>(bool.Parse);

        public string ReadString(string Section, string Key, string Default, uint Size)
        {
            sbyte[] Buffer = new sbyte[Size];
            Native.GetPrivateProfileStringA(Section, Key, Default, Buffer, Size, FileName);
            fixed (sbyte* lpBuffer = Buffer)
                return new string(lpBuffer).Trim('\0');
        }
        public string ReadString(string Section, string Key, string Default)
        {
            return ReadString(Section, Key, Default, 255);
        }
        public bool ReadStruct(string Section, string Key, void* lpDefault, void* lpStruct, uint Size)
        {
            if (!Native.GetPrivateProfileStructA(Section, Key, lpStruct, Size, FileName))
            {
                if (lpDefault != null)
                {
                    Native.memcpy(lpStruct, lpDefault, Size);
                }
                return false;
            }
            return true;
        }
        public T ReadValue<T>(string Section, string Key, T Default, GenericConvertCallback<T, string> callback)
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
        public T ReadValue<T>(string Section, string Key, T Default, GenericConvertCallback<T, string> callback, uint BufferSize)
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

        public int ReadInt32(string Section, string Key, int Default)
        {
            return ReadValue<int>(Section, Key, Default, ToInt32, Int32_Size);
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

        public void WriteString(string Section, string Key, object Value)
        {
            Native.WritePrivateProfileStringA(Section, Key, Value.ToString(), FileName);
        }
        public void WriteStruct(string Section, string Key, void* lpStruct, uint Size)
        {
            Native.WritePrivateProfileStructA(Section, Key, lpStruct, Size, FileName);
        }

        public string[] GetSectionNames(uint BufferSize)
        {
            sbyte[] Buffer = new sbyte[BufferSize];
            int Size = Native.GetPrivateProfileSectionNames(Buffer, BufferSize, FileName);
            fixed (sbyte* lpBuffer = Buffer)
                return new string(lpBuffer, 0, (int)Size).Split('\0');
        }
        public string[] GetSection(string Section, uint BufferSize)
        {
            sbyte[] Buffer = new sbyte[BufferSize];
            int Size = Native.GetPrivateProfileSection(Section, Buffer, BufferSize, FileName);
            fixed (sbyte* lpBuffer = Buffer)
                return new string(lpBuffer, 0, (int)Size).Split('\0');
        }
        public void DeleteSection(string Section)
        {
            Native.WritePrivateProfileSectionA(Section, null, FileName);
        }
        public void DeleteKey(string Section, string Key)
        {
            Native.WritePrivateProfileStringA(Section, Key, null, FileName);
        }
    }