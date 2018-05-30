using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/**
   В данных, получаемых от БГР/ИПАК, неявно заданы индексы детекторов.

   Структуры PETSingleDigitalEvent и PETDigitalCoincidence(TOF) аналогичны
   структурам PETSingleEvent и PETCoincidence(TOF), но в них вычислены дискретные координаты
   детекторов по набору XPlus,XMinus,YPlus,YMinus.
*/

namespace AcqModeling
{
    /// <summary>
    /// Information about single detection event. 15 bytes per single event
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PETSingleEvent
    {
        #region Data containers

        // Timestamp [4 bytes]
        public UInt32 Timestamp;

        // Index of the detector [ring, 4 bits + block, 6 bits. 2 bytes total]
        public UInt16 Position;

        // Position of the event [8 = 4 * 2 bytes] within detector/crystal block
        public UInt16 XPlus, XMinus;
        public UInt16 YPlus, YMinus;

        // Misc flags [these might be at the beginning]
        public byte Flags;

        #endregion

        #region indexing

        public int Block
        {
            get { return (Position >> 4) & 0x3F; }
        }

        #endregion

        #region Processing methods

        static int Ofs_Timestamp = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "Timestamp");
        static int Ofs_Position = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "Position");
        static int Ofs_XPlus = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "XPlus");
        static int Ofs_XMinus = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "XMinus");
        static int Ofs_YPlus = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "YPlus");
        static int Ofs_YMinus = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "YMinus");
        static int Ofs_Flags = (int)Marshal.OffsetOf(typeof(PETSingleEvent), "Flags");

        public void FromBytes(int Offset, byte[] Src)
        {
            // 0-3
            Timestamp = BitConverter.ToUInt32(Src, Offset + Ofs_Timestamp);
            // 4-5
            Position = BitConverter.ToUInt16(Src, Offset + Ofs_Position);
            // 6-7
            XPlus = BitConverter.ToUInt16(Src, Offset + Ofs_XPlus);
            // 8-9
            XMinus = BitConverter.ToUInt16(Src, Offset + Ofs_XMinus);
            // 10-11
            YPlus = BitConverter.ToUInt16(Src, Offset + Ofs_YPlus);
            // 12-13
            YMinus = BitConverter.ToUInt16(Src, Offset + Ofs_YMinus);
            // 14th byte
            Flags = Src[Offset + Ofs_Flags];
        }

        public void ToBytes(int Offset, byte[] Dest)
        {
            BitUtils.ToBytes32(Timestamp, Dest, Offset + Ofs_Timestamp);

            BitUtils.ToBytes16(Position, Dest, Offset + Ofs_Position);

            BitUtils.ToBytes16(XPlus, Dest, Offset + Ofs_XPlus);
            BitUtils.ToBytes16(XMinus, Dest, Offset + Ofs_XMinus);

            BitUtils.ToBytes16(YPlus, Dest, Offset + Ofs_YPlus);
            BitUtils.ToBytes16(YMinus, Dest, Offset + Ofs_YMinus);

            Dest[Offset + Ofs_Flags] = Flags;
        }

        #endregion

        public static PETSingleEvent RandomEvent(System.Random rnd = null)
        {
            var res = new PETSingleEvent();

            var r = (rnd == null) ? new System.Random() : rnd;

            res.Timestamp = (UInt32)r.Next();
            res.Position = (UInt16)(r.Next(0, 47) + 64 * r.Next(0, 3));

            res.XPlus = (UInt16)r.Next(0, 255);
            res.YPlus = (UInt16)r.Next(0, 255);
            res.XMinus = (UInt16)r.Next(0, 255);
            res.YMinus = (UInt16)r.Next(0, 255);

            res.Flags = (byte)r.Next();

            return res;
        }

        public bool CompareTo(PETSingleEvent p)
        {
            return (p.Timestamp == this.Timestamp) &&
                (p.Position == this.Position) &&
                (p.XPlus == this.XPlus) &&
                (p.YPlus == this.YPlus) &&
                (p.XMinus == this.XMinus) &&
                (p.YMinus == this.YMinus) &&
                (p.Flags == this.Flags);
        }

        public static int GetArraySize(int NumEvents)
        {
            return PETStructSizes.PETSingleEvent_Size * NumEvents;
        }

        // TODO: range checking !
        public static void EncodeArray(byte[] b, int Offset, PETSingleEvent[] events)
        {
            int ofs = Offset;

            for (int i = 0; i < events.Length; i++)
            {
                events[i].ToBytes(ofs, b);
                ofs += PETStructSizes.PETSingleEvent_Size;
            }
        }

        public static PETSingleEvent[] DecodeArray(byte[] b, int Offset, int Size)
        {
            int ofs = Offset;
            int num = (Size) / PETStructSizes.PETSingleEvent_Size;
            var res = new PETSingleEvent[num];

            for (int i = 0; i < num; i++)
            {
                res[i].FromBytes(ofs, b);
                ofs += PETStructSizes.PETSingleEvent_Size;
            }

            return res;
        }
    }

    // 8 bytes for single digital detection event (discrete detector indices)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PETSingleDigitalEvent
    {
        // Timestamp [4 bytes]
        public UInt32 Timestamp;

        // Index of the detector [ring, 4 bits + block, 6 bits. 2 bytes total]
        public UInt16 Position;

        // Position of the event [2 * 2 bytes] within detector/crystal block
        public byte I, J;

        // Total energy of the event, calculate as XPlus + XMinus + YPlus + YMinus
        //		public UInt16 Energy;
        // Misc flags [these might be at the beginning]
        //		byte   Flags; // ?

        #region indexing

        public int Block
        {
            get { return (Position >> 4) & 0x3F; }
        }

        #endregion

        public static void GetRingAndDetectorForIJ(int aPosition, int aI, int aJ, out int Ring, out int Detector)
        {
            var blockNumber = (aPosition >> 4) & 0xFF;
            var ringGroup = aPosition & 0xF;

            var i = blockNumber * 15 + aI; // EventConverter.iForX(this.XPlus, this.XMinus);
            var j = ringGroup * 15 + aJ; //EventConverter.jForY(this.YPlus, this.YMinus);

            Detector = i;
            Ring = j;
        }

        // индекс кольца и индекс детектора в логическом кольце
        public void GetRingAndDetectorForEvent(out int Ring, out int Detector)
        {
            GetRingAndDetectorForIJ(this.Position, this.I, this.J, out Ring, out Detector);
        }

        public static void GetRingAndDetectorForXY(int aPosition, UInt16 XPlus, UInt16 XMinus, UInt16 YPlus, UInt16 YMinus,
            out int Ring, out int Detector)
        {
            var blockNumber = (aPosition >> 4) & 0xFF;
            var ringGroup = aPosition & 0xF;

            var i = blockNumber * 15 + EventConverter.iForX(XPlus, XMinus);
            var j = ringGroup * 15 + EventConverter.jForY(YPlus, YMinus);

            Detector = i;
            Ring = j;
        }

        public static int GetArraySize(int numItems)
        {
            return numItems * PETStructSizes.PETSingleDigitalEvent_Size;
        }

        public void ToBytes(byte[] b, int Ofs)
        {
            BitUtils.ToBytes32(this.Timestamp, b, Ofs + 0);
            BitUtils.ToBytes16(this.Position, b, Ofs + 4);
            b[Ofs + 6] = I;
            b[Ofs + 7] = J;
        }

        public static PETSingleDigitalEvent FromBytes(byte[] b, int Ofs)
        {
            var res = new PETSingleDigitalEvent();

            res.Timestamp = BitConverter.ToUInt32(b, Ofs + 0);
            res.Position = BitConverter.ToUInt16(b, Ofs + 4);

            res.I = b[Ofs + 6];
            res.J = b[Ofs + 7];

            return res;
        }

        public static void EncodeArray(PETSingleDigitalEvent[] events, byte[] b, int Ofs)
        {
            int ofs = Ofs;

            for (int i = 0; i < events.Length; i++)
            {
                events[i].ToBytes(b, ofs);
                ofs += PETStructSizes.PETSingleDigitalEvent_Size;
            }
        }

        public static PETSingleDigitalEvent[] DecodeArray(byte[] b, int Ofs, int Len)
        {
            int ofs = Ofs;
            var res = new List<PETSingleDigitalEvent>();
            int num = Len / PETStructSizes.PETSingleDigitalEvent_Size;

            for (int i = 0; i < num; i++)
            {
                var evt = PETSingleDigitalEvent.FromBytes(b, ofs);
                res.Add(evt);

                ofs += PETStructSizes.PETSingleDigitalEvent_Size;
            }

            return res.ToArray();
        }
    }
}
