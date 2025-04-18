using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum StudentInfoID
    {
        StudentInfo = 0x02,
        FinishedStudentInfo = 0x03,
        StudentsStudentInfo = 0x04 /* The mentor system allows you to obtain rewards from your students' students */
    }

    public struct MentorStudentData
    {
        public uint UID;
        public uint Model;
        public uint _Unknown1;
        public uint _999999;
        private uint m_JoinDate;
        public byte Level;
        public byte Job;
        public ushort PKPoints;
        public ushort GuildID;
        private byte _GuildRank;
        public GuildRank GuildRank;
        public uint _Unknown2;
        public uint _Unknown3;
        private int m_Online;
        public uint _Unknown5;
        private uint m_Experience;
        public uint _Unknown7;
        public short BlessingHours;
        public ushort m_PlusItem;

        public void SetJoinDate(int Year, int Month, int Day)
        {
            m_JoinDate = (uint)((Year * 10000) + (Month * 100) + Day);
        }
        public void GetJoinDate(out int Year, out int Month, out int Day)
        {
            Year = (int)(m_JoinDate / 10000);
            Month = (int)((m_JoinDate % 10000) / 100);
            Day = (int)(m_JoinDate % 100); 
        }
        public uint JoinDate
        {
            get
            {
                return m_JoinDate;
            }
        }
        public float Experience
        {
            set 
            {
                m_Experience = (uint)((value / 0.01) * 6);
            }
        }
        public float PlusItem
        {
            set
            {
                m_PlusItem = (ushort)(value * 100);
            }
        }
        public bool Online
        {
            set { m_Online = value ? 1 : 0; }
        }
    }

    public unsafe class MentorStudentInfoPacket
    {
        public StudentInfoID ID;
        public string[] Strings;
        public byte StringsLength;
        public MentorStudentData Student;
        public uint UID;

        public MentorStudentInfoPacket()
        {
            Student = new MentorStudentData();
        }
        public void WriteStrings(string Name, string Student)
        {
            Strings = new string[2];
            Strings[0] = Name;
            Strings[1] = Student;
            StringsLength = (byte)(Name.Length + Student.Length);
        }
        public void WriteStrings(string Name, string Student, string StudentsSpouse)
        {
            Strings = new string[3];
            Strings[0] = Name;
            Strings[1] = Student;
            Strings[2] = StudentsSpouse;
            StringsLength = (byte)(Name.Length + Student.Length + StudentsSpouse.Length);
        }
        public void ClearStrings()
        {
            Strings = null;
            StringsLength = 0;
        }
        public void ClearStudent()
        {
            fixed (void* ptr = &Student)
                MSVCRT.memset(ptr, 0, sizeof(MentorStudentData));
        }
        public static implicit operator byte[](MentorStudentInfoPacket info)
        {
            byte[] packet = new byte[0x4a + info.StringsLength + 8];
            fixed (byte* ptr = packet)
            {
                *((ushort*)ptr) = (ushort)(packet.Length - 8);
                *((ushort*)(ptr + 2)) = 0x812;
                *((StudentInfoID*)(ptr + 4)) = info.ID;
                *((uint*)(ptr + 8)) = info.UID;
                *((MentorStudentData*)(ptr + 12)) = info.Student;
                if (info.Strings != null)
                {
                    ptr[68] = (byte)info.Strings.Length;
                    ushort pos = 69;
                    for (byte i = 0; i < info.Strings.Length; i++)
                    {
                        ptr[pos] = (byte)(info.Strings[i].Length);
                        info.Strings[i].CopyTo(ptr + pos + 1);
                        pos += (ushort)(info.Strings[i].Length + 1);
                    }
                }
                PacketBuilder.AppendTQServer(ptr, packet.Length);
            }
            return packet;
        }
    }
}
