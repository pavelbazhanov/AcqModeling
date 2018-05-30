using System;
namespace AcqModeling
{
    public class DetectorsConfiguration
    {
        public int RingsCount { get; set; }
        public double RingRadius { get; set; }
        public int BlocksCount { get; set; }
        public int DetectorsPerBlock { get; set; }
        public double DetectorSide { get; set; }
        public double BlockSize { get; set; }

        public static DetectorsConfiguration BaseConfiguration = new DetectorsConfiguration()
        {
            RingsCount = 4,
            RingRadius = 410,
            BlocksCount = 48,
            DetectorsPerBlock = 15,
            BlockSize = 49.9,
            DetectorSide = 3
        };

        public bool Detect(Photon ph, out PETSingleEvent se)
        {
            se = new PETSingleEvent();

            if (ph.Status != stat.Finished) return false;

            //to cylindrical
            double azimuthalAngle = Math.Atan2(ph.CurrentPosition.Y, ph.CurrentPosition.X);
            if (azimuthalAngle < 0)
                azimuthalAngle += 2 * Math.PI;
            //double rc = Math.Sqrt(ph.CurrentPosition.X * ph.CurrentPosition.X + ph.CurrentPosition.Y * ph.CurrentPosition.Y + ph.CurrentPosition.Z * ph.CurrentPosition.Z) * Math.Sin(ph.Theta);
            double rc = Math.Sqrt(ph.CurrentPosition.X * ph.CurrentPosition.X + ph.CurrentPosition.Y * ph.CurrentPosition.Y);
            double z0 = ph.CurrentPosition.Z;

            var tmpAngle = Math.Asin(rc / this.RingRadius * Math.Sin(azimuthalAngle - ph.Phi));
            //var tmpAngle = this.Phi - azimuthalAngle - Math.Asin(rc / ringRadius * Math.Sin(azimuthalAngle - this.Phi));

            var psi = tmpAngle + ph.Phi; //угол регистрации
            if (psi < 0) psi += 2 * Math.PI;
            if (psi >= 2 * Math.PI) psi -= 2 * Math.PI;

            var block = (int)(psi / (2 * Math.PI) * this.BlocksCount); // номер блока детекторов

            var r1 = Math.Sqrt(this.RingRadius * this.RingRadius + rc * rc - 2 * this.RingRadius * rc * Math.Cos(azimuthalAngle - psi));
            var dz = (r1 / Math.Tan(ph.Theta)) / this.BlockSize;
            z0 /= this.BlockSize;

            var blockRing = (int)Math.Floor(z0 + dz + this.RingsCount / 2.0); // номер кольца блоков
            //double s = Math.Sqrt((this.CurrentPosition.X - ringRadius * Math.Cos(psi)) * (this.CurrentPosition.X - ringRadius * Math.Cos(psi)) +
            //    (this.CurrentPosition.Y - ringRadius * Math.Sin(psi)) * (this.CurrentPosition.Y - ringRadius * Math.Sin(psi)) +
            //        (this.CurrentPosition.Z - dz * blockSide) * (this.CurrentPosition.Z - dz * blockSide)) + this.Path;
            //this.Time += s / (3e11);

            if (blockRing >= 0 && blockRing < this.RingsCount)
            {
                se.Position = (UInt16)(((block & 0x3F) << 4) | (blockRing & 0xF));
                se.Timestamp = (UInt32)(ph.Time);

                var I = (byte)((psi - block * 2 * Math.PI / this.BlocksCount) / (2 * Math.PI / (this.BlocksCount * this.DetectorsPerBlock)));
                var J = (byte)((z0 + dz + this.RingsCount / 2.0) * this.DetectorsPerBlock) % this.DetectorsPerBlock;

                EventConverter.GetXYFromIJ(I, (byte)J, ph.Energy, out se.XPlus, out se.XMinus, out se.YPlus, out se.YMinus);
                return true;
            }
            return false;
        }
    }
}
