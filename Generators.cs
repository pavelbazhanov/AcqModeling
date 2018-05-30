using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AcqModeling
{
    static class Generators
    {
        static Random rnd = new Random();
        public static List<AnnihilationEvent> GenerateEventsLinearSource(double rad, double phi, double length, ref double activity, double currentTime, double dt, double halfLife, int index)
        {
            Console.WriteLine("{0} Generating {1} events list", DateTime.Now.ToString(), index + 1);

            List<AnnihilationEvent> result = new List<AnnihilationEvent>();

            long count = Statistics.GetPoisson(activity * dt);
            double prevTime = 0;
            for (long i = 0; i < count; i++)
            {
                AnnihilationEvent ev = new AnnihilationEvent();
                //uniform sphere distribution
                ev.Phi = 2 * Math.PI * rnd.NextDouble();
                ev.Theta = Math.Acos(2 * rnd.NextDouble() - 1.0) - Math.PI;

                //uniform linear distribution
                ev.Position = new Vector3d(rad * Math.Cos(phi), rad * Math.Sin(phi), (rnd.NextDouble() - .5) * length);

                //uniform time distribution
                var v = rnd.NextDouble();
                var tm = 1 - (1 - prevTime) * Math.Pow(v, 1.0 / (count - i));
                prevTime = tm;

                ev.Time = (tm * dt + currentTime) * 1e9;

                ev.Energy = 511e3;

                result.Add(ev);
            }
            activity *= Math.Pow(2, -dt / halfLife);

            return result;
        }
        public static List<AnnihilationEvent> GenerateEventsCylSource(double rad, double length, ref double activity, double intervalTime, double dt, double halfLife, int index)
        {
            Console.WriteLine("{0} Generating {1} events list", DateTime.Now.ToString(), index + 1);

            var numEvents = Statistics.GetPoisson(activity * dt);
            var list = new List<AnnihilationEvent>();

            double prevtime = 0;

            for (long i = 0; i < numEvents; i++)
            {
                //uniform time distribution
                var v = rnd.NextDouble();
                var tm = 1 - (1 - prevtime) * Math.Pow(v, 1.0 / (numEvents - i));
                prevtime = tm;

                //uniform cyl distribution
                double phi = 2 * Math.PI * rnd.NextDouble();
                double u = rnd.NextDouble() + rnd.NextDouble();
                double R = u > 1 ? (2 - u) * rad : u * rad;
                double z = (rnd.NextDouble() - 0.5) * length;
                Vector3d position = new Vector3d(R * Math.Cos(phi), R * Math.Sin(phi), z);

                var evnt = new AnnihilationEvent(position, (intervalTime + tm * dt) * 1e9, 511E3, 2 * Math.PI * rnd.NextDouble(), Math.Acos(2 * rnd.NextDouble() - 1.0) - Math.PI);

                list.Add(evnt);
            }

            activity *= Math.Pow(2, -dt / halfLife);

            return list;
        }
        public static List<PETSingleEvent> GenerateSinglesNoAtt(List<AnnihilationEvent> eventList, DetectorsConfiguration dc, string outDir, int index)
        {
            List<PETSingleEvent> result = new List<PETSingleEvent>();

            foreach (var ev in eventList)
            {
                Photon ph1 = new Photon(ev) { Status = stat.Finished, Energy = 511E3 },
                    ph2 = new Photon(ev, true) { Status = stat.Finished, Energy = 511E3 };

                PETSingleEvent se = new PETSingleEvent();
                bool detection = dc.Detect(ph1, out se);
                if (detection)
                    result.Add(se);

                detection = dc.Detect(ph2, out se);
                if (detection)
                    result.Add(se);
            }

            if (result.Count != 0)
                WorkWithFiles.WriteSingleEventList(string.Format("{1}\\{0}.csv", index++, outDir), result);
            Console.WriteLine("\tRegistered {0} events", result.Count);

            return result;
        }
        public static List<PETCoincidenceTOF> GenerateCoincList(List<PETSingleEvent> events, string outDir, int index, int window)
        {
            List<PETCoincidenceTOF> result = new List<PETCoincidenceTOF>();

            for (int i = 0; i < events.Count-1; i++)
            {
                for (int j = i + 1; j < events.Count && (events[j].Timestamp - events[i].Timestamp < window); j++)
                    if (events[j].Timestamp - events[i].Timestamp < window && events[j].Block != events[i].Block)
                    {
                        var coinc = new PETCoincidenceTOF();

                        coinc.Timestamp1 = events[i].Timestamp;
                        coinc.Position1 = events[i].Position;
                        coinc.XMinus1 = events[i].XMinus;
                        coinc.XPlus1 = events[i].XPlus;
                        coinc.YMinus1 = events[i].YMinus;
                        coinc.YPlus1 = events[i].YPlus;
                        coinc.Timestamp2 = events[j].Timestamp;
                        coinc.Position2 = events[j].Position;
                        coinc.XMinus2 = events[j].XMinus;
                        coinc.XPlus2 = events[j].XPlus;
                        coinc.YMinus2 = events[j].YMinus;
                        coinc.YPlus2 = events[j].YPlus;
                        result.Add(coinc);
                        i = j + 1;
                        break;
                    }
            }

            if (result.Count != 0)
                WorkWithFiles.WriteCoincList(string.Format("{1}\\{0}.csv", index++, outDir), result);
            Console.WriteLine("\tFound {0} coincidences", result.Count);
            return result;
        }
    }
}
