using System;

namespace AcqModeling
{
    public static class Statistics
    {
        private static Random random = new Random();

        public static long GetPoisson(double lambda)
        {
            if (lambda > 50) return GetNormal(lambda, lambda);
            double L = Math.Exp(-lambda);
            long k = 0;
            double p = 1;
            do
            {
                k++;
                p = p * random.NextDouble();
            } while (p >= L);
            return k - 1;
        }

        public static long GetNormal(double mean, double variance)
        {
            double sum = 0;
            for (int i = 0; i < 12; i++)
                sum += random.NextDouble();
            sum -= 6;
            return (long)(sum * Math.Sqrt(variance) + mean);
        }
    }
}
