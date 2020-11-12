using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestingFramework.Testing;

namespace TestingFramework.Algorithms
{
    public static class AlgoPack
    {
        // points to the location of the Algorithms/ folder, all the subfolders are handled automatically
        public static string GlobalAlgorithmsLocation = null;
        
        // static
        public static readonly Algorithm InCd = new IncrementalCentroidDecompositionAlgorithm();
        public static readonly Algorithm Tkcm = new TkcmAlgorithm();
        public static readonly Algorithm Spirit = new SpiritAlgorithm();
        public static readonly Algorithm Grouse = new GrouseAlgorithm();
        public static readonly Algorithm ArImp = new AutoRegressionAlgorithm();
        public static readonly Algorithm Ssa = new SSAAlgorithm();
        public static readonly Algorithm Mrnn = new MRNNAlgorithm();
        public static readonly Algorithm PcaMme = new PCAMMEAlgorithm();
        
        //example:
        //    public static readonly Algorithm Example = new ExampleAlgorithm();
        
        public static Algorithm[] ListAlgorithms = { InCd, Tkcm, Spirit, Grouse, ArImp, Ssa, Mrnn, PcaMme }; //initial full list of all algos
        public static Algorithm[] ListAlgorithmsMulticolumn = null;
        public static Algorithm[] ListAlgorithmsStreaming = null;

        public const int TypicalTruncation = 3;

        public static void PurgeAllIntermediateFiles()
        {
            ListAlgorithms.ForEach(x => x.DataCleanUp());
        }

        public static void CleanUncollectedResults()
        {
            Directory.EnumerateDirectories(DataWorks.FolderResults + "plots/")
                .Where(x => !x.EndsWith("/raw"))
                .Where(Directory.Exists)
                .ForEach(x => Directory.Delete(x, true));

            Directory.EnumerateFiles(DataWorks.FolderResults + "missingMat/")
                .Where(File.Exists)
                .Where(x => !x.EndsWith(".gitkeep"))
                .ForEach(File.Delete);

            Directory.EnumerateFiles(DataWorks.FolderResults + "plotfiles/out/")
                .Where(File.Exists)
                .Where(x => !x.EndsWith(".gitkeep"))
                .ForEach(File.Delete);
        }

        public static void EnsureFolderStructure(ExperimentType et, List<string> scenarios)
        {
            string root = DataWorks.FolderPlotsRemote + $"{et.ToLongString()}/";

            foreach (var es in EnumMethods.AllExperimentScenarios().Where(x => scenarios.Contains(x.ToLongString())))
            {
                string esDir = $"{root}{es.ToLongString()}/";
                if (!Directory.Exists(esDir))
                {
                    Directory.CreateDirectory(esDir);
                }
            }
        }
    }
    
    /*///////////////////////////////////////////////////////////*/
    /*                         P A T H S                         */
    /*///////////////////////////////////////////////////////////*/
    
    public partial class IncrementalCentroidDecompositionAlgorithm
    {
        public override string AlgCode => "orbits";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}AlgoCollection/_data/";
        protected override string SubFolderDataIn => "in/";
        protected override string SubFolderDataOut => "out/";
        public override bool IsStreaming => true;
    }

    public partial class SpiritAlgorithm
    {
        public override string AlgCode => "spirit";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}AlgoCollection/_data/";
        protected override string SubFolderDataIn => "in/";
        protected override string SubFolderDataOut => "out/";
        public override bool IsStreaming => true;
        public override bool IsMulticolumn => false;
    }

    public partial class TkcmAlgorithm
    {
        public override string AlgCode => "tkcm";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}AlgoCollection/_data/";
        protected override string SubFolderDataIn => "in/";
        protected override string SubFolderDataOut => "out/";
        public override bool IsStreaming => true;
        public override bool IsMulticolumn => false;
    }
    
    public partial class GrouseAlgorithm
    {
        public override string AlgCode => "grouse";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}AlgoCollection/_data/";
        protected override string SubFolderDataIn => "in/";
        protected override string SubFolderDataOut => "out/";
        public override bool IsStreaming => true;
        public override bool IsBlackout => false;
    }

    public partial class AutoRegressionAlgorithm
    {
        public override string AlgCode => "ogd-imp";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}AlgoCollection/_data/";
        protected override string SubFolderDataIn => "in/";
        protected override string SubFolderDataOut => "out/";
        public override bool IsStreaming => true;
    }

    public partial class SSAAlgorithm
    {
        public override string AlgCode => "ssa";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}ssa/tslib/";
        protected override string SubFolderDataIn => "data_in/";
        protected override string SubFolderDataOut => "data_out/";
        public override bool IsMulticolumn => false;
    }

    public partial class MRNNAlgorithm
    {
        public override string AlgCode => "m-rnn";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}M-RNN/";
        protected override string SubFolderDataIn => "data_in/";
        protected override string SubFolderDataOut => "data_out/";
        public override bool IsStreaming => true;
    }

    public partial class PCAMMEAlgorithm
    {
        public override string AlgCode => "pca-mme";
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}AlgoCollection/_data/";
        protected override string SubFolderDataIn => "in/";
        protected override string SubFolderDataOut => "out/";
        public override bool IsStreaming => true;
    }

    /*///////////////////////////////////////////////////////////*/
    /*                        E X A M P L E                      */
    /*///////////////////////////////////////////////////////////*/

    public partial class ExampleAlgorithm
    {
        // should have trailing slash, should have access level to write to
        // {this.EnvPath + this.SubFolderIn} and {this.EnvPath + this.SubFolderOut} should both be:
        // * valid locations as a Directory
        // * {{} + validFileName} should always be an eligible file destination to be copied or written to

        public override string AlgCode => "example"; // unique code
        protected override string _EnvPath => $"{AlgoPack.GlobalAlgorithmsLocation}/TBA/"; // usually expected to be a working directory of an executable
        //protected override string _EnvPath => "~/TBA/"; // can be replaced by an absolute path
        protected override string SubFolderDataIn => "todo/in/";
        protected override string SubFolderDataOut => "todo/out/";
        public override bool IsStreaming => true; // whether algorithm supports incremental recovery without recomputing everything from scratch
        public override bool IsMulticolumn => true; // whether algorithm can recover values outside of the first TS
        public override bool IsBlackout => false; // whether algorithm can recover lines that are completely missing
        
        // optional override of a getter
        public override bool IsPlottable => false;
    }
    
    public partial class ExampleAlgorithm : Algorithm
    {
        private static bool _init = false;
        public ExampleAlgorithm() : base(ref _init)
        { }
        
        // optional override function
        public override string[] EnumerateInputFiles(string dataCode, int tcase)
        {
            throw new NotImplementedException();
        }
        
        // optional override function
        public override string[] EnumerateOutputFiles(int tcase)
        {
            throw new NotImplementedException();
        }

        protected override void PrecisionExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            throw new NotImplementedException();
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es, DataDescription data,
            int tcase)
        {
            throw new NotImplementedException();
        }

        public override void GenerateData(string sourceFile, string code, int tcase, (int, int, int)[] missingBlocks,
            (int, int) rowRange, (int, int) columnRange)
        {
            throw new NotImplementedException();
        }
        
        // last element of the tuple should be
        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms()
        {
            return new[] { new SubAlgorithm($"{AlgCode}", String.Empty, "") };
        }

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms(int tcase)
        {
            return new[] { new SubAlgorithm($"{AlgCode}", $"{AlgCode}{tcase}", "") };
        }
    }
}