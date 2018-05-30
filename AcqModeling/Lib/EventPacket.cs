using System;
using System.Collections.Generic;

/**
	Для реализации низкоуровневого обмена с драйверами ИПАК/БГР в различных режимах
	определяются следующие структуры данных:

		1) Структура PETSingleEvent, содержащая информацию о единичном событии детектирования

		2) Структура, содержащая информацию о совпадении (без TOF)

		3) Структура, содержащая информацию о совпадении (с TOF)

		4) Структура PETEventPacket с массивом PETSingleEvent - коллекция единичных событий

		4) Структура PETCoincidencePacket с массивом PETCoincidence - коллекция событий совпадения (без TOF)

		4) Структура PETCoincidenceTOFPacket с массивом PETCoincidenceTOF - коллекция событий совпадения (с TOF)

	Для структур 4-6 базовой является PETCommonPacket (с общим заголовком UDP-пакета).

	Все определённые структуры поддерживают преобразование в массивы байтов (для передачи по сети или сохранения в файл)
*/

namespace AcqModeling
{
    public static class PETStructSizes
    {
        public static readonly int PETCoincidence_Size = 25;
        public static readonly int PETCoincidenceTOF_Size = 29;
        public static readonly int PETSingleEvent_Size = 15;

        public static readonly int PETSingleDigitalEvent_Size = 8;
        public static readonly int PETDigitalCoincidence_Size = 12;
    }

    public class PETCommonPacket
    {
        public byte Flags;
        public byte ItemCount;

        public PETCommonPacket()
        {
        }
    }

    /// Collection of non-TOF PET coincidences
    public class PETCoincidencePacket : PETCommonPacket
    {
        public PETCoincidencePacket()
        {
        }

        public PETCoincidence[] Coincidences;

        //static int ItemSize = PETCoincidence.Size; //Marshal.SizeOf(typeof(PETCoincidence));

        public static PETCoincidencePacket RandomPacket(System.Random rnd = null)
        {
            var res = new PETCoincidencePacket();

            var r = (rnd == null) ? new System.Random() : rnd;

            res.Flags = (byte)r.Next();
            res.ItemCount = (byte)r.Next(1, 10);

            res.Coincidences = new PETCoincidence[res.ItemCount];

            for (int i = 0; i < res.ItemCount; i++)
                res.Coincidences[i] = PETCoincidence.RandomCoincidence(rnd);

            return res;
        }

        public bool CompareTo(PETCoincidencePacket p)
        {
            if ((p.ItemCount != ItemCount) || (p.Flags != Flags))
                return false;

            for (int i = 0; i < ItemCount; i++)
                if (!this.Coincidences[i].CompareTo(p.Coincidences[i]))
                    return false;

            return true;
        }

        public int GetSize()
        {
            return 2 + ItemCount * PETStructSizes.PETCoincidence_Size; // 25/*ItemSize*/;
        }

        public void FromBytes(int Offset, byte[] Src)
        {
            // 1. read common header
            Flags = Src[Offset + 0];
            ItemCount = Src[Offset + 1];

            Coincidences = new PETCoincidence[ItemCount];

            // 2. allocate and read coincidences
            for (int i = 0; i < ItemCount; i++)
            {
                // Coincidences[i] = new PETCoincidence(); // ?
                Coincidences[i].FromBytes(Offset + 2 + i * PETStructSizes.PETCoincidence_Size, Src); //ItemSize, Src);
            }
        }

        public void ToBytes(int Offset, byte[] Dest)
        {
            Dest[Offset + 0] = Flags;
            Dest[Offset + 1] = ItemCount;

            for (int i = 0; i < ItemCount; i++)
                Coincidences[i].ToBytes(Offset + 2 + i * PETStructSizes.PETCoincidence_Size, Dest); //ItemSize, Dest);
        }

        public static int GetArraySizeInBytes(PETCoincidencePacket[] Packets, bool UseSizeInfo = false)
        {
            int TotalSize = 0;

            for (int i = 0; i < Packets.Length; i++)
            {
                TotalSize += Packets[i].GetSize();

                if (UseSizeInfo)
                    TotalSize += 2;
            }

            return TotalSize;
        }

