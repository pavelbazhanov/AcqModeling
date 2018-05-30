using System;
using System.IO;
using AcqModeling;

class Program
{
    #region Acquisition settings

    // Время сбора
    public static double TotalTime = 0.5;

    // Временной интервал
    public static double dt = 0.05;

    // Суммарная активность (МБк)
    public static double TotalActivity = 100;

    // Период полураспада
    public static double T12 = 6400;

    static DetectorsConfiguration dc = DetectorsConfiguration.BaseConfiguration;

    static int coincWindow = 10;

    #endregion Acquisition settings

    static string OutSinglesDir = "D:/Test/Singles";
    static string OutCoincDir = "D:/Test/Coinc";
    static string OutSinDir = "D:/Test/Sin";
    static void Main(string[] args)
    {
        if (Directory.Exists(OutSinglesDir))
            Directory.Delete(OutSinglesDir, true);
        Directory.CreateDirectory(OutSinglesDir);

        if (Directory.Exists(OutCoincDir))
            Directory.Delete(OutCoincDir, true);
        Directory.CreateDirectory(OutCoincDir);

        double activity = TotalActivity * 1e6;
        int intervalCount = (int) (TotalTime / dt);
        for (int i = 0; i < intervalCount; i++)
        {
            //var events = Generators.GenerateEventsLinearSource(100, 0, 300, ref activity, i * dt, dt, T12, i);
            var events = Generators.GenerateEventsCylSource(100, 300, ref activity, i * dt, dt, T12, i);
            var singleEvents = Generators.GenerateSinglesNoAtt(events, dc, OutSinglesDir, i);
            Generators.GenerateCoincList(singleEvents, OutCoincDir, i, coincWindow);
        }

        SinogramBuilder sb = new SinogramBuilder(dc, OutSinDir);
        sb.Build(OutCoincDir);
    }
};