using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TestingFramework.Testing;

namespace TestingFramework.Algorithms
{
    public partial class PCAMMEAlgorithm : Algorithm
    {
        private static bool _init = false;
        public PCAMMEAlgorithm() : base(ref _init)
        { }

        public override string[] EnumerateInputFiles(string dataCode, int tcase)
        {
            return new[] { $"{dataCode}_m{tcase}.txt" };
        }
        
        private static string Style => "linespoints lt 8 dt 1 lw 2 pt 1 lc rgbcolor \"purple\" pointsize 1.2";

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
            Run(et == ExperimentType.Streaming
                ? GetOnlineProcess(data, tcase, Experiment.Precision)
                : GetOfflineProcess(data, tcase, Experiment.Precision));
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es, DataDescription data,
            int tcase)
        {
            Run(et == ExperimentType.Streaming
                ? GetOnlineProcess(data, tcase, Experiment.Runtime)
                : GetOfflineProcess(data, tcase, Experiment.Runtime));
        }

        private Process GetOfflineProcess(DataDescription data, int len, Experiment ex)
        {
            string test = ex == Experiment.Precision ? "o" : "rt";
            Process proc = TemplateProcess();
            proc.StartInfo.FileName = EnvPath + "../cmake-build-debug/incCD";
            
            proc.StartInfo.Arguments = $"-alg pca-mme -test {test} -n {data.N} -m {data.M} -k {AlgoPack.TypicalTruncation} " +
                                       $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                       $"-out ./{SubFolderDataOut}{AlgCode}{len}.txt";

            return proc;
        }

        private Process GetOnlineProcess(DataDescription data, int len, Experiment ex)
        {
            string test = ex == Experiment.Precision ? "o" : "rt";
            Process proc = TemplateProcess();
            proc.StartInfo.FileName = EnvPath + "../cmake-build-debug/incCD";

            proc.StartInfo.Arguments = $"-alg pca-mme -test {test} -n {data.N} -m {data.M} -k {AlgoPack.TypicalTruncation} " +
                                       $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                       $"-out ./{SubFolderDataOut}{AlgCode}{len}.txt" + " -xtra stream";

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