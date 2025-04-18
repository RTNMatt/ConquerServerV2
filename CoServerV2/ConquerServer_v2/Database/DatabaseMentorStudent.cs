using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Database
{
    public unsafe struct DatabaseMentorStudent
    {
        public MentorStudentData StudentData;
        public fixed sbyte Name[16];
        public fixed sbyte Account[16];

        public void FromStudent(MentorStudent Student)
        {
            StudentData = Student.Data;
            fixed (sbyte* pName = Name, pAccount = Account)
            {
                MSVCRT.memset(pName, 0, 16);
                MSVCRT.memset(pAccount, 0, 16);
                Student.Name.CopyTo(pName);
                Student.Account.CopyTo(pAccount);
            }
        }
        public MentorStudent GetStudent()
        {
            MentorStudent student = new MentorStudent();
            student.Data = StudentData;
            fixed (sbyte* pName = Name, pAccount = Account)
            {
                student.Name = new string(pName);
                student.Account = new string(pAccount);
            }
            return student;
        }
    }
}