        /// <summary>
        /// Encode a list of packets to the byte array (AddSizeInfo shows if we have to store packet sizes)
        /// </summary>
        public static bool EncodeArray(PETCoincidencePacket[] Packets, int Offset, byte[] Dest, bool AddSizeInfo = false)
        {
            // 1. calculate the size
            int TotalSize = GetArraySizeInBytes(Packets);

            if (TotalSize > Dest.Length)
                return false;

            // 2. allocate array
            int Ofs = 0;

            for (int i = 0; i < Packets.Length; i++)
            {
                if (AddSizeInfo)
                {
                    BitUtils.ToBytes16((UInt16)Packets[i].GetSize(), Dest, Ofs);

                    Ofs += 2;
                }

                Packets[i].ToBytes(Offset + Ofs, Dest);

                Ofs += Packets[i].GetSize();
            }

            return true;
        }

        /// <summary>
        /// Decode byte[] array to a list of Coincidence[]
        /// </summary>
        public static PETCoincidencePacket[] DecodeArray(int NumPackets, int Offset, byte[] Src, bool UseSizeInfo = false)
        {
            int Ofs = 0;

            var res = new PETCoincidencePacket[NumPackets];

            for (int i = 0; i < NumPackets; i++)
            {
                if (UseSizeInfo)
                {
                    // skip two bytes
                    Ofs += 2;
                }

                var pack = new PETCoincidencePacket();
                pack.FromBytes(Offset + Ofs, Src);

                res[i] = pack;

                Ofs += pack.GetSize();
            }

            return res;
        }

        #region Группировка наборов совпадений в пакеты и "разгруппировка" массива пакетов обратно в массивы совпадений

        public static PETCoincidence[] UnpackCoincidencePacketList(PETCoincidencePacket[] packets)
        {
            var res = new List<PETCoincidence>();

            foreach (var p in packets)
            {
                foreach (var c in p.Coincidences)
                {
                    res.Add(c);
                }
            }

            return res.ToArray();
        }

        public static PETCoincidencePacket[] PackCoincidencesToList(PETCoincidence[] coincidences, int CoincPerPacket)
        {
            var res = new List<PETCoincidencePacket>();

            int num = coincidences.Length;
            int i = 0;
            while (i < num)
            {
                // сколько совпадений в этом пакете (не более CoincPerPacket, но в конце меньше)
                int cnt = Math.Min(num - i, CoincPerPacket);

                // начинаем новый пакет
                var p = new PETCoincidencePacket();
                p.ItemCount = (byte)cnt;
                p.Flags = 0;

                p.Coincidences = new PETCoincidence[cnt];

                for (int j = 0; j < cnt; j++)
                    p.Coincidences[j] = coincidences[i + j];

                i += CoincPerPacket;

                res.Add(p);
            }

            return res.ToArray();
        }

        #endregion
    }

    /// Collection of TOF PET coincidences
    public class PETCoincidenceTOFPacket : PETCommonPacket
    {
        public PETCoincidenceTOFPacket()
        {
        }

        public PETCoincidenceTOF[] Coincidences;

        //		static int ItemSize = PETCoincidenceTOF.Size; // Marshal.SizeOf(typeof(PETCoincidenceTOF));
        public int GetSize()
        {
            return 2 + ItemCount * PETStructSizes.PETCoincidenceTOF_Size; // 25/*ItemSize*/;
        }

        public void FromBytes(int Offset, byte[] Src)
        {
            // 1. read common header
            Flags = Src[Offset + 0];
            ItemCount = Src[Offset + 1];

            Coincidences = new PETCoincidenceTOF[ItemCount];

            // 2. allocate and read coincidences
            for (int i = 0; i < ItemCount; i++)
            {
                // Coincidences[i] = new PETCoincidenceTOF(); // ?
                Coincidences[i].FromBytes(Offset + 2 + i * PETStructSizes.PETCoincidenceTOF_Size, Src);// ItemSize, Src);
            }
        }

