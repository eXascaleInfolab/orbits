using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace TestingFramework.Testing
{
    public static class EnumMethods
    {
        public static bool EnableStreaming = false;
        public static bool EnableContinuous = false;
        public static bool EnableMulticolumn = false;
        
        //
        // Experiments
        //
        public static string ToShortString(this Experiment ex)
        {
            switch (ex)
            {
                case Experiment.Precision: return "prec";
                case Experiment.Runtime: return "runtime";
                default: throw new InvalidDataException();
            }
        }

        public static string ToLongString(this Experiment ex)
        {
            switch (ex)
            {
                case Experiment.Precision: return "precision";
                case Experiment.Runtime: return "runtime";
                default: throw new InvalidDataException();
            }
        }

        public static IEnumerable<Experiment> AllExperiments()
        {
            yield return Experiment.Precision;
            yield return Experiment.Runtime;
        }

        //
        // Types
        //
        
        public static string ToShortString(this ExperimentType et)
        {
            switch (et)
            {
                case ExperimentType.Continuous: return "cont";
                case ExperimentType.Recovery: return "rec";
                case ExperimentType.Streaming: return "str";
                default: throw new InvalidDataException();
            }
        }

        public static string ToLongString(this ExperimentType et)
        {
            switch (et)
            {
                case ExperimentType.Continuous: return "continuous";
                case ExperimentType.Recovery: return "recovery";
                case ExperimentType.Streaming: return "streaming";
                default: throw new InvalidDataException();
            }
        }

        public static IEnumerable<ExperimentType> AllExperimentTypes()
        {
            yield return ExperimentType.Recovery;
            if (EnableContinuous) yield return ExperimentType.Continuous;
            if (EnableStreaming) yield return ExperimentType.Streaming;
        }
        
        //
        // Scenarios
        //

        public static string ToShortString(this ExperimentScenario es)
        {
            switch (es)
            {
                case ExperimentScenario.Missing: return "mis";
                case ExperimentScenario.Length: return "len";
                case ExperimentScenario.Columns: return "col";
                case ExperimentScenario.MultiColumnDisjoint: return "mc-dj";
                case ExperimentScenario.MultiColumnOverlap: return "mc-ol";
                case ExperimentScenario.Fullrow: return "frow";
                case ExperimentScenario.BlockSlide: return "bsld";
                case ExperimentScenario.McarElement: return "mcar-elem";
                case ExperimentScenario.McarBlock: return "mcar-blck";
                default: throw new InvalidDataException();
            }
        }
        
        public static string ToLongString(this ExperimentScenario es)
        {
            switch (es)
            {
                case ExperimentScenario.Missing: return "missingpercentage";
                case ExperimentScenario.Length: return "length";
                case ExperimentScenario.Columns: return "columns";
                case ExperimentScenario.MultiColumnDisjoint: return "multicolumn-disjoint";
                case ExperimentScenario.MultiColumnOverlap: return "multicolumn-overlap";
                case ExperimentScenario.Fullrow: return "blackout";
                case ExperimentScenario.BlockSlide: return "blockslide";
                case ExperimentScenario.McarElement: return "mcar-element";
                case ExperimentScenario.McarBlock: return "mcar-block";
                default: throw new InvalidDataException();
            }
        }
        
        public static string ToDescriptiveString(this ExperimentScenario es)
        {
            switch (es)
            {
                case ExperimentScenario.Missing: return "number of missing values";
                case ExperimentScenario.Length: return "number of rows";
                case ExperimentScenario.Columns: return "number of columns";
                case ExperimentScenario.MultiColumnDisjoint: return "number of missing values";
                case ExperimentScenario.MultiColumnOverlap: return "number of missing values";
                case ExperimentScenario.Fullrow: return "number of missing rows";
                case ExperimentScenario.BlockSlide: return "position of the block (% from top)";
                case ExperimentScenario.McarElement: return "% of the values missing in all time series";
                case ExperimentScenario.McarBlock: return "number of columns containing missing blocks";
                default: throw new InvalidDataException();
            }
        }

        public static IEnumerable<ExperimentScenario> AllExperimentScenarios()
        {
            yield return ExperimentScenario.Missing;
            if (EnableMulticolumn) yield return ExperimentScenario.MultiColumnDisjoint;
            if (EnableMulticolumn) yield return ExperimentScenario.MultiColumnOverlap;
            yield return ExperimentScenario.Length;
            yield return ExperimentScenario.Columns;
            yield return ExperimentScenario.Fullrow;
            yield return ExperimentScenario.BlockSlide;
            yield return ExperimentScenario.McarElement;
            yield return ExperimentScenario.McarBlock;
        }
        
        public static bool IsSingleColumn(this ExperimentScenario es)
        {
            return es == ExperimentScenario.Missing || es == ExperimentScenario.Length ||
                   es == ExperimentScenario.Columns || es == ExperimentScenario.BlockSlide;
        }
        
        public static bool HasBlackouts(this ExperimentScenario es)
        {
            return es == ExperimentScenario.Fullrow || es == ExperimentScenario.McarElement ||
                   es == ExperimentScenario.McarBlock;
        }
    }
    public enum Experiment
    {
        Precision, Runtime
    }
    
    public enum ExperimentType
    {
        Continuous, Recovery, Streaming
    }

    public enum ExperimentScenario
    {
        Length, Missing, Columns, MultiColumnDisjoint, MultiColumnOverlap,
        Fullrow, BlockSlide, McarElement, McarBlock
    }

    [ImmutableObject(true)]
    public struct DataDescription
    {
        public readonly int N;
        public readonly int M;
        public readonly (int, int, int)[] MissingBlocks;
        public readonly string Code;

        public DataDescription(int n, int m, (int, int, int)[] missingBlocks, string code)
        {
            N = n;
            M = m;
            MissingBlocks = missingBlocks;
            Code = code;
        }
    }
}