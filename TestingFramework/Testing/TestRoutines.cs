using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TestingFramework.Algorithms;

namespace TestingFramework.Testing
{
    public static class TestRoutines
    {
        #region Configuration

        private const int TypicalN = 1000;
        private const int BlockSize = TypicalN / 10;
        private const int StartOffset = BlockSize / 2;

        private const int RandomSeed = 58975;
        
        private static (ValueTuple<int, int, int>[], int[]) GetExperimentSetup(ExperimentType et, ExperimentScenario es,
            int nlimit)
        {
            switch (et)
            {
                case ExperimentType.Continuous:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            return (new[] {(0, -1, -1)}, Utils.ClosedSequence(BlockSize, BlockSize * 8, BlockSize).ToArray());
                        
                        case ExperimentScenario.Length:
                            return (new[] {(0, -1, BlockSize)}, Utils.ClosedSequence(200, nlimit, 200).ToArray());
                        
                        case ExperimentScenario.Columns:
                            return (new[] {(0, nlimit - BlockSize, BlockSize)}, Utils.ClosedSequence(4, 12).ToArray());

                        case ExperimentScenario.Fullrow:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(10, 100, 10).TakeWhile(x => x + 10 < nlimit).ToArray());

                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                    
                case ExperimentType.Recovery:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            return (new[] {(0, StartOffset, -1)}, Utils.ClosedSequence(BlockSize, BlockSize * 8, BlockSize).ToArray());
                        
                        case ExperimentScenario.Length:
                            return (new[] {(0, StartOffset, BlockSize)}, Utils.ClosedSequence(200, nlimit, 200).ToArray());
                        
                        case ExperimentScenario.Columns:
                            return (new[] {(0, StartOffset, BlockSize)}, Utils.ClosedSequence(4, 12).ToArray());

                        case ExperimentScenario.MultiColumnDisjoint:
                            return (new[] {(0, StartOffset, -1)}, Utils.ClosedSequence(1, 4, 1).ToArray());
                        
                        case ExperimentScenario.MultiColumnOverlap:
                            return (new[] {(0, StartOffset, -1)}, Utils.ClosedSequence(1, 4, 1).ToArray());

                        case ExperimentScenario.Fullrow:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(10, 100, 10).TakeWhile(x => (x + nlimit / 20) < nlimit).ToArray());

                        case ExperimentScenario.BlockSlide:
                            return (new[] {(0, -1, BlockSize * 2)}, Utils.ClosedSequence(5, 80, 5).ToArray());
                        
                        case ExperimentScenario.McarElement:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(10, 80, 10).ToArray());
                        
