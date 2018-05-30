using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace AcqModeling
{
    class SinogramBuilder
    {
        public SinogramBuilder(DetectorsConfiguration dc, string outDir)
        {
            indexer = new Indexer(dc.BlocksCount * dc.DetectorsPerBlock, dc.RingsCount * dc.DetectorsPerBlock, 2, 1);
            numSins = 2 * indexer.NumRings - 1;
            this.outDir = outDir;
        }
        Indexer indexer;
        int numSins;
        string outDir;

        public void Build(string path)
        {
            var sins = new double[numSins][,];
            for (int i = 0; i < numSins; i++)
                sins[i] = new double[indexer.NumDirs, indexer.NumLines];

            Bitmap bmp = new Bitmap(1024, 1024);
            Graphics gr = Graphics.FromImage(bmp);
            gr.TranslateTransform(bmp.Width / 2f, bmp.Height / 2f);

            List<string> fileList = new List<string>();
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileSystemInfo[] files = di.GetFileSystemInfos();
                foreach (var file in files)
                {
                    fileList.Add(file.FullName);
                }
            }

            foreach (var file in fileList)
            {
                var clist = WorkWithFiles.ReadCoincList(file);
                foreach (var c in clist)
                {
                    PETDigitalCoincidenceTOF cIJ = new PETDigitalCoincidenceTOF();
                    EventConverter.CalculateDigitalCoincidenceTOF(c, ref cIJ);

                    int Ring1 = indexer.GetRing(c.Position1, cIJ.J1);
                    int Ring2 = indexer.GetRing(c.Position2, cIJ.J2);

                    var sinidx = Ring1 + Ring2;

                    if (sinidx >= numSins) continue;

                    int Dir = indexer.GetDir(c.Position1, c.Position2, cIJ.I1, cIJ.I2);
                    int Line = indexer.GetLine(c.Position1, c.Position2, cIJ.I1, cIJ.I2);

                    //if (isDebug && ++cnt % 1000 == 0) 
                    //{ 
                    // gr.DrawLine(Pens.White, 410 * (float)Math.Cos(Math.PI * Detector1 / 360), 410 * (float)Math.Sin(Math.PI * Detector1 / 360), 
                    // 410 * (float)Math.Cos(Math.PI * Detector2 / 360), 410 * (float)Math.Sin(Math.PI * Detector2 / 360)); 
                    //} 
                    if (Line < 0) continue;
                    sins[sinidx][Dir, Line]++;
                }
            }

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            for (int i = 0; i < numSins; i++)
            {
                Sinogram sino = new Sinogram(sins[i], AnglesDistribution.Uniform(0, Math.PI, indexer.NumDirs));
                Stream ostream = File.Create(string.Format("{0}\\{1}", outDir, string.Format("{0}.sg", i)));
                //using (StreamWriter w = new StreamWriter(ostream, Encoding.UTF8)) 
                using (ostream)
                {
                    Sinogram.SaveSinogramToStream(ostream, sino);
                }

                Sinogram.DumpSinogram(sins[i], string.Format("{0}\\{1}.png", outDir, i));
                //if (isDebug) 
                // bmp.Save(string.Format("{0}\\lines.png", arg.OutDir)); 
            }
        }
    }
}
