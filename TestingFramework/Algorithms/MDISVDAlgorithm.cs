using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TestingFramework.Testing;

namespace TestingFramework.Algorithms
{
    public partial class MDISVDAlgorithm : Algorithm
    {
        private static bool _init = false;
        public MDISVDAlgorithm() : base(ref _init)
        { }

        public override string[] EnumerateInputFiles(string dataCode, int tcase)
        {
            return new[] { $"{dataCode}_m{tcase}.txt" };
        }
        
        private static string Style => "linespoints lt 8 dt 1 lw 2 pt 1 lc rgbcolor \"black\" pointsize 1.2";

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms()
        {
            return new[] { new SubAlgorithm($"{AlgCode}", String.Empty, Style) };
        }

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms(int tcase)
        {
            return new[] { new SubAlgorithm($"{AlgCode}", $"{AlgCode}{tcase}", Style) };
        }
        
        protected override void PrecisionExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            RunGrouse(GetMDISVDProcess(data, tcase));
        }
        
        private Process GetMDISVDProcess(DataDescription data, int len)
        {
            Process isvdproc = new Process();
            
            isvdproc.StartInfo.WorkingDirectory = EnvPath;
            isvdproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/incCD";
            isvdproc.StartInfo.CreateNoWindow = true;
            isvdproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            isvdproc.StartInfo.UseShellExecute = false;

            isvdproc.StartInfo.Arguments = $"-alg mdisvd -test o -n {data.N} -m {data.M} -k {AlgoPack.TypicalTruncation} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}.txt";

            return isvdproc;
        }
        
        private Process GetRuntimeMDISVDProcess(DataDescription data, int len, bool stream)
        {
            Process isvdproc = new Process();
            
            isvdproc.StartInfo.WorkingDirectory = EnvPath;
            isvdproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/incCD";
            isvdproc.StartInfo.CreateNoWindow = true;
            isvdproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            isvdproc.StartInfo.UseShellExecute = false;

            isvdproc.StartInfo.Arguments = $"-alg mdisvd -test rt -n {data.N} -m {data.M} -k {AlgoPack.TypicalTruncation} " +
                                             $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                             $"-out ./{SubFolderDataOut}{AlgCode}{len}.txt " + (stream ? "-xtra stream" : "");

            return isvdproc;
        }
        private void RunGrouse(Process isvdproc)
        {
            isvdproc.Start();
            isvdproc.WaitForExit();
                
            if (isvdproc.ExitCode != 0)
            {
                string errText =
                    $"[WARNING] GROUSE returned code {isvdproc.ExitCode} on exit.{Environment.NewLine}" +
                    $"CLI args: {isvdproc.StartInfo.Arguments}";
                
                Console.WriteLine(errText);
                Utils.DelayedWarnings.Enqueue(errText);
            }
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es, DataDescription data,
            int tcase)
        {
            RunGrouse(GetRuntimeMDISVDProcess(data, tcase, et == ExperimentType.Streaming));
        }

        public override void GenerateData(string sourceFile, string code, int tcase, (int, int, int)[] missingBlocks,
            (int, int) rowRange, (int, int) columnRange)
        {
            sourceFile = DataWorks.FolderData + sourceFile;
            
            (int rFrom, int rTo) = rowRange;
            (int cFrom, int cTo) = columnRange;
            
            double[][] res = DataWorks.GetDataLimited(sourceFile, rTo - rFrom, cTo - cFrom);
            
            int n = rTo > res.Length ? res.Length : rTo;
            int m = cTo > res[0].Length ? res[0].Length : cTo;
            
            var data = new StringBuilder();

            for (int i = rFrom; i < n; i++)
            {
                string line = "";

                for (int j = cFrom; j < m; j++)
                {
                    if (Utils.IsMissing(missingBlocks, i, j))
                    {
                        line += "NaN" + " ";
                    }
                    else
                    {
                        line += res[i][j] + " ";
                    }
                }
                data.Append(line.Trim() + Environment.NewLine);
            }

            string destination = EnvPath + SubFolderDataIn + $"{code}_m{tcase}.txt";
            
            if (File.Exists(destination)) File.Delete(destination);
            File.AppendAllText(destination, data.ToString());
        }
    }
}