                        case ExperimentScenario.McarBlock:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(1, 12, 1).ToArray());

                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                
                case ExperimentType.Streaming:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            return (new[] {(0, -1, -1)}, Utils.ClosedSequence(BlockSize, BlockSize * 8, BlockSize).ToArray());
                        
                        case ExperimentScenario.Length:
                            return (new[] {(0, -1, 100)}, Utils.ClosedSequence(200, nlimit, 200).ToArray());
                        
                        case ExperimentScenario.Columns:
                            return (new[] {(0, nlimit - BlockSize, BlockSize)}, Utils.ClosedSequence(4, 12).ToArray());
                        
                        case ExperimentScenario.MultiColumnOverlap:
                        case ExperimentScenario.MultiColumnDisjoint:
                            throw new ArgumentException("Multicolumn is unsupported for streaming test");
                            
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                
                default:
                    throw new ArgumentException("Unrecognized experiment type");
            }
        }

        private static void UpdateMissingBlocks(ExperimentType et, ExperimentScenario es, int nlimit,
            int tcase, ref ValueTuple<int, int, int>[] missingBlocks, int columns)
        {
            switch (et)
            {
                case ExperimentType.Continuous:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            missingBlocks[0].Item3 = tcase;
                            missingBlocks[0].Item2 = nlimit - tcase;
                            break;
                        
                        case ExperimentScenario.Length:
                            missingBlocks[0].Item2 = tcase - missingBlocks[0].Item3;
                            break;

                        case ExperimentScenario.Columns:
                            break;//nothing

                        case ExperimentScenario.Fullrow:
                            missingBlocks = Enumerable.Range(0, columns).Select(x => (x, nlimit - tcase, tcase)).ToArray();
                            break;

                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                    break;
                    
                case ExperimentType.Recovery:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            missingBlocks[0].Item3 = tcase;
                            break;
                        
                        case ExperimentScenario.Length:
                            break;//nothing

                        case ExperimentScenario.Columns:
                            break;//nothing
                        
                        case ExperimentScenario.MultiColumnDisjoint:
                            missingBlocks = Enumerable.Repeat(0, tcase).Select((_, idx) =>
                            {
                                int col = idx;
                                int start = BlockSize + idx * BlockSize * 2;
                                return (col, start, BlockSize * 2);
                            }).ToArray();
                            break;
                        
                        case ExperimentScenario.MultiColumnOverlap: // OVERLAPPING; STAIRCASE PLACEMENT; x1.5
                            missingBlocks = Enumerable.Repeat(0, tcase).Select((_, idx) =>
                            {
                                int col = idx;
                                int start = BlockSize + idx * BlockSize * 2;
                                return (col, start, BlockSize * 3);
                            }).ToArray();
                            break;

                        case ExperimentScenario.Fullrow:
                            missingBlocks = Enumerable.Range(0, columns).Select(col => (col, nlimit / 20, tcase)).ToArray();
                            break;

                        case ExperimentScenario.BlockSlide:
                            missingBlocks[0].Item2 = (nlimit * tcase) / 100;
                            break;
                        
                        case ExperimentScenario.McarElement:
                            List<(int, int, int)> missing2 = new List<(int, int, int)>();
                            Random r = new Random(RandomSeed);

                            List<(int, int)> missing = new List<(int, int)>();
                            
                            Dictionary<int, List<int>> columnIdx = new Dictionary<int, List<int>>();

                            for (int i = 0; i < columns; i++)
                            {
                                columnIdx.Add(i, Enumerable.Range(0, nlimit).ToList());
                            }
                            
                            for (int i = 0; i < (nlimit * columns * tcase) / 100; i++)
                            {
                                int col = r.Next(0, columnIdx.Count);
                                col = columnIdx.Keys.ElementAt(col);
                                int row = r.Next(0, columnIdx[col].Count);
                                row = columnIdx[col][row];
                                
                                missing.Add((col, row));

                                columnIdx[col].Remove(row);

                                if (columnIdx[col].Count == 1) columnIdx.Remove(col);
                            }

                            missing = missing.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
                            
                            int currentCol = -1;
                            int blockStart = -1;
                            int lastIdx = -1;
                            
                            for (int i = 0; i < missing.Count; i++)
                            {
                                (int col, int row) = missing[i];
                                
                                if (currentCol == col) //same col
                                {
                                    if (lastIdx == -1) // start of new block
                                    {
                                        lastIdx = row;
                                        blockStart = row;
                                    }
                                    else if (lastIdx != row - 1) // jump to the next block
                                    {
                                        missing2.Add((col, blockStart, lastIdx - blockStart + 1));

                                        blockStart = lastIdx = row;
                                    }
                                    else
                                    {
                                        lastIdx = row;
                                    }
                                }
                                else
                                {
                                    if (blockStart >= 0)
                                    {
                                        missing2.Add((currentCol, blockStart, lastIdx - blockStart + 1));
                                    }

                                    blockStart = lastIdx = row;
                                    currentCol = col;
                                }
                            }
                            
                            if (blockStart >= 0)
                            {
                                missing2.Add((currentCol, blockStart, lastIdx - blockStart + 1));
                            }

                            missingBlocks = missing2.ToArray();
                            break;
                        
                        case ExperimentScenario.McarBlock:
                            const int mcar_block = 10;
                            const int mcar_percentage = 10;
                            List<(int, int, int)> missing2b = new List<(int, int, int)>();
                            Random r2 = new Random(RandomSeed);

                            int activeColumns = tcase; // 1..4 or 1..12

                            List<(int, int)> missingb = new List<(int, int)>();
                            
                            Dictionary<int, List<int>> columnIdxb = new Dictionary<int, List<int>>();

                            for (int i = 0; i < activeColumns; i++)
                            {
                                columnIdxb.Add(i, Enumerable.Range(0, nlimit / mcar_block).ToList());
                            }
                            
                            for (int i = 0; i < (nlimit * activeColumns * mcar_percentage) / (100 * mcar_block); i++) // 100 for percentage adj
                            {
                                int col = r2.Next(0, columnIdxb.Count);
                                col = columnIdxb.Keys.ElementAt(col);
                                int row = r2.Next(0, columnIdxb[col].Count);
                                row = columnIdxb[col][row];
                                
                                for (int j = 0; j < 10; j++)
                                {
                                    missingb.Add((col, 10 * row + j));
                                }

                                columnIdxb[col].Remove(row);

                                if (columnIdxb[col].Count == 1) columnIdxb.Remove(col);
                            }

                            missingb = missingb.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
                            
                            int currentColb = -1;
                            int blockStartb = -1;
                            int lastIdxb = -1;
                            
                            for (int i = 0; i < missingb.Count; i++)
                            {
                                (int col, int row) = missingb[i];
                                
                                if (currentColb == col) //same col
                                {
                                    if (lastIdxb == -1) // start of new block
                                    {
                                        lastIdxb = row;
                                        blockStartb = row;
                                    }
                                    else if (lastIdxb != row - 1) // jump to the next block
                                    {
                                        missing2b.Add((col, blockStartb, lastIdxb - blockStartb + 1));

                                        blockStartb = lastIdxb = row;
                                    }
                                    else
                                    {
                                        lastIdxb = row;
                                    }
                                }
                                else
                                {
                                    if (blockStartb >= 0)
                                    {
                                        missing2b.Add((currentColb, blockStartb, lastIdxb - blockStartb + 1));
                                    }

                                    blockStartb = lastIdxb = row;
                                    currentColb = col;
                                }
                            }
                            
                            if (blockStartb >= 0)
                            {
                                missing2b.Add((currentColb, blockStartb, lastIdxb - blockStartb + 1));
                            }

                            missingBlocks = missing2b.ToArray();
                            break;
                        
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                    break;
                
                case ExperimentType.Streaming:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            missingBlocks[0].Item3 = tcase;
                            missingBlocks[0].Item2 = nlimit - tcase;
                            break;
                        
                        case ExperimentScenario.Length:
                            missingBlocks[0].Item2 = tcase - missingBlocks[0].Item3;
                            break;

                        case ExperimentScenario.Columns:
                            break;//nothing
                        
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                    break;
                
                default:
                    throw new ArgumentException("Unrecognized experiment type");
            }
        }

        private static int GetGnuPlotStartingNumber(ExperimentType et, ExperimentScenario es, int nlimit, int tcase)
        {
            if ((et == ExperimentType.Streaming || et == ExperimentType.Continuous) && es == ExperimentScenario.Length)
                return nlimit - tcase;
            
            return 0;
        }

        private static DataDescription PrepareDataDescription(ExperimentType et, ExperimentScenario es,
            string code, int nlimit, int cols, int tcase, (int, int, int)[] missingBlocks)
        {
            int n = es == ExperimentScenario.Length
                ? tcase
                : nlimit;
            
            int m = es == ExperimentScenario.Columns
                ? tcase
                : cols;
            
            return new DataDescription(n, m, missingBlocks, code);
        }

        private static ((int, int), (int, int)) GetDataRanges(ExperimentType et, ExperimentScenario es,
            int nlimit, int cols, int tcase)
        {
            (int rFrom, int rTo) = (0, nlimit);
            (int cFrom, int cTo) = (0, cols);

            switch (es)
            {
                // columns always start from 0, limited by datasize unless it's a column test
                case ExperimentScenario.Columns:
                    cTo = tcase;
                    break;
                
                // varlengths, it's 0...tcase
                case ExperimentScenario.Length:
                    rTo = tcase;
                    break;
            }

            return ((rFrom, rTo), (cFrom, cTo));
        }

        #endregion
        
        /// <summary>
        /// Runs a precision experiment of type <paramref name="et"/> with scenario <paramref name="es"/> on a dataset <paramref name="code"/>.
        /// </summary>
        /// <param name="et">Experiment type</param>
        /// <param name="es">Experiment scenario</param>
        /// <param name="code">Dataset codename</param>
        /// <param name="nlimit">Maximum length from the dataset</param>
        /// <exception cref="ArgumentException">Throws an exception if incompatible type/scenario are provided or a code isn't found.</exception>
        public static void PrecisionTest(
            ExperimentType et, ExperimentScenario es,
            string code, int nlimit = 1000)
        {
            if (et == ExperimentType.Streaming)
            {
                throw new ArgumentException("ExperimentType.Streaming is unsupported for precision test runs");
            }
            if (!File.Exists($"{DataWorks.FolderData}{code}/{code}_normal.txt"))
            {
                throw new ArgumentException("Invalid code is supplied, file not found in a expected location: " + $"{code}/{code}_normal.txt");
            }
            
            IEnumerable<Algorithm> algorithms =
                !es.IsSingleColumn()
                    ? AlgoPack.ListAlgorithmsMulticolumn
                    : AlgoPack.ListAlgorithms;

            if (es.HasBlackouts())
            {
                algorithms = algorithms.Where(a => a != AlgoPack.Ssa && a != AlgoPack.Grouse && a != AlgoPack.MdIsvd);
            }

            if (es.HasBlackouts() && es != ExperimentScenario.Fullrow) //only mcars
            {
                algorithms = algorithms.Where(a => a != AlgoPack.Mrnn);
            }
            
            // exceptional cases
            if (code == "bball" && nlimit >= 1800)
            {
                nlimit = 1800;
            }
            
            //varlen only
            ulong token =
                (code + et.ToLongString() + es.ToLongString()).ToCharArray()
                .Select((x, i) => (UInt64) (i * Math.Abs(Math.PI * x)))
                .Aggregate(0UL, (i, arg2) => i ^ arg2);
            
            // forward definitons
            const Experiment ex = Experiment.Precision;
            (ValueTuple<int, int, int>[] missingBlocks, int[] lengths) = GetExperimentSetup(et, es, nlimit);
            
            int dataSetColumns = DataWorks.CountMatrixColumns($"{code}/{code}_normal.txt");

            if (es == ExperimentScenario.MultiColumnDisjoint || es == ExperimentScenario.MultiColumnOverlap || es == ExperimentScenario.McarBlock)
            {
                lengths = lengths.Take(dataSetColumns).ToArray();
            }

            //
            // create necessary folder structure
            //

            foreach (int tcase in lengths)
            {
                if (!Directory.Exists(DataWorks.FolderResultsPlots + tcase))
                {
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase);
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase + "/raw");
                }
            }
            
            //
            // test phase
            //

            if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
            {
                string dataSource = $"{code}/{code}_normal.txt";
                
                foreach (int tcase in lengths)
                {
                    string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";

                    if (File.Exists($"{DataWorks.FolderData}" + adjustedDataSource)) File.Delete($"{DataWorks.FolderData}" + adjustedDataSource);
                    DataWorks.DataRange(dataSource, adjustedDataSource, nlimit - tcase, tcase);
                }
            }

            //do it
            foreach (Algorithm alg in algorithms)
            {
                foreach (int tcase in lengths)
                {
                    string dataSource = $"{code}/{code}_normal.txt";

                    if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
                    {
                        string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";
                        dataSource = adjustedDataSource;
                    }

                    UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns);

                    var (rowRange, columnRange) = GetDataRanges(et, es, nlimit, dataSetColumns, tcase);
                    var data = PrepareDataDescription(et, es, code, nlimit, dataSetColumns, tcase, missingBlocks);
                    
                    alg.GenerateData(dataSource, code, tcase, missingBlocks, rowRange, columnRange);
                    alg.RunExperiment(ex, et, es, data, tcase);
                }

                alg.CollectResults(ex, DataWorks.FolderResults,
                    lengths.Select(x => alg.EnumerateOutputFiles(x)).Flatten().ToArray());
            }

            //
            // add GNUPLOT
            //

            foreach (int tcase in lengths)
            {
                string[] allFiles = algorithms.Select(
                    alg => alg.EnumerateOutputFiles(tcase)
                ).Flatten().ToArray();
                    
                DataWorks.AddGnuPlotNumeration(false, tcase + "/",
                    GetGnuPlotStartingNumber(et, es, nlimit, tcase),
                    allFiles);
            }

            //
            // MSE/Correlation test
            //

            foreach (int tcase in lengths)
            {
                UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns);

                string referenceMatrix = $"../{DataWorks.FolderData}{code}/{code}_normal.txt";
                
                if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
                {
                    referenceMatrix = $"../{DataWorks.FolderData}_.temp/{token}_{code}_{tcase}.txt";
                }
                
                string[] allFiles = algorithms.Select(
                    alg => alg.EnumerateOutputFiles(tcase)
                ).Flatten().ToArray();

                DataWorks.CalculateMse(
                    missingBlocks, referenceMatrix, $"missingMat/missingMat{tcase}.txt",
                    allFiles
                );
            }

            //
            // GNUPLOT plt files
            //
            foreach (int tcase in lengths)
            {
                UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns);

                int offset = (et == ExperimentType.Continuous && es == ExperimentScenario.Length) ? nlimit - tcase : 0;
                DataWorks.GeneratePrecisionGnuPlot(algorithms, code, nlimit, tcase, missingBlocks, et, offset);
            }

            string referenceData = $"{DataWorks.FolderResults}{code}_normal.txt";
            if (File.Exists(referenceData)) File.Delete(referenceData);
            
            File.Copy($"{DataWorks.FolderData}{code}/{code}_normal.txt", referenceData);
            
            DataWorks.AddGnuPlotNumeration(false, "", 0, $"{code}_normal.txt"); // now the file resides in results/plots/file.txt
            
            //
            // Move results to proper folders
            //
            
            string rootDir = DataWorks.FolderPlotsRemote +
                             $"{ex.ToLongString()}/{et.ToLongString()}/{es.ToLongString()}/{code}/";
            if (!Directory.Exists(rootDir))
            {
                Directory.CreateDirectory(rootDir);
                Directory.CreateDirectory(rootDir + "data/");
                Directory.CreateDirectory(rootDir + "error/");
                Directory.CreateDirectory(rootDir + "recovery/");
                
                Directory.CreateDirectory(rootDir + "error/figs/");
                Directory.CreateDirectory(rootDir + "error/misc/");
                Directory.CreateDirectory(rootDir + "error/results/");
                Directory.CreateDirectory(rootDir + "error/results/values/");
                Directory.CreateDirectory(rootDir + "error/results/values/mse/");
                Directory.CreateDirectory(rootDir + "error/results/values/rmse/");
                Directory.CreateDirectory(rootDir + "error/results/recovered_matrices/");
                
                Directory.CreateDirectory(rootDir + "recovery/figs/");
                Directory.CreateDirectory(rootDir + "recovery/scripts/");
            }
            
            Console.WriteLine("Copying over results");

            // case'd things
            foreach (int tcase in lengths)
            {
                // gnuplots
                string dataTCaseFolder = rootDir + "data/" + tcase + "/";
                if (Directory.Exists(dataTCaseFolder)) Directory.Delete(dataTCaseFolder, true);
                
                Directory.Move(
                    DataWorks.FolderResultsPlots + tcase + "/",
                    dataTCaseFolder);
                
                // plotfiles
                string pltFile = rootDir + $"recovery/scripts/{code}_m{tcase}.plt";
                if (File.Exists(pltFile)) File.Delete(pltFile);
                    
                File.Move(
                    DataWorks.FolderResults + "plotfiles/out/" + $"{code}_m{tcase}.plt",
                    pltFile);
                
                // recovered matrices
                string recoveredMatFile = rootDir + "error/results/recovered_matrices/" + $"recoveredMat{tcase}.txt";
                if (File.Exists(recoveredMatFile)) File.Delete(recoveredMatFile);
                
                File.Move(
                    DataWorks.FolderResults + "missingMat/" + $"missingMat{tcase}.txt",
                    recoveredMatFile);
            }
            // independent things
            int start = lengths.First(), end = lengths.Last(), tick = lengths.Skip(1).First() - start;
            
            // mse
            DataWorks.GenerateMseGnuPlot(algorithms, code, start, end, tick, es, et);

            string mseFile = rootDir + $"error/results/{code}_mse.plt";
            if (File.Exists(mseFile)) File.Delete(mseFile);
            
            File.Copy($"{DataWorks.FolderResults}plotfiles/out/{code}_mse.plt", mseFile);
            
            // Rscript
            Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/template_err.r",
                $"{rootDir}error/results/error_calculation.r",
                ("{start}", start.ToString()),
                ("{end}", end.ToString()),
                ("{tick}", tick.ToString()),
                ("{allAlgos}", algorithms
                    .Select(a => a.EnumerateSubAlgorithms())
                    .Flatten()
                    .Select(sa => $"\"{sa.Code}\"")
                    .StringJoin(","))
                );
            
            // plotall
            Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/template_plotall.py",
                $"{rootDir}plotall.py",
                ("{code}", code),
                ("{start}", start.ToString()),
                ("{end}", end.ToString()),
                ("{tick}", tick.ToString())
            );

            // reference plot
            if (dataSetColumns > 4)
            {
                // copy 6-column reference
                Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/reference_plot_6.plt",
                    $"{rootDir}recovery/scripts/reference_plot.plt",
                    ("{nlimit}", nlimit.ToString())
                );
            }
            else
            {
                // copy 4-column reference
                Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/reference_plot_4.plt",
                    $"{rootDir}recovery/scripts/reference_plot.plt",
                    ("{nlimit}", nlimit.ToString())
                );
            }
            
            if (File.Exists($"{rootDir}data/reference.txt")) File.Delete($"{rootDir}data/reference.txt");
            File.Move($"{DataWorks.FolderResultsPlots}{code}_normal.txt", $"{rootDir}data/reference.txt");
            
            Console.WriteLine("Plotting results");
            
            Utils.RunSimpleVoidProcess("python", rootDir, "plotall.py");
            
            Console.WriteLine($"Sequence {ex.ToLongString()} / {et.ToLongString()} / {es.ToLongString()} for {code} completed");
            
            //
            // cleanup
            //
            Console.WriteLine("Starting cleanup...");
            AlgoPack.PurgeAllIntermediateFiles(); // handles algo's internal in/out fodlers

            Console.WriteLine("Intermediate files cleaned up");

            Directory.EnumerateFiles(DataWorks.FolderResults)
                .Where(x => !Directory.Exists(x)) // hacky-hacky
                .ForEach(File.Delete);
            
            Console.WriteLine("Results folder cleaned up");
            Console.WriteLine("Cleanup finished");
        }

        /*///////////////////////////////////////////////////////////*/
        
        
        /// <summary>
        /// Runs a runtime experiment of type <paramref name="et"/> with scenario <paramref name="es"/> on a dataset <paramref name="code"/>.
        /// </summary>
        /// <param name="et">Experiment type</param>
        /// <param name="es">Experiment scenario</param>
        /// <param name="code">Dataset codename</param>
        /// <param name="nlimit">Maximum length from the dataset</param>
        /// <exception cref="ArgumentException">Throws an exception if incompatible type/scenario are provided or a code isn't found.</exception>
        public static void RuntimeTest(
            ExperimentType et, ExperimentScenario es,
            string code, int nlimit = 1000)
        {
            if (et == ExperimentType.Streaming && es == ExperimentScenario.MultiColumnDisjoint)
            {
                throw new ArgumentException("ExperimentScenario.MissingMultiColumn is unsupported in combination with ExperimentType.Streaming");
            }
            
            IEnumerable<Algorithm> algorithms =
                et == ExperimentType.Streaming
                ? AlgoPack.ListAlgorithmsStreaming
                : (
                    !es.IsSingleColumn()
                        ? AlgoPack.ListAlgorithmsMulticolumn
                        : AlgoPack.ListAlgorithms
                );

            if (es.HasBlackouts())
            {
                algorithms = algorithms.Where(a => a != AlgoPack.Ssa && a != AlgoPack.Grouse && a != AlgoPack.MdIsvd);
            }

            if (es.HasBlackouts() && es != ExperimentScenario.Fullrow) //only mcars
            {
                algorithms = algorithms.Where(a => a != AlgoPack.Mrnn);
            }
            
            if (!File.Exists($"{DataWorks.FolderData}{code}/{code}_normal.txt"))
            {
                throw new ArgumentException("Invalid code is supplied, file not found in a expected location: " + $"{code}/{code}_normal.txt");
            }
            
            // exceptions
            if (code == "bball" && nlimit >= 1800)
            {
                nlimit = 1800;
            }
            
            //varlen only
            ulong token =
                (code + et.ToLongString() + es.ToLongString()).ToCharArray()
                .Select((x, i) => (UInt64) (i * Math.Abs(Math.PI * x)))
                .Aggregate(0UL, (i, arg2) => i ^ arg2);
            
            // forward definitons
            const Experiment ex = Experiment.Runtime;
            (ValueTuple<int, int, int>[] missingBlocks, int[] lengths) = GetExperimentSetup(et, es, nlimit);

            int dataSetColumns = DataWorks.CountMatrixColumns($"{code}/{code}_normal.txt");

            if (es == ExperimentScenario.MultiColumnDisjoint || es == ExperimentScenario.MultiColumnOverlap)
            {
                lengths = lengths.Take(dataSetColumns).ToArray();
            }
            //
            // create necessary folder structure
            //

            foreach (int tcase in lengths)
            {
                if (!Directory.Exists(DataWorks.FolderResultsPlots + tcase))
                {
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase);
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase + "/raw");
                }
            }
            
            //
            // test phase
            //

            if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
            {
                string dataSource = $"{code}/{code}_normal.txt";
                
                foreach (int tcase in lengths)
                {
                    string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";

                    if (File.Exists($"{DataWorks.FolderData}" + adjustedDataSource)) File.Delete($"{DataWorks.FolderData}" + adjustedDataSource);
                    DataWorks.DataRange(dataSource, adjustedDataSource, nlimit - tcase, tcase);
                }
            }

            //do it
            foreach (Algorithm alg in algorithms)
            {
                foreach (int tcase in lengths)
                {
                    string dataSource = $"{code}/{code}_normal.txt";

                    if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
                    {
                        string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";
                        dataSource = adjustedDataSource;
                    }

                    UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns);

                    var (rowRange, columnRange) = GetDataRanges(et, es, nlimit, dataSetColumns, tcase);
                    var data = PrepareDataDescription(et, es, code, nlimit, dataSetColumns, tcase, missingBlocks);
                    
                    alg.GenerateData(dataSource, code, tcase, missingBlocks, rowRange, columnRange);
                    alg.RunExperiment(ex, et, es, data, tcase);
                }

                alg.CollectResults(ex, DataWorks.FolderResults,
                    lengths.Select(x => alg.EnumerateOutputFiles(x)).Flatten().ToArray());
            }
            
            //
            // create outputs
            //

            string rootDir = DataWorks.FolderPlotsRemote +
                             $"{ex.ToLongString()}/{et.ToLongString()}/{es.ToLongString()}/{code}/";
            if (!Directory.Exists(rootDir))
            {
                Directory.CreateDirectory(rootDir);
                Directory.CreateDirectory(rootDir + "results/");
                Directory.CreateDirectory(rootDir + "figs/");
            }

            Console.WriteLine("Copying over results");

            //
            // add GNUPLOT
            //
            
            DataWorks.CollectRuntimeResults(lengths, algorithms, rootDir + "results/");

            //
            // GNUPLOT plt files
            //
            int start = lengths.First(), end = lengths.Last(), tick = lengths.Skip(1).First() - start;

            DataWorks.GenerateRuntimeGnuPlot(algorithms, code, start, end, tick, et, es);
            
            string plotFileExt = rootDir + $"{code}_rt.plt";

            if (File.Exists(plotFileExt)) File.Delete(plotFileExt);
            
            File.Move($"{DataWorks.FolderResults}plotfiles/out/{code}_rt.plt", plotFileExt);
            
            Console.WriteLine("Plotting results");
            Utils.RunSimpleVoidProcess("gnuplot", rootDir, $"{code}_rt.plt");

            //
            // cleanup
            //
            Console.WriteLine("Starting cleanup...");
            AlgoPack.PurgeAllIntermediateFiles(); // handles algo's internal in/out fodlers

            Console.WriteLine("Intermediate files cleaned up");
            
            Directory.EnumerateFiles(DataWorks.FolderResults)
                .Where(x => !Directory.Exists(x))
                .ForEach(File.Delete);
            
            Console.WriteLine("Gnuplot folder cleaned up");
            Console.WriteLine("Cleanup finished");
            
            Console.WriteLine($"Sequence {ex.ToLongString()} / {et.ToLongString()} / {es.ToLongString()} for {code} completed");
        }
        
        /// <summary>
        /// Plots the results of a runtime experiment of type <paramref name="et"/> with scenario <paramref name="es"/> on a dataset <paramref name="code"/>.
        /// Overwrites old templates, only to be executed on top of an experiment with the same parameters. Doesn't re-run the experiment.
        /// </summary>
        /// <param name="et">Experiment type</param>
        /// <param name="es">Experiment scenario</param>
        /// <param name="code">Dataset codename</param>
        /// <param name="nlimit">Maximum length from the dataset</param>
        /// <exception cref="ArgumentException">Throws an exception if incompatible type/scenario are provided or a code isn't found.</exception>
        /// <exception cref="InvalidOperationException">Throws an exception if the folder for this specific experiment set up doesn't exist.</exception>
        public static void RuntimeTestReplot(
            ExperimentType et, ExperimentScenario es,
            string code, int nlimit = 1000)
        {
            if (!File.Exists($"{DataWorks.FolderData}{code}/{code}_normal.txt"))
            {
                throw new ArgumentException("Invalid code is supplied, file not found in a expected location: " + $"{code}/{code}_normal.txt");
            }
            
            IEnumerable<Algorithm> algorithms =
                et == ExperimentType.Streaming
                    ? AlgoPack.ListAlgorithmsStreaming
                    : (
                        !es.IsSingleColumn()
                            ? AlgoPack.ListAlgorithmsMulticolumn
                            : AlgoPack.ListAlgorithms
                    );
            
            // exceptions
            if (code == "bball" && nlimit >= 1800)
            {
                nlimit = 1800;
            }
            
            // forward definitons
            const Experiment ex = Experiment.Runtime;
            (_, int[] lengths) = GetExperimentSetup(et, es, nlimit);

            //
            // create outputs
            //

            string rootDir = DataWorks.FolderPlotsRemote +
                             $"{ex.ToLongString()}/{et.ToLongString()}/{es.ToLongString()}/{code}/";
            
            if (!Directory.Exists(rootDir))
            {
                throw new InvalidOperationException();
            }

            //
            // GNUPLOT plt files
            //
            int start = lengths.First(), end = lengths.Last(), tick = lengths.Skip(1).First() - start;

            DataWorks.GenerateRuntimeGnuPlot(algorithms, code, start, end, tick, et, es);
            
            string plotFileExt = rootDir + $"{code}_rt.plt";

            if (File.Exists(plotFileExt)) File.Delete(plotFileExt);
            
            File.Move($"{DataWorks.FolderResults}plotfiles/out/{code}_rt.plt", plotFileExt);
            
            Console.WriteLine("Plotting results");
            Utils.RunSimpleVoidProcess("gnuplot", rootDir, $"{code}_rt.plt");

            //
            // cleanup
            //
            
            Console.WriteLine($"Sequence {ex.ToLongString()} / {et.ToLongString()} / {es.ToLongString()} [REPLOT] for {code} completed");
        }
    }
}