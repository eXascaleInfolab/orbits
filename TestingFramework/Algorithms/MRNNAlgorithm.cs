using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TestingFramework.Testing;

namespace TestingFramework.Algorithms
{
    public partial class MRNNAlgorithm : Algorithm
    {
        private static bool _init = false;
        public MRNNAlgorithm() : base(ref _init)
        { }

        public override string[] EnumerateInputFiles(string dataCode, int tcase)
        {
            return new[] { $"{dataCode}_m{tcase}.txt" };
        }
        
        private static string Style => "linespoints lt 8 dt 1 lw 2 pt 4 lc rgbcolor \"red\" pointsize 1.2";

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
            RunMRNN(GetMRNNProcess(data, tcase));
        }
        
        private Process GetMRNNProcess(DataDescription data, int len)
        {
            Process mrnnproc = new Process();
            
            mrnnproc.StartInfo.WorkingDirectory = EnvPath;
            mrnnproc.StartInfo.FileName = "python";
            mrnnproc.StartInfo.CreateNoWindow = true;
            mrnnproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            mrnnproc.StartInfo.UseShellExecute = false;
            
            string functionArgs = $"\"{SubFolderDataIn}{data.Code}_m{len}.txt\", \"{SubFolderDataOut}{AlgCode}{len}.txt\", 0";

            mrnnproc.StartInfo.Arguments = "-c 'from testerMRNN import mrnn_recov; " +
                                          $"mrnn_recov({functionArgs})'";

            return mrnnproc;
        }
        
        private Process GetRuntimeMRNNProcess(DataDescription data, int len)
        {
            Process mrnnproc = new Process();
            
            mrnnproc.StartInfo.WorkingDirectory = EnvPath;
            mrnnproc.StartInfo.FileName = "python";
            mrnnproc.StartInfo.CreateNoWindow = true;
            mrnnproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            mrnnproc.StartInfo.UseShellExecute = false;
            
            string functionArgs = $"\"{SubFolderDataIn}{data.Code}_m{len}.txt\", \"{SubFolderDataOut}{AlgCode}{len}.txt\", 1";

            mrnnproc.StartInfo.Arguments = "-c 'from testerMRNN import mrnn_recov; " +
                                          $"mrnn_recov({functionArgs})'";

            return mrnnproc;
        }
        private void RunMRNN(Process mrnnroc)
        {
            mrnnroc.Start();
            mrnnroc.WaitForExit();
                
            if (mrnnroc.ExitCode != 0)
            {
                string errText =
                    $"[WARNING] M-RNN returned code {mrnnroc.ExitCode} on exit.{Environment.NewLine}" +
                    $"CLI args: {mrnnroc.StartInfo.Arguments}";
                
                Console.WriteLine(errText);
                Utils.DelayedWarnings.Enqueue(errText);
            }
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es, DataDescription data,
            int tcase)
        {
            RunMRNN(GetRuntimeMRNNProcess(data, tcase));
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