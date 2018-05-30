using System;

namespace AcqModeling
{
    public static class EventConverter
    {
        public static int Nx = 15; // 15 crystal rows
        public static int Ny = 15; // 15 crystals in a row

        #region Converting to digital events/coincidences

        // Recalculate (X+, X-) to i
        public static UInt16 iForX(UInt16 XPlus, UInt16 XMinus)
        {
            // X in [-1, +1]
            var X = (float)(XPlus - XMinus) / (float)(XPlus + XMinus);

            // i in [0 .. Ny - 1]
            var i = Math.Round((float)(Nx - 1) * (X + 1.0f) / 2.0f);

            return (UInt16)i;
        }

        // Recalculate (Y+, Y-) to j
        public static UInt16 jForY(UInt16 YPlus, UInt16 YMinus)
        {
            // Y in [-1, +1]
            var Y = (float)(YPlus - YMinus) / (float)(YPlus + YMinus);

            // j in [0 .. Ny - 1]
            var j = Math.Round((float)(Ny - 1) * (Y + 1.0f) / 2.0f);

            return (UInt16)j;
        }

        // Calculate energy for the event using X/Y detection coordinates
        public static int EnergyForEvent(PETSingleEvent In)
        {
            return (int)(In.XPlus + In.XMinus + In.YPlus + In.YMinus);
        }

        // Recalculate (X+...) from (I, J)
        public static void CalculateXYEventFromIJ(PETSingleDigitalEvent In, ref PETSingleEvent Out)
        {
            Out.Position = In.Position;
            Out.Timestamp = In.Timestamp;

            // не совсем точно с энергией. она должна быть взята из пакета, но пока её нет в протоколе
            GetXYFromIJ(In.I, In.J, 511, out Out.XPlus, out Out.XMinus, out Out.YPlus, out Out.YMinus);
            // Out.E = 511; или In.E
        }

        public static void CalculateXYCoincidenceFromIJ(PETDigitalCoincidence In, ref PETCoincidence Out)
        {
            Out.Position1 = In.Position1;
            Out.Position2 = In.Position2;
            Out.Timestamp = In.Timestamp;

            GetXYFromIJ(In.I1, In.J1, 511.0, out Out.XPlus1, out Out.XMinus1, out Out.YPlus1, out Out.YMinus1);
            GetXYFromIJ(In.I1, In.J1, 511.0, out Out.XPlus2, out Out.XMinus2, out Out.YPlus2, out Out.YMinus2);
        }

        // Recalculate (I,J) from (X+, X-, Y+, Y-) and copy other parameters
        public static void CalculateDigitalEvent(PETSingleEvent In, ref PETSingleDigitalEvent OutE)
        {
            OutE.I = (byte)iForX(In.XPlus, In.XMinus);
            OutE.J = (byte)iForX(In.YPlus, In.YMinus);

            OutE.Position = In.Position;
            OutE.Timestamp = In.Timestamp;

            // OutE.Flags = In.Flags;
        }

        public static void CalculateDigitalCoincidence(PETCoincidence In, ref PETDigitalCoincidence OutE)
        {
            OutE.I1 = (byte)iForX(In.XPlus1, In.XMinus1);
            OutE.J1 = (byte)jForY(In.YPlus1, In.YMinus1);

            OutE.I2 = (byte)iForX(In.XPlus2, In.XMinus2);
            OutE.J2 = (byte)jForY(In.YPlus2, In.YMinus2);

            OutE.Position1 = In.Position1;
            OutE.Position2 = In.Position2;

            OutE.Timestamp = In.Timestamp;

            // OutE.Flags = In.Flags;
        }

        public static void CalculateDigitalCoincidenceTOF(PETCoincidenceTOF In, ref PETDigitalCoincidenceTOF OutE)
        {
            OutE.I1 = (byte)iForX(In.XPlus1, In.XMinus1);
            OutE.J1 = (byte)jForY(In.YPlus1, In.YMinus1);

            OutE.I2 = (byte)iForX(In.XPlus2, In.XMinus2);
            OutE.J2 = (byte)jForY(In.YPlus2, In.YMinus2);

            OutE.Position1 = In.Position1;
            OutE.Position2 = In.Position2;

            OutE.Timestamp1 = In.Timestamp1;
            OutE.Timestamp2 = In.Timestamp2;

            // OutE.Flags = In.Flags;
        }

        public static PETDigitalCoincidence[] ConvertXYToIJCoincidences(PETCoincidence[] coinc)
        {
            var res = new PETDigitalCoincidence[coinc.Length];

            for (int i = 0; i < res.Length; i++)
            {
                CalculateDigitalCoincidence(coinc[i], ref res[i]);
            }

            return res;
        }

        public static PETSingleDigitalEvent[] ConvertXYToIJEvents(PETSingleEvent[] events)
        {
            var res = new PETSingleDigitalEvent[events.Length];

            for (int i = 0; i < res.Length; i++)
            {
                CalculateDigitalEvent(events[i], ref res[i]);
            }

            return res;
        }

        public static PETCoincidence[] ConvertIJToXYCoincidences(PETDigitalCoincidence[] coinc)
        {
            var res = new PETCoincidence[coinc.Length];

            for (int i = 0; i < coinc.Length; i++)
            {
                CalculateXYCoincidenceFromIJ(coinc[i], ref res[i]);
            }

            return res;
        }

        public static PETSingleEvent[] ConvertIJToXYEvents(PETSingleDigitalEvent[] events)
        {
            var res = new PETSingleEvent[events.Length];

            for (int i = 0; i < res.Length; i++)
            {
                //                res[i] = 
            }

            return res;
        }

        #endregion

        #region Converting from digital events/coincidences

