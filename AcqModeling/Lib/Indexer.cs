using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AcqModeling
{
    public class Indexer
    {
        public Indexer(int baseNumDets, int baseNumRings, int detMash = 1, int ringMash = 1)
        {
            this.baseNumDets = baseNumDets;
            this.baseNumRings = baseNumRings;
            this.detMash = detMash;
            this.ringMash = ringMash;
            NumDetectors = baseNumDets / detMash;
            NumRings = baseNumRings / ringMash;
            NumDirs = NumDetectors / 2;
            NumLines = NumDetectors - 1;
        }

        public int GetRing(int aPosition, int aJ)
        {
            var ringGroup = aPosition & 0xF;

            var j = ringGroup * 15 + aJ; //EventConverter.jForY(this.YPlus, this.YMinus);

            return j / ringMash;
        }

        public int GetDetector(int aPosition, int aI)
        {
            var blockNumber = (aPosition >> 4) & 0xFF;

            var i = blockNumber * 15 + aI; // EventConverter.iForX(this.XPlus, this.XMinus);

            return i / detMash;
        }

        public int GetDir(int p1, int p2, int i1, int i2)
        {
            var d1 = GetDetector(p1, i1);
            var d2 = GetDetector(p2, i2);
            int dir = GetDir(d1, d2);
            return dir;
        }

        public int GetDir(int d1, int d2)
        {
            return ((d1 + d2) % NumDetectors) / 2;
        }

        public int GetLine(int p1, int p2, int i1, int i2)
        {
            var d1 = GetDetector(p1, i1);
            var d2 = GetDetector(p2, i2);
            int line = GetLine(d1, d2);
            return line;
        }

        public int GetLine(int d1, int d2)
        {
            if (d1 > d2)
            {
                int tmp = d1;
                d1 = d2;
                d2 = tmp;
            }
            if (d1 + d2 >= NumDetectors)
            {
                int tmp = d1;
                d1 = d2;
                d2 = tmp;
            }
            return (d1 - d2 + NumDetectors) % NumDetectors - 1;
        }

        public int NumDetectors { get; private set; }
        public int NumDirs { get; private set; }
        public int NumLines { get; private set; }
        public int NumRings { get; private set; }

        public int DetMash { get { return detMash; } }
        public int RingMash { get { return ringMash; } }

        private int baseNumDets;
        private int baseNumRings;
        private int detMash;
        private int ringMash;
    }
}