using System;

namespace AcqModeling
{
    public class BitUtils
    {
        public static void DoubleArray4DToUInt16Bytes(double[, , ,] data, byte[] b, int ofs)
        {
            int d0 = data.GetLength(0);
            int d1 = data.GetLength(1);
            int d2 = data.GetLength(2);
            int d3 = data.GetLength(3);

            int Offset = ofs;

            for (int i0 = 0; i0 < d0; i0++)
            {
                for (int i1 = 0; i1 < d1; i1++)
                {
                    for (int i2 = 0; i2 < d2; i2++)
                    {
                        for (int i3 = 0; i3 < d3; i3++)
                        {
                            BitUtils.ToBytes16((UInt16)data[i0, i1, i2, i3], b, Offset);
                            Offset += 2;
                        }
                    }
                }
            }
        }

        public static void DoubleArray4DFromUInt16Bytes(double[, , ,] data, byte[] b, int ofs)
        {
            int d0 = data.GetLength(0);
            int d1 = data.GetLength(1);
            int d2 = data.GetLength(2);
            int d3 = data.GetLength(3);

            int Offset = ofs;

            for (int i0 = 0; i0 < d0; i0++)
            {
                for (int i1 = 0; i1 < d1; i1++)
                {
                    for (int i2 = 0; i2 < d2; i2++)
                    {
                        for (int i3 = 0; i3 < d3; i3++)
                        {
                            data[i0, i1, i2, i3] = (double)BitConverter.ToUInt16(b, Offset);
                            Offset += 2;
                        }
                    }
                }
            }
        }

        public static void DoubleArray4DToBytes(double[, , ,] data, byte[] b, int ofs)
        {
            int d0 = data.GetLength(0);
            int d1 = data.GetLength(1);
            int d2 = data.GetLength(2);
            int d3 = data.GetLength(3);

            int Offset = ofs;

            for (int i0 = 0; i0 < d0; i0++)
            {
                for (int i1 = 0; i1 < d1; i1++)
                {
                    for (int i2 = 0; i2 < d2; i2++)
                    {
                        for (int i3 = 0; i3 < d3; i3++)
                        {
                            BitUtils.ToDouble(data[i0, i1, i2, i3], b, Offset);
                            Offset += 8;
                        }
                    }
                }
            }
        }

        public static void DoubleArray4DFromBytes(double[, , ,] data, byte[] b, int ofs)
        {
            int d0 = data.GetLength(0);
            int d1 = data.GetLength(1);
            int d2 = data.GetLength(2);
            int d3 = data.GetLength(3);

            int Offset = ofs;

            for (int i0 = 0; i0 < d0; i0++)
            {
                for (int i1 = 0; i1 < d1; i1++)
                {
                    for (int i2 = 0; i2 < d2; i2++)
                    {
                        for (int i3 = 0; i3 < d3; i3++)
                        {
                            data[i0, i1, i2, i3] = BitConverter.ToDouble(b, Offset);
                            Offset += 8;
                        }
                    }
                }
            }
        }

        public static void ToBytes32(UInt32 value, byte[] array, int offset)
        {
            var arr = BitConverter.GetBytes(value);
            for (int i = 0; i < 4; i++)
                array[offset + i] = arr[i];
            /*
                        GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);

                        try
                        {
                            Marshal.Copy(handle.AddrOfPinnedObject(), array, offset, 4);
                        }
                        finally
                        {
                            handle.Free();
                        }
             */
        }

        public static void ToSingle(float value, byte[] array, int offset)
        {
            var arr = BitConverter.GetBytes(value);
            for (int i = 0; i < 4; i++)
                array[offset + i] = arr[i];
        }

        public static void ToDouble(double value, byte[] array, int offset)
        {
            var arr = BitConverter.GetBytes(value);
            arr.CopyTo(array, offset);
            //for (int i = 0; i < 8; i++)
            //    array[offset + i] = arr[i];
        }

        public static void ToBytes16(UInt16 value, byte[] array, int offset)
        {
            var arr = BitConverter.GetBytes(value);
            for (int i = 0; i < 2; i++)
                array[offset + i] = arr[i];
            /*
                        GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);

                        try
                        {
                            Marshal.Copy(handle.AddrOfPinnedObject(), array, offset, 2);
                        }
                        finally
                        {
                            handle.Free();
                        }
             */
        }
    }
}
