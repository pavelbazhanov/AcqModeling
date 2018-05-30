
using System.Collections.Generic;
using System.IO;
namespace AcqModeling
{
    static class WorkWithFiles
    {
        public static void WriteSingleEventList(string path, List<PETSingleEvent> events)
        {
            if (File.Exists(path))
                File.Delete(path);
            StreamWriter sw = new StreamWriter(path);

            foreach (var evnt in events)
            {
                string s = string.Format("{0},{1},{2},{3},{4},{5}\n", evnt.Timestamp, evnt.Position, evnt.XPlus, evnt.XMinus, evnt.YPlus, evnt.YMinus);
                sw.Write(s);
            }
        }
        public static void WriteCoincList(string path, List<PETCoincidenceTOF> coinc)
        {
            if (File.Exists(path))
                File.Delete(path);
            StreamWriter sw = new StreamWriter(path);

            foreach (var c in coinc)
            {
                string s = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n", c.Timestamp1, c.Position1, c.XPlus1, c.XMinus1, c.YPlus1, c.YMinus1, c.Timestamp2, c.Position2, c.XPlus2, c.XMinus2, c.YPlus2, c.YMinus2);
                sw.Write(s);
            }
        }

        public static List<PETCoincidenceTOF> ReadCoincList(string path)
        {
            var result = new List<PETCoincidenceTOF>();
            StreamReader sr = new StreamReader(path);

            string line;
            while ((line = sr.ReadLine()) != null) 
            {
                string[] parts = line.Split(',');
                if (parts.Length == 12)
                {
                    PETCoincidenceTOF coinc = new PETCoincidenceTOF();
                    uint.TryParse(parts[0], out coinc.Timestamp1);
                    ushort.TryParse(parts[1], out coinc.Position1);
                    ushort.TryParse(parts[2], out coinc.XPlus1);
                    ushort.TryParse(parts[3], out coinc.XMinus1);
                    ushort.TryParse(parts[4], out coinc.YPlus1);
                    ushort.TryParse(parts[5], out coinc.YMinus1);

                    uint.TryParse(parts[6], out coinc.Timestamp2);
                    ushort.TryParse(parts[7], out coinc.Position2);
                    ushort.TryParse(parts[8], out coinc.XPlus2);
                    ushort.TryParse(parts[9], out coinc.XMinus2);
                    ushort.TryParse(parts[10], out coinc.YPlus2);
                    ushort.TryParse(parts[11], out coinc.YMinus2);
                    result.Add(coinc);
                }
            }

            return result;
        }
    }
}
