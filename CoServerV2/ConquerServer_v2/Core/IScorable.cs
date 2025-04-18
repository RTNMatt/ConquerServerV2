using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ConquerServer_v2.Core
{
    public interface IScorable
    {
        int Score { get; set; }
    }
    public class ScoreComparer : IComparer
    {
        public static ScoreComparer CMP = new ScoreComparer();

        int IComparer.Compare(object x, object y)
        {
            return (y as IScorable).Score - (x as IScorable).Score;
        }
    }
}