using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AcqModeling
{
    // 25 bytes for a single coincidence event
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PETCoincidence
    {
        /** PET contains 4 rings, 48 blocks. Each block has a 15 * 15 crystal matrix */

        #region Data

        // [4 bytes, timestamp]
        public UInt32 Timestamp;

        // [2 bytes, first block's index]
        public UInt16 Position1;

        // [2 bytes, second block's index]
        public UInt16 Position2;

        // [8 bytes, position of the event in the first block]
        public UInt16 XPlus1, XMinus1, YPlus1, YMinus1;

        // [8 bytes, position of the event in the second block]
        public UInt16 XPlus2, XMinus2, YPlus2, YMinus2;

        // [1 byte, flags]
        public byte Flags;

        #endregion

        static int Ofs_Timestamp = (int)Marshal.OffsetOf(typeof(PETCoincidence), "Timestamp");
        static int Ofs_Position1 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "Position1");
        static int Ofs_Position2 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "Position2");
        static int Ofs_XPlus1 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "XPlus1");
        static int Ofs_XMinus1 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "XMinus1");
        static int Ofs_YPlus1 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "YPlus1");
        static int Ofs_YMinus1 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "YMinus1");
        static int Ofs_XPlus2 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "XPlus2");
        static int Ofs_XMinus2 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "XMinus2");
        static int Ofs_YPlus2 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "YPlus2");
        static int Ofs_YMinus2 = (int)Marshal.OffsetOf(typeof(PETCoincidence), "YMinus2");
        static int Ofs_Flags = (int)Marshal.OffsetOf(typeof(PETCoincidence), "Flags");

        public void FromBytes(int Offset, byte[] Src)
        {
            // 0-3
            Timestamp = BitConverter.ToUInt32(Src, Offset + Ofs_Timestamp);
            // 4-5, 6-7
            Position1 = BitConverter.ToUInt16(Src, Offset + Ofs_Position1);
            Position2 = BitConverter.ToUInt16(Src, Offset + Ofs_Position2);
            // 8-9, 10-11, 12-13, 14-15
            XPlus1 = BitConverter.ToUInt16(Src, Offset + Ofs_XPlus1);
            XMinus1 = BitConverter.ToUInt16(Src, Offset + Ofs_XMinus1);
            YPlus1 = BitConverter.ToUInt16(Src, Offset + Ofs_YPlus1);
            YMinus1 = BitConverter.ToUInt16(Src, Offset + Ofs_YMinus1);
            // 16-17, 18-19, 20-21, 22-23
            XPlus2 = BitConverter.ToUInt16(Src, Offset + Ofs_XPlus2);
            XMinus2 = BitConverter.ToUInt16(Src, Offset + Ofs_XMinus2);
            YPlus2 = BitConverter.ToUInt16(Src, Offset + Ofs_YPlus2);
            YMinus2 = BitConverter.ToUInt16(Src, Offset + Ofs_YMinus2);
            // 24th byte
            Flags = Src[Offset + Ofs_Flags];
        }

        public void ToBytes(int Offset, byte[] Dest)
        {
            BitUtils.ToBytes32(Timestamp, Dest, Offset + Ofs_Timestamp);

            BitUtils.ToBytes16(Position1, Dest, Offset + Ofs_Position1);
            BitUtils.ToBytes16(Position2, Dest, Offset + Ofs_Position2);

            BitUtils.ToBytes16(XPlus1, Dest, Offset + Ofs_XPlus1);
            BitUtils.ToBytes16(XMinus1, Dest, Offset + Ofs_XMinus1);
            BitUtils.ToBytes16(YPlus1, Dest, Offset + Ofs_YPlus1);
            BitUtils.ToBytes16(YMinus1, Dest, Offset + Ofs_YMinus1);

            BitUtils.ToBytes16(XPlus2, Dest, Offset + Ofs_XPlus2);
            BitUtils.ToBytes16(XMinus2, Dest, Offset + Ofs_XMinus2);
            BitUtils.ToBytes16(YPlus2, Dest, Offset + Ofs_YPlus2);
            BitUtils.ToBytes16(YMinus2, Dest, Offset + Ofs_YMinus2);

            Dest[Offset + Ofs_Flags] = Flags;
        }

        public static PETCoincidence RandomCoincidence(System.Random rnd = null)
        {
            var res = new PETCoincidence();

            var r = (rnd == null) ? new System.Random() : rnd;

            res.Timestamp = (UInt32)r.Next();
            res.Position1 = (UInt16)(r.Next(0, 47) + 64 * r.Next(0, 3));
            res.Position2 = (UInt16)(r.Next(0, 47) + 64 * r.Next(0, 3));

            res.XPlus1 = (UInt16)r.Next(0, 255);
            res.YPlus1 = (UInt16)r.Next(0, 255);
            res.XMinus1 = (UInt16)r.Next(0, 255);
            res.YMinus1 = (UInt16)r.Next(0, 255);

            res.XPlus2 = (UInt16)r.Next(0, 255);
            res.YPlus2 = (UInt16)r.Next(0, 255);
            res.XMinus2 = (UInt16)r.Next(0, 255);
            res.YMinus2 = (UInt16)r.Next(0, 255);

            res.Flags = (byte)r.Next();

            return res;
        }

        public bool CompareTo(PETCoincidence p)
        {
            return (p.Timestamp == this.Timestamp) &&
                (p.Position1 == this.Position1) &&
                (p.Position2 == this.Position2) &&
                (p.XPlus1 == this.XPlus1) &&
                (p.YPlus1 == this.YPlus1) &&
                (p.XMinus1 == this.XMinus1) &&
                (p.YMinus1 == this.YMinus1) &&
                (p.XPlus2 == this.XPlus2) &&
                (p.YPlus2 == this.YPlus2) &&
                (p.XMinus2 == this.XMinus2) &&
                (p.YMinus2 == this.YMinus2) &&
                (p.Flags == this.Flags);
        }

        public static int GetArraySize(int NumCoinc)
        {
            return PETStructSizes.PETCoincidence_Size * NumCoinc;
        }

        // TODO: range checking !
        public static void EncodeArray(byte[] b, int Offset, PETCoincidence[] coinc)
        {
            int ofs = Offset;

            for (int i = 0; i < coinc.Length; i++)
            {
                coinc[i].ToBytes(ofs, b);
                ofs += PETStructSizes.PETCoincidence_Size;
            }
        }

        public static PETCoincidence[] DecodeArray(byte[] b, int Offset, int Size)
        {
            int ofs = Offset;
            int num = (Size) / PETStructSizes.PETCoincidence_Size;
            var res = new PETCoincidence[num];

            for (int i = 0; i < num; i++)
            {
                res[i].FromBytes(ofs, b);
                ofs += PETStructSizes.PETCoincidence_Size;
            }

            return res;
        }
    }

    // 29(TOF) bytes for a single coincidence event
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PETCoincidenceTOF
    {
        //public PETCoincidenceTOF()
        //{
        //}

        public PETCoincidenceTOF(PETSingleEvent first, PETSingleEvent second, byte flags = 0)
        {
            this.Timestamp1 = first.Timestamp;
            this.Timestamp2 = second.Timestamp;
            this.Position1 = first.Position;
            this.Position2 = second.Position;
            this.XPlus1 = first.XPlus;
            this.XMinus1 = first.XMinus;
            this.YPlus1 = first.YPlus;
            this.YMinus1 = first.YMinus;
            this.XPlus2 = second.XPlus;
            this.XMinus2 = second.XMinus;
            this.YPlus2 = second.YPlus;
            this.YMinus2 = second.YMinus;
            this.Flags = flags;
        }
        /** PET contains 4 rings, 48 blocks. Each block has a 15 * 15 crystal matrix */

        #region Data

        // [8 bytes, two timestamps] for TOF PET we would need two timestamps
        public UInt32 Timestamp1, Timestamp2;

        // [2 bytes, first block's index]
        public UInt16 Position1;

        // [2 bytes, second block's index]
        public UInt16 Position2;

        // [8 bytes, position of the event in the first block]
        public UInt16 XPlus1, XMinus1, YPlus1, YMinus1;

        // [8 bytes, position of the event in the second block]
        public UInt16 XPlus2, XMinus2, YPlus2, YMinus2;

        // [1 byte, flags]
        public byte Flags;

        #endregion

        #region indexing

        public int Block1
        {
            get { return (Position1 >> 4) & 0x3F; }
        }

        public int Block2
        {
            get { return (Position2 >> 4) & 0x3F; }
        }

        #endregion

        static int Ofs_Timestamp1 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "Timestamp1");
        static int Ofs_Timestamp2 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "Timestamp2");
        static int Ofs_Position1 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "Position1");
        static int Ofs_Position2 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "Position2");
        static int Ofs_XPlus1 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "XPlus1");
        static int Ofs_XMinus1 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "XMinus1");
        static int Ofs_YPlus1 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "YPlus1");
        static int Ofs_YMinus1 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "YMinus1");
        static int Ofs_XPlus2 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "XPlus2");
        static int Ofs_XMinus2 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "XMinus2");
        static int Ofs_YPlus2 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "YPlus2");
        static int Ofs_YMinus2 = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "YMinus2");
        static int Ofs_Flags = (int)Marshal.OffsetOf(typeof(PETCoincidenceTOF), "Flags");

        public void FromBytes(int Offset, byte[] Src)
        {
            // 0-3, 4-7
            Timestamp1 = BitConverter.ToUInt32(Src, Offset + Ofs_Timestamp1);
            Timestamp2 = BitConverter.ToUInt32(Src, Offset + Ofs_Timestamp2);
            // 8-9, 10-11
            Position1 = BitConverter.ToUInt16(Src, Offset + Ofs_Position1);
            Position2 = BitConverter.ToUInt16(Src, Offset + Ofs_Position2);
            // 12-13, 14-15, 16-17, 18-19
            XPlus1 = BitConverter.ToUInt16(Src, Offset + Ofs_XPlus1);
            XMinus1 = BitConverter.ToUInt16(Src, Offset + Ofs_XMinus1);
            YPlus1 = BitConverter.ToUInt16(Src, Offset + Ofs_YPlus1);
            YMinus1 = BitConverter.ToUInt16(Src, Offset + Ofs_YMinus1);
            // 20-21, 22-23, 24-25, 26-27
            XPlus2 = BitConverter.ToUInt16(Src, Offset + Ofs_XPlus2);
            XMinus2 = BitConverter.ToUInt16(Src, Offset + Ofs_XMinus2);
            YPlus2 = BitConverter.ToUInt16(Src, Offset + Ofs_YPlus2);
            YMinus2 = BitConverter.ToUInt16(Src, Offset + Ofs_YMinus2);
            // 28th byte
            Flags = Src[Offset + Ofs_Flags];
        }

        public void ToBytes(int Offset, byte[] Dest)
        {
            BitUtils.ToBytes32(Timestamp1, Dest, Offset + Ofs_Timestamp1);
            BitUtils.ToBytes32(Timestamp2, Dest, Offset + Ofs_Timestamp2);

            BitUtils.ToBytes16(Position1, Dest, Offset + Ofs_Position1);
            BitUtils.ToBytes16(Position2, Dest, Offset + Ofs_Position2);

            BitUtils.ToBytes16(XPlus1, Dest, Offset + Ofs_XPlus1);
            BitUtils.ToBytes16(XMinus1, Dest, Offset + Ofs_XMinus1);
            BitUtils.ToBytes16(YPlus1, Dest, Offset + Ofs_YPlus1);
            BitUtils.ToBytes16(YMinus1, Dest, Offset + Ofs_YMinus1);

            BitUtils.ToBytes16(XPlus2, Dest, Offset + Ofs_XPlus2);
            BitUtils.ToBytes16(XMinus2, Dest, Offset + Ofs_XMinus2);
            BitUtils.ToBytes16(YPlus2, Dest, Offset + Ofs_YPlus2);
            BitUtils.ToBytes16(YMinus2, Dest, Offset + Ofs_YMinus2);

            Dest[Offset + Ofs_Flags] = Flags;
        }
    }

    // 12 bytes for non-TOF coincidence
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PETDigitalCoincidence
    {
        public UInt32 Timestamp;

        public UInt16 Position1, Position2;

        public byte I1, J1;
        public byte I2, J2;

        public static int GetArraySize(int numItems)
        {
            return numItems * PETStructSizes.PETDigitalCoincidence_Size;
        }

        public void ToBytes(byte[] b, int Ofs)
        {
            BitUtils.ToBytes32(this.Timestamp, b, Ofs + 0);
            BitUtils.ToBytes16(this.Position1, b, Ofs + 4);
            BitUtils.ToBytes16(this.Position2, b, Ofs + 6);
            b[Ofs + 8] = I1;
            b[Ofs + 9] = J1;
            b[Ofs + 10] = I2;
            b[Ofs + 11] = J2;
        }

        public static PETDigitalCoincidence FromBytes(byte[] b, int Ofs)
        {
            var res = new PETDigitalCoincidence();

            res.Timestamp = BitConverter.ToUInt32(b, Ofs + 0);
            res.Position1 = BitConverter.ToUInt16(b, Ofs + 4);
            res.Position2 = BitConverter.ToUInt16(b, Ofs + 6);

            res.I1 = b[Ofs + 8];
            res.J1 = b[Ofs + 9];
            res.I2 = b[Ofs + 10];
            res.J2 = b[Ofs + 11];

            return res;
        }

        public static void EncodeArray(PETDigitalCoincidence[] coinc, byte[] b, int Ofs)
        {
            int ofs = Ofs;

            for (int i = 0; i < coinc.Length; i++)
            {
                coinc[i].ToBytes(b, ofs);
                ofs += PETStructSizes.PETDigitalCoincidence_Size;
            }
        }

        public static PETDigitalCoincidence[] DecodeArray(byte[] b, int Ofs, int Len)
        {
            int ofs = Ofs;

            var res = new List<PETDigitalCoincidence>();

            int num = Len / PETStructSizes.PETDigitalCoincidence_Size;

            for (int i = 0; i < num; i++)
            {
                var coi = PETDigitalCoincidence.FromBytes(b, ofs);
                res.Add(coi);

                ofs += PETStructSizes.PETDigitalCoincidence_Size;
            }

            return res.ToArray();
        }
    }

    // 16 bytes for TOF coincidence
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PETDigitalCoincidenceTOF
    {
        public UInt32 Timestamp1, Timestamp2;

        public UInt16 Position1, Position2;

        public byte I1, J1;
        public byte I2, J2;
    }
}
