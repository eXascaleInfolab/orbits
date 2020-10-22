using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace TestingFramework.Testing
{
    public static class EnumMethods
    {
        public static bool EnableStreaming = true;
        public static bool EnableContinuous = true;
        public static bool EnableBatchMid = true;
        
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
                case ExperimentType.Continuous: return "btc-e";
                case ExperimentType.Recovery: return "btc-m";
                case ExperimentType.Streaming: return "str-e";
                default: throw new InvalidDataException();
            }
        }

        public static string ToLongString(this ExperimentType et)
        {
            switch (et)
            {
                case ExperimentType.Continuous: return "batch_end";
                case ExperimentType.Recovery: return "batch_mid";
                case ExperimentType.Streaming: return "streaming_end";
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
                case ExperimentScenario.BlockSlide: return "bslide";
                case ExperimentScenario.Blackout: return "frow";
                case ExperimentScenario.IncreasingBlockCount: return "mc-incr";
                
                case ExperimentScenario.MulticolDisjoint: return "mc-dj";
                case ExperimentScenario.MulticolOverlap: return "mc-ol";
                
                case ExperimentScenario.McarMatrixBlock: return "mcar-matbl";
                case ExperimentScenario.McarTsBlock: return "mcar-tsbl";
                case ExperimentScenario.McarTsMultiBlock: return "mcar-tsmulbl";
                case ExperimentScenario.McarTsElement: return "mcar-tselem";
                case ExperimentScenario.McarTsBlock2: return "mcar-tsbl2";
                case ExperimentScenario.McarTsMultiBlock2: return "mcar-tsmulbl";

                case ExperimentScenario.McarColumns: return "mcar-col";
                case ExperimentScenario.McarLength: return "mcar-len";
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
                case ExperimentScenario.BlockSlide: return "blockslide";
                case ExperimentScenario.Blackout: return "blackout";
                case ExperimentScenario.IncreasingBlockCount: return "multicol-increasing";
                
                case ExperimentScenario.MulticolDisjoint: return "multicol-disjoint";
                case ExperimentScenario.MulticolOverlap: return "multicol-overlap";
                
                case ExperimentScenario.McarMatrixBlock: return "mcar-matrix-block";
                case ExperimentScenario.McarTsBlock: return "mcar-ts-block";
                case ExperimentScenario.McarTsMultiBlock: return "mcar-ts-multiblock";
                case ExperimentScenario.McarTsElement: return "mcar-ts-element";
                case ExperimentScenario.McarTsBlock2: return "mcar-ts-block2";
                case ExperimentScenario.McarTsMultiBlock2: return "mcar-ts-multiblock2";

                case ExperimentScenario.McarColumns: return "mcar-columns";
                case ExperimentScenario.McarLength: return "mcar-length";
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
                case ExperimentScenario.BlockSlide: return "starting position of a block";
                case ExperimentScenario.Blackout: return "number of missing rows";
                case ExperimentScenario.IncreasingBlockCount: return "number of columns with a missing block";
                
                case ExperimentScenario.MulticolOverlap:
                case ExperimentScenario.MulticolDisjoint: return "number of columns with missing values";
                
                case ExperimentScenario.McarMatrixBlock: return "percentage of missing values";

                case ExperimentScenario.McarTsBlock:
                case ExperimentScenario.McarTsMultiBlock:
                case ExperimentScenario.McarTsBlock2:
                case ExperimentScenario.McarTsMultiBlock2:
                    return "percentage of time series with missing values";

                case ExperimentScenario.McarTsElement: return "percentage of time series with missing values";
                case ExperimentScenario.McarColumns: return "number of columns";
                case ExperimentScenario.McarLength: return "number of rows";
                default: throw new InvalidDataException();
            }
        }

        public static IEnumerable<ExperimentScenario> AllExperimentScenarios()
        {
            yield return ExperimentScenario.Missing;
            yield return ExperimentScenario.Length;
            yield return ExperimentScenario.Columns;
            yield return ExperimentScenario.BlockSlide;
            yield return ExperimentScenario.Blackout;
            yield return ExperimentScenario.IncreasingBlockCount;
            
            yield return ExperimentScenario.MulticolDisjoint;
            yield return ExperimentScenario.MulticolOverlap;
            
            yield return ExperimentScenario.McarMatrixBlock;
            yield return ExperimentScenario.McarTsBlock;
            yield return ExperimentScenario.McarTsMultiBlock;
            yield return ExperimentScenario.McarTsElement;
            yield return ExperimentScenario.McarTsBlock2;
            yield return ExperimentScenario.McarTsMultiBlock2;

            yield return ExperimentScenario.McarColumns;
            yield return ExperimentScenario.McarLength;
        }

        public static bool IsLimited(this ExperimentScenario es)
        {
            return es == ExperimentScenario.Columns || es == ExperimentScenario.McarColumns;
        }

        public static bool IsContinuous(this ExperimentScenario es)
        {
            return es == ExperimentScenario.Missing || es == ExperimentScenario.Length ||
                   es == ExperimentScenario.Columns || es == ExperimentScenario.Blackout ||
                   es == ExperimentScenario.IncreasingBlockCount ||
                   es == ExperimentScenario.McarTsBlock || es == ExperimentScenario.McarTsMultiBlock ||
                   es == ExperimentScenario.McarTsBlock2 || es == ExperimentScenario.McarTsMultiBlock2 ||
                   es == ExperimentScenario.McarColumns || es == ExperimentScenario.McarLength;
        }

        public static bool IsStreaming(this ExperimentScenario es)
        {
            return es.IsContinuous();
        }

        public static bool IsBatchMid(this ExperimentScenario es)
        {
            return es != ExperimentScenario.IncreasingBlockCount;
        }

        public static bool IsSingleColumn(this ExperimentScenario es)
        {
            return es == ExperimentScenario.Missing || es == ExperimentScenario.Length ||
                   es == ExperimentScenario.Columns || es == ExperimentScenario.BlockSlide;
        }

        public static bool IsBlackout(this ExperimentScenario es)
        {
            return es == ExperimentScenario.Blackout || es == ExperimentScenario.IncreasingBlockCount;
        }
        
        //
        // multi-purpose
        //
        public static bool IsMatch(this ExperimentType et, ExperimentScenario es)
        {
            switch (et)
            {
                case ExperimentType.Streaming:
                    return es.IsStreaming();
                
                case ExperimentType.Continuous:
                    return es.IsContinuous();
                
                case ExperimentType.Recovery:
                    return es.IsBatchMid();
                
                default:
                    return false;
            }
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
        Length, Missing, Columns, BlockSlide, Blackout, IncreasingBlockCount,
        
        MulticolDisjoint, MulticolOverlap,
        
        McarMatrixBlock, McarTsBlock, McarTsMultiBlock, McarTsElement, McarTsBlock2, McarTsMultiBlock2,

        McarColumns, McarLength
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
