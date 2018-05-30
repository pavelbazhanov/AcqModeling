using System;
using System.Collections.Generic;
using System.Drawing;

namespace AcqModeling
{
    public class Sinogram
    {
        private Sinogram() { }
        public Sinogram(int numSamples, AnglesDistribution angles)
        {
            anglesDistribution = angles;
            data = new double[angles.Count, numSamples];
        }
        public Sinogram(double[,] data, AnglesDistribution angles)
        {
            if (data.GetLength(0) != angles.Count) throw new ArgumentException();
            anglesDistribution = angles;
            this.data = data;
        }


        private double[,] data;
        private AnglesDistribution anglesDistribution;

        public int NumSamples { get { return data.GetLength(1); } }
        public double this[int angleIndex, int sampleIndex]
        {
            get { return data[angleIndex, sampleIndex]; }
            set { data[angleIndex, sampleIndex] = value; }
        }
        public AnglesDistribution AnglesDistribution { get { return anglesDistribution; } }

        public double[] GetProjection(int angleIndex)
        {
            double[] projection = new double[NumSamples];

            for (int i = 0; i < projection.Length; i++)
                projection[i] = this[angleIndex, i];

            return projection;
        }
        public void SetProjection(int angleIndex, double[] projection)
        {
            for (int i = 0; i < projection.Length; i++)
                this[angleIndex, i] = projection[i];
        }
        public Sinogram Clone()
        {
            Sinogram res = new Sinogram();

            res.data = (double[,])data.Clone();
            res.anglesDistribution = anglesDistribution.Clone();

            return res;
        }

        public static int GetSinogramSizeInBytes(Sinogram sino)
        {
            // +8 - это NumSamples и NumPhi 
            return 8 * sino.NumSamples * sino.AnglesDistribution.Count + 8;
        }

        public static void SaveSinogramToBytes(byte[] b, int ofs, Sinogram sino)
        {
            int Offset = ofs;

            int numSamples = sino.NumSamples;
            int numPhi = sino.AnglesDistribution.Count;

            // 1. кол-во сэмплов по S 
            BitUtils.ToBytes32((uint)numSamples, b, Offset + 0);
            // 2. кол-во направлений 
            BitUtils.ToBytes32((uint)numPhi, b, Offset + 4);
            // 3. данные синограммы, doubles 
            Offset += 8;

            for (int j = 0; j < numPhi; j++)
            {
                for (int i = 0; i < numSamples; i++)
                {
                    BitUtils.ToDouble(sino[j, i], b, Offset);
                    Offset += 8;
                }
            }
        }

        public static void SaveSinogramToStream(System.IO.Stream s, Sinogram sino)
        {
            int sz = Sinogram.GetSinogramSizeInBytes(sino);

            var outBytes = new byte[sz];

            SaveSinogramToBytes(outBytes, 0, sino);

            s.Write(outBytes, 0, outBytes.Length);
        }

        public static void DumpSinogram(double[,] sin, string filename)
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            for (int i = 0; i < sin.GetLength(0); i++)
                for (int j = 0; j < sin.GetLength(1); j++)
                {
                    if (max < sin[i, j]) { max = sin[i, j]; } // max = Math.Max(max, sin[i, j]); 
                    if (min > sin[i, j]) { min = sin[i, j]; } // min = Math.Min(min, sin[i, j]); 
                }

            double scale = 255.0 / (max - min);


            Bitmap bmp = new Bitmap(sin.GetLength(0), sin.GetLength(1));

            unsafe
            {
                var bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                byte* ptr = (byte*)bd.Scan0;
                for (int j = 0; j < bmp.Height; j++)
                {
                    for (int i = 0; i < bmp.Width; i++)
                    {
                        int val = (int)((sin[i, j] - min) * scale);

                        if (val < 0) { val = 0; }
                        if (val > 255) { val = 255; }

                        byte b = (byte)val;

                        ptr[i * 4 + 0] = b;
                        ptr[i * 4 + 1] = b;
                        ptr[i * 4 + 2] = b;
                        ptr[i * 4 + 3] = 255;
                    }

                    ptr += bd.Stride;
                }

                bmp.UnlockBits(bd);
            }
            /* 
            for (int i = 0; i < bmp.Width; i++) 
            { 
            for (int j = 0; j < bmp.Height; j++) 
            { 
            int val = (int)((sin[i, j] - min) * scale); 

            if (val < 0) { val = 0; } 
            if (val > 255) { val = 255; } 
            Color c = Color.FromArgb(val, val, val); 
            bmp.SetPixel(i, j, c); 
            } 
            } 
            */
            bmp.Save(filename);
        }
    }
}