        public static void GetXYFromIJ(byte i, byte j, double energy, out UInt16 xp, out UInt16 xm, out UInt16 yp, out UInt16 ym)
        {
            xp = (UInt16)(energy / 4 * (1 + (2 * i / (float)(Nx - 1) - 1)));
            xm = (UInt16)(energy / 4 * (1 - (2 * i / (float)(Nx - 1) - 1)));
            yp = (UInt16)(energy / 4 * (1 + (2 * j / (float)(Ny - 1) - 1)));
            ym = (UInt16)(energy / 4 * (1 - (2 * j / (float)(Ny - 1) - 1)));
        }

        // Recalculate i to X+
        public static UInt16 XPlusForI(int i)
        {
            // corner case1: i = 0    =>  X = -1  => X+ = 0, X- - any
            if (i == 0)
                return 0;

            ////			// corner case2: i = Nx-1 =>  X = +1  => X- = 

            // X in [-1, +1]
            float X = 2.0f * (float)i / (float)(Nx - 1);

            if (X == 1.0f)
            {
                // X+ can be anything, so we just take the quater of max value
                return 16384;
            }

            // X+ / X- = (1 + X) / (1 - X)   <= we need to balance X+ and X-

            // float D = (1 + X) / (1 - X);

            return (UInt16)X;
        }

        // Recalculate i to X-
        public static UInt16 XMinusForI(int i)
        {
            // corner case1: i = 0    =>  X = -1  => X+ = 0, X- - any
            if (i == 0)
                return 16384;

            ////			// corner case2: i = Nx-1 =>  X = +1  => X- = 

            // X in [-1, +1]
            float X = 2.0f * (float)i / (float)(Nx - 1);

            if (X == 1.0f)
            {
                // X+ can be anything, so we just take 0 as X-
                return 0;
            }

            // X+ / X- = (1 + X) / (1 - X)   <= we need to balance X+ and X-

            // float D = (1 + X) / (1 - X);

            return (UInt16)X;
        }

        // Recalculate j to Y+
        public static UInt16 YMinusForJ(int j)
        {
            // corner case1: j = 0    =>  Y = -1  => Y+ = 0, Y- - any
            if (j == 0)
                return 0;

            ////			// corner case2: j = Ny-1 =>  Y = +1  => Y- = 

            // Y in [-1, +1]
            float Y = 2.0f * (float)j / (float)(Ny - 1);

            if (Y == 1.0f)
            {
                // Y+ can be anything, so we just take the quater of max value
                return 16384;
            }

            // Y+ / Y- = (1 + Y) / (1 - Y)   <= we need to balance Y+ and Y-

            // float D = (1 + Y) / (1 - Y);

            return (UInt16)Y;
        }

        // Recalculate j to Y-
        public static UInt16 YPlusForJ(int j)
        {
            // corner case1: j = 0    =>  Y = -1  => Y+ = 0, Y- - any
            if (j == 0)
                return 16384;

            ////			// corner case2: j = Ny-1 =>  Y = +1  => Y- = 

            // Y in [-1, +1]
            float Y = 2.0f * (float)j / (float)(Ny - 1);

            if (Y == 1.0f)
            {
                // Y+ can be anything, so we just take 0 as Y-
                return 0;
            }

            // Y+ / Y- = (1 + Y) / (1 - Y)   <= we need to balance Y+ and Y-

            // float D = (1 + Y) / (1 - Y);

            return (UInt16)Y;
        }

        public static void CalculateCoincidenceTOFForDigital(PETDigitalCoincidenceTOF In, ref PETCoincidenceTOF OutE)
        {
            OutE.XPlus1 = XPlusForI(In.I1);
            OutE.XMinus1 = XMinusForI(In.I1);
            OutE.XPlus2 = XPlusForI(In.I2);
            OutE.XMinus2 = XMinusForI(In.I2);

            OutE.YPlus1 = YPlusForJ(In.J1);
            OutE.YMinus1 = YMinusForJ(In.J1);
            OutE.YPlus2 = YPlusForJ(In.J2);
            OutE.YMinus2 = YMinusForJ(In.J2);

            OutE.Position1 = In.Position1;
            OutE.Position2 = In.Position2;

            OutE.Timestamp1 = In.Timestamp1;
            OutE.Timestamp2 = In.Timestamp2;

            // OutE.Flags = In.Flags;
        }

        public static void CalculateCoincidenceForDigital(PETDigitalCoincidence In, ref PETCoincidence OutE)
        {
            OutE.XPlus1 = XPlusForI(In.I1);
            OutE.XMinus1 = XMinusForI(In.I1);
            OutE.XPlus2 = XPlusForI(In.I2);
            OutE.XMinus2 = XMinusForI(In.I2);

            OutE.YPlus1 = YPlusForJ(In.J1);
            OutE.YMinus1 = YMinusForJ(In.J1);
            OutE.YPlus2 = YPlusForJ(In.J2);
            OutE.YMinus2 = YMinusForJ(In.J2);

            OutE.Position1 = In.Position1;
            OutE.Position2 = In.Position2;

            OutE.Timestamp = In.Timestamp;

            // OutE.Flags = In.Flags;
        }

        public static void CalculateEventForDigital(PETSingleDigitalEvent In, ref PETSingleEvent OutE)
        {
            OutE.XPlus = XPlusForI(In.I);
            OutE.XMinus = XMinusForI(In.I);

            OutE.YPlus = YPlusForJ(In.J);
            OutE.YMinus = YMinusForJ(In.J);

            OutE.Position = In.Position;
            OutE.Timestamp = In.Timestamp;

            // OutE.Flags = In.Flags;
        }
        #endregion
    }
}