        public void ToBytes(int Offset, byte[] Dest)
        {
            Dest[Offset + 0] = Flags;
            Dest[Offset + 1] = ItemCount;

            for (int i = 0; i < ItemCount; i++)
                Coincidences[i].ToBytes(Offset + 2 + i * PETStructSizes.PETCoincidenceTOF_Size, Dest);
        }

        public static int GetArraySizeInBytes(PETCoincidenceTOFPacket[] Packets, bool UseSizeInfo = false)
        {
            int TotalSize = 0;

            for (int i = 0; i < Packets.Length; i++)
            {
                TotalSize += Packets[i].GetSize();

                if (UseSizeInfo)
                    TotalSize += 2;
            }

            return TotalSize;
        }

        /// <summary>
        /// Encode a list of packets to the byte array (AddSizeInfo shows if we have to store packet sizes)
        /// </summary>
        public static bool EncodeArray(PETCoincidenceTOFPacket[] Packets, int Offset, byte[] Dest, bool AddSizeInfo = false)
        {
            // 1. calculate the size
            int TotalSize = GetArraySizeInBytes(Packets);

            if (TotalSize > Dest.Length)
                return false;

            // 2. allocate array
            int Ofs = 0;

            for (int i = 0; i < Packets.Length; i++)
            {
                if (AddSizeInfo)
                {
                    BitUtils.ToBytes16((UInt16)Packets[i].GetSize(), Dest, Ofs);

                    Ofs += 2;
                }

                Packets[i].ToBytes(Offset + Ofs, Dest);

                Ofs += Packets[i].GetSize();
            }

            return true;
        }

        /// <summary>
        /// Decode byte[] array to a list of Coincidence[]
        /// </summary>
        public static PETCoincidenceTOFPacket[] DecodeArray(int NumPackets, int Offset, byte[] Src, bool UseSizeInfo = false)
        {
            int Ofs = 0;

            var res = new PETCoincidenceTOFPacket[NumPackets];

            for (int i = 0; i < NumPackets; i++)
            {
                if (UseSizeInfo)
                {
                    // skip two bytes
                    Ofs += 2;
                }

                var pack = new PETCoincidenceTOFPacket();
                pack.FromBytes(Offset + Ofs, Src);

                res[i] = pack;

                Ofs += pack.GetSize();
            }

            return res;
        }

        #region Группировка наборов ивентов в пакеты и "разгруппировка" массива пакетов обратно в массивы ивентов

        public static PETCoincidenceTOF[] UnpackCoincidenceTOFPacketList(PETCoincidenceTOFPacket[] packets)
        {
            var res = new List<PETCoincidenceTOF>();

            foreach (var p in packets)
            {
                foreach (var s in p.Coincidences)
                {
                    res.Add(s);
                }
            }

            return res.ToArray();
        }

        #endregion

    }

    /// Collection of PET single events
    public class PETSingleEventsPacket : PETCommonPacket
    {
        public PETSingleEventsPacket()
        {
        }

        public PETSingleEvent[] Events;

        //		static int ItemSize = PETSingleEvent.Size; // Marshal.SizeOf(typeof(PETSingleEvent));

        public static PETSingleEventsPacket RandomPacket(System.Random rnd = null)
        {
            var res = new PETSingleEventsPacket();

            var r = (rnd == null) ? new System.Random() : rnd;

            res.Flags = (byte)r.Next();
            res.ItemCount = (byte)r.Next(1, 10);

            res.Events = new PETSingleEvent[res.ItemCount];

            for (int i = 0; i < res.ItemCount; i++)
                res.Events[i] = PETSingleEvent.RandomEvent(rnd);

            return res;
        }

        public bool CompareTo(PETSingleEventsPacket p)
        {
            if ((p.ItemCount != ItemCount) || (p.Flags != Flags))
                return false;

            for (int i = 0; i < ItemCount; i++)
                if (!this.Events[i].CompareTo(p.Events[i]))
                    return false;

            return true;
        }

        public void FromBytes(int Offset, byte[] Src)
        {
            // 1. read common header
            Flags = Src[Offset + 0];
            ItemCount = Src[Offset + 1];

            Events = new PETSingleEvent[ItemCount];

            // 2. allocate and read events
            for (int i = 0; i < ItemCount; i++)
            {
                // Events[i] = new PETSingleEvent(); // ?
                Events[i].FromBytes(Offset + 2 + i * PETStructSizes.PETSingleEvent_Size, Src);
            }
        }

        public void ToBytes(int Offset, byte[] Dest)
        {
            Dest[Offset + 0] = Flags;
            Dest[Offset + 1] = ItemCount;

            for (int i = 0; i < ItemCount; i++)
                Events[i].ToBytes(Offset + 2 + i * PETStructSizes.PETSingleEvent_Size, Dest);
        }

        public int GetSize()
        {
            return 2 + ItemCount * PETStructSizes.PETSingleEvent_Size; // ItemSize;
        }

        #region Упаковка массива пакетов в массив байтов

        public static int GetArraySizeInBytes(PETSingleEventsPacket[] Packets, bool UseSizeInfo = false)
        {
            int TotalSize = 0;

            for (int i = 0; i < Packets.Length; i++)
            {
                TotalSize += Packets[i].GetSize();

                if (UseSizeInfo)
                    TotalSize += 2;
            }

            return TotalSize;
        }

        /// <summary>
        /// Encode a list of packets to the byte array (AddSizeInfo shows if we have to store packet sizes)
        /// </summary>
        public static bool EncodeArray(PETSingleEventsPacket[] Packets, int Offset, byte[] Dest, bool AddSizeInfo = false)
        {
            // 1. calculate the size
            int TotalSize = GetArraySizeInBytes(Packets);

            if (TotalSize > Dest.Length)
                return false;

            // 2. allocate array
            int Ofs = 0;

            for (int i = 0; i < Packets.Length; i++)
            {
                if (AddSizeInfo)
                {
                    BitUtils.ToBytes16((UInt16)Packets[i].GetSize(), Dest, Ofs);

                    Ofs += 2;
                }

                Packets[i].ToBytes(Offset + Ofs, Dest);

                Ofs += Packets[i].GetSize();
            }

            return true;
        }

        /// <summary>
        /// Decode byte[] array to a list of SingleEvent[]
        /// </summary>
        public static PETSingleEventsPacket[] DecodeArray(int NumPackets, int Offset, byte[] Src, bool UseSizeInfo = false)
        {
            int Ofs = 0;

            var res = new PETSingleEventsPacket[NumPackets];

            for (int i = 0; i < NumPackets; i++)
            {
                if (UseSizeInfo)
                {
                    // skip two bytes
                    Ofs += 2;
                }

                var pack = new PETSingleEventsPacket();
                pack.FromBytes(Offset + Ofs, Src);

                res[i] = pack;

                Ofs += pack.GetSize();
            }

            return res;
        }

        #endregion

        #region Группировка наборов ивентов в пакеты и "разгруппировка" массива пакетов обратно в массивы ивентов

        public static PETSingleEvent[] UnpackEventsPacketList(PETSingleEventsPacket[] packets)
        {
            var res = new List<PETSingleEvent>();

            foreach (var p in packets)
            {
                foreach (var s in p.Events)
                {
                    res.Add(s);
                }
            }

            return res.ToArray();
        }

        public static PETSingleEventsPacket[] PackEventsToList(PETSingleEvent[] events, int EventsPerPacket)
        {
            var res = new List<PETSingleEventsPacket>();

            int num = events.Length;
            int i = 0;
            while (i < num)
            {
                // сколько совпадений в этом пакете (не более EventsPerPacket, но в конце меньше)
                int cnt = Math.Min(num - i, EventsPerPacket);

                // начинаем новый пакет
                var p = new PETSingleEventsPacket();
                p.ItemCount = (byte)cnt;
                p.Flags = 0;

                p.Events = new PETSingleEvent[cnt];

                for (int j = 0; j < cnt; j++)
                    p.Events[j] = events[i + j];

                i += EventsPerPacket;

                res.Add(p);
            }

            return res.ToArray();
        }

        #endregion
    }

    public class PETPacketGenerator
    {
        // один пакет с одним совпадением на детекторах 1 и 2, всё остальное по нулям
        public static PETCoincidencePacket[] CreateUnitCoincidencePacket()
        {
            var coinc = new PETCoincidence();
            coinc.Position1 = 1;
            coinc.Position2 = 2;
            coinc.Flags = 13;
            coinc.XPlus1 = 5;
            coinc.XMinus1 = 6;
            coinc.YPlus1 = 7;
            coinc.YMinus1 = 8;
            coinc.XPlus2 = 9;
            coinc.XMinus2 = 10;
            coinc.YPlus2 = 11;
            coinc.YMinus2 = 12;
            coinc.Timestamp = 255;

            var packet = new PETCoincidencePacket();
            packet.Flags = 63;
            packet.ItemCount = 1;
            packet.Coincidences = new PETCoincidence[1];
            packet.Coincidences[0] = coinc;

            return new PETCoincidencePacket[] { packet };
        }

        // один пакет с одним ивентом на детекторе 1
        public static PETSingleEventsPacket[] CreateUnitEventPacket()
        {
            var evt = new PETSingleEvent();
            evt.Flags = 1;
            evt.Position = 2;
            evt.XMinus = 3;
            evt.XPlus = 4;
            evt.YMinus = 5;
            evt.YPlus = 6;
            evt.Timestamp = 255;

            var packet = new PETSingleEventsPacket();
            packet.Flags = 63;
            packet.ItemCount = 1;
            packet.Events = new PETSingleEvent[1];
            packet.Events[0] = evt;

            return new PETSingleEventsPacket[] { packet };
        }

        public static PETCoincidencePacket[] CreateRandomCoincidencePackets(int NumPackets)
        {
            var res = new PETCoincidencePacket[NumPackets];

            var rnd = new System.Random();

            // Generate random coincidences and pack the to packets
            for (int i = 0; i < NumPackets; i++)
                res[i] = PETCoincidencePacket.RandomPacket(rnd);

            return res;
        }

        public static PETSingleEventsPacket[] CreateRandomPackets(int NumPackets)
        {
            var res = new PETSingleEventsPacket[NumPackets];

            var rnd = new System.Random();

            // Generate random single events and pack the to packets
            for (int i = 0; i < NumPackets; i++)
                res[i] = PETSingleEventsPacket.RandomPacket(rnd);

            return res;
        }

        public static bool CompareEventPacketArray(PETSingleEventsPacket[] packets1, PETSingleEventsPacket[] packets2)
        {
            if (packets1 == null || packets2 == null)
            {
                Console.WriteLine("One of arrays is null");
                return false;
            }

            if (packets1.Length != packets2.Length)
            {
                Console.WriteLine("Packet array sizes differ");
                return false;
            }

            for (int i = 0; i < packets1.Length; i++)
            {
                if (!packets1[i].CompareTo(packets2[i]))
                {
                    Console.WriteLine("Packets differ at " + i);
                    return false;
                }
            }

            return true;
        }

        public static bool CompareCoincidencePacketArray(PETCoincidencePacket[] packets1, PETCoincidencePacket[] packets2)
        {
            if (packets1 == null || packets2 == null)
            {
                Console.WriteLine("One of arrays is null");
                return false;
            }

            if (packets1.Length != packets2.Length)
            {
                Console.WriteLine("Packet array sizes differ");
                return false;
            }

            for (int i = 0; i < packets1.Length; i++)
            {
                if (!packets1[i].CompareTo(packets2[i]))
                {
                    Console.WriteLine("Packets differ at " + i);
                    return false;
                }
            }

            return true;
        }
    }
}
