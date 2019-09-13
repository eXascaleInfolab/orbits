using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TestingFramework.Testing;

namespace TestingFramework.Algorithms
{
    public partial class IncrementalCentroidDecompositionAlgorithm : Algorithm
    {
        public List<int> KList = new List<int>(new[] { 3, 2 });

        private static bool _init = false;
        public IncrementalCentroidDecompositionAlgorithm() : base(ref _init)
        { }
        
        public override string[] EnumerateOutputFiles(int tcase)
        {
            return KList.Select(k => $"{AlgCode}{tcase}_k{k}.txt").ToArray();
        }

        private static string StyleOf(int k) =>
            "linespoints lt 8 lw 3 pt 7 lc rgbcolor \"" + (k == 2 ? "blue" : "cyan") + $"\" pointsize 1.2";

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms()
        {
            return KList.Select(k => new SubAlgorithm($"{AlgCode}_k{k}", String.Empty, StyleOf(k)));
        }

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms(int tcase)
        {
            return KList.Select(k => new SubAlgorithm($"{AlgCode}_k{k}", $"{AlgCode}{tcase}_k{k}", StyleOf(k)));
        }
        
        protected override void PrecisionExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            if (et == ExperimentType.Streaming)
            {
                KList.ForEach(k => Run(GetOnlineProcess(data, tcase, k, Experiment.Precision)));
            }
            else
            {
                KList.ForEach(k => Run(GetOfflineProcess(data, tcase, k, Experiment.Precision)));
            }
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            if (et == ExperimentType.Streaming)
            {
                KList.ForEach(k => Run(GetOnlineProcess(data, tcase, k, Experiment.Runtime)));
            }
            else
            {
                KList.ForEach(k => Run(GetOfflineProcess(data, tcase, k, Experiment.Runtime)));
            }
        }

        private Process GetOfflineProcess(DataDescription data, int len, int k, Experiment ex)
        {
            string test = ex == Experiment.Precision ? "o" : "rt";
            Process proc = TemplateProcess();
            proc.StartInfo.FileName = EnvPath + "../cmake-build-debug/incCD";
            
            proc.StartInfo.Arguments = $"-alg cd -test {test} -n {data.N} -m {data.M} -k {k} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}_k{k}.txt";

            return proc;
        }

        private Process GetOnlineProcess(DataDescription data, int len, int k, Experiment ex)
        {
            string test = ex == Experiment.Precision ? "o" : "rt";
            Process proc = TemplateProcess();
            proc.StartInfo.FileName = EnvPath + "../cmake-build-debug/incCD";

            proc.StartInfo.Arguments = $"-alg cd -test {test} -n {data.N} -m {data.M} -k {k} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}_k{k}.txt" + " -xtra stream";

            return proc;
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