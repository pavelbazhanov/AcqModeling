using System;

namespace AcqModeling
{
    [Serializable]
    public struct Vector3d
    {
        public Vector3d(double V)
            : this()
        {
            X = Y = Z = V;
        }

        public Vector3d(double x, double y, double z)
            : this()
        {
            X = x; Y = y; Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        public Vector3d Floor
        {
            get { return new Vector3d(Math.Floor(X), Math.Floor(Y), Math.Floor(Z)); }
        }

        public Vector3d Normalized
        {
            get { return new Vector3d(X / Length, Y / Length, Z / Length); }
        }

        public static Vector3d operator -(Vector3d vector)
        {
            return new Vector3d(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector3d operator +(Vector3d left, Vector3d right)
        {
            return new Vector3d(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3d operator -(Vector3d left, Vector3d right)
        {
            return new Vector3d(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3d operator *(Vector3d vector, double multiplier)
        {
            return new Vector3d(vector.X * multiplier, vector.Y * multiplier, vector.Z * multiplier);
        }
        public static Vector3d operator *(double multiplier, Vector3d vector)
        {
            return new Vector3d(vector.X * multiplier, vector.Y * multiplier, vector.Z * multiplier);
        }
        public static Vector3d operator /(Vector3d vector, double divider)
        {
            return new Vector3d(vector.X / divider, vector.Y / divider, vector.Z / divider);
        }

        /// <summary>
        /// Dot product of two vectors
        /// </summary>
        public static double operator *(Vector3d left, Vector3d right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static bool operator ==(Vector3d left, Vector3d right)
        {
            return left.equals(right);
        }
        public static bool operator !=(Vector3d left, Vector3d right)
        {
            return !left.equals(right);
        }

        public Vector3d Cross(Vector3d Vec)
        {
            return new Vector3d(Y * Vec.Z - Z * Vec.Y, Z * Vec.X - X * Vec.Z, X * Vec.Y - Y * Vec.X);
        }

        public static Vector3d Empty
        {
            get { return new Vector3d(0, 0, 0); }
        }

        public void Normalize()
        {
            double L = Length;

            if (L > 0.000001)
            {
                X /= L;
                Y /= L;
                Z /= L;
            }
        }

        public double this[int idx]
        {
            get
            {
                if (idx == 0) { return X; }
                if (idx == 1) { return Y; }
                if (idx == 2) { return Z; }
                return 0;
            }
            set
            {
                if (idx == 0) { X = value; }
                if (idx == 1) { Y = value; }
                if (idx == 2) { Z = value; }
            }
        }

        /// <summary>
        /// Два ортогональных вектора, дополняющих данный до базиса
        /// </summary>
        /// <param name="N"></param>
        /// <param name="V1"></param>
        /// <param name="V2"></param>
        public static void BuildComplementaryBasis(Vector3d N, out Vector3d V1, out Vector3d V2)
        {
            V1 = N.Cross(new Vector3d(1, 0, 0));

            if (V1.Length <= 0.000001f)
            {
                V1 = N.Cross(new Vector3d(0, 1, 0));

                if (V1.Length <= 0.000001f)
                {
                    V1 = N.Cross(new Vector3d(0, 0, 1));
                }
            }

            V2 = N.Cross(V1);

            V1.Normalize();
            V2.Normalize();
        }

        public static Vector3d Lerp(Vector3d v1, Vector3d v2, double p)
        {
            return v1 * (1 - p) + v2 * p;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3d)
                return equals((Vector3d)obj);
            else return false;
        }
        public bool equals(Vector3d compared)
        {

            return X == compared.X && Y == compared.Y && Z == compared.Z;
        }
        public override int GetHashCode()
        {
            return (int)(X + 0xFFFF * Y + 0xFFFFFF * Z); // -_-
        }
        public override string ToString()
        {
            return "{" + X + ";" + Y + ";" + Z + "}";
        }
    }
}
