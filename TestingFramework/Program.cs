﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TestingFramework.Algorithms;
using TestingFramework.Testing;

namespace TestingFramework
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // technical
            Console.CancelKeyPress += InterruptHandler;
            
            string[] codes = null;
            string[] codesLimited = null;

            List<string> ignoreList = new List<string>();

            Dictionary<string, string> config =
                args.Length == 0
                    ? Utils.ReadConfigFile()
                    : Utils.ReadConfigFile(args[0]);

            bool disableTrmf = false;

            bool runPrecision = true;
            bool runRuntime = true;

            foreach (KeyValuePair<string,string> entry in config)
            {
                switch (entry.Key)
                {
                    case "PlotsFolder":
                        DataWorks.FolderPlotsRemoteBase = entry.Value.Trim().Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                        DataWorks.FolderPlotsRemote = DataWorks.FolderPlotsRemoteBase;
                        break;
                        
                    case "AlgorithmsLocation":
                        AlgoPack.GlobalAlgorithmsLocation = entry.Value.Trim().Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                        break;
                    
                    case "Datasets":
                        codes = entry.Value.Split(',').Select(c => c.Trim()).ToArray();
                        codesLimited = codes
                            .Where(c => DataWorks.CountMatrixColumns($"{c}/{c}_normal.txt") > 4)
                            .ToArray();
                        break;
                    
                    case "EnableStreaming":
                        EnumMethods.EnableStreaming = Convert.ToBoolean(entry.Value);
                        break;
                    
                    case "EnableContinuous":
                        EnumMethods.EnableContinuous = Convert.ToBoolean(entry.Value);
                        break;
                    
                    case "DisableTrmf":
                        disableTrmf = Convert.ToBoolean(entry.Value);
                        break;

                    case "EnabledAlgorithms":
                        ignoreList.AddRange(entry.Value.Split(',').Select(x => x.Trim().ToLower()));
                        break;
                    
                    case "DisablePrecision":
                        runPrecision = !Convert.ToBoolean(entry.Value);
                        break;
                    
                    case "DisableRuntime":
                        runRuntime = !Convert.ToBoolean(entry.Value);
                        break;
                    
                    default:
                        Utils.DelayedWarnings.Enqueue($"Warning: unknown config entry with the key {entry.Key}");
                        break;
                }
            }
            
            // verification that all necessary entries are provided
            if (DataWorks.FolderPlotsRemoteBase == null)
            {
                throw new InvalidProgramException("Invalid config file: plots folder has to be supplied (PlotsFolder=)");
            }
            
            if (AlgoPack.GlobalAlgorithmsLocation == null)
            {
                throw new InvalidProgramException("Invalid config file: algorithms folder has to be supplied (AlgorithmsLocation=)");
            }
            
            if (codes == null || codes.Length == 0)
            {
                throw new InvalidProgramException("Invalid config file: datasets are not supplied (Datasets=) or the list is empty");
            }
            
            if (disableTrmf)
            {
                ignoreList.Remove(AlgoPack.Trmf.AlgCode.ToLower());
            }

            AlgoPack.ListAlgorithms = AlgoPack.ListAlgorithms.Where(x => ignoreList.Contains(x.AlgCode.ToLower())).ToArray();
            AlgoPack.ListAlgorithmsStreaming = AlgoPack.ListAlgorithms.Where(x => x.IsStreaming).ToArray();
            AlgoPack.ListAlgorithmsMulticolumn = AlgoPack.ListAlgorithms.Where(x => x.IsMulticolumn).ToArray();
            
            AlgoPack.CleanUncollectedResults();
            AlgoPack.EnsureFolderStructure();
            
            void FullRun(bool enablePrecision, bool enableRuntime)
            {
                foreach (string code in codes)
                {
                    if (EnumMethods.EnableContinuous)
                    {
                        foreach (ExperimentScenario es in EnumMethods.AllExperimentScenarios().Where(EnumMethods.IsContinuous))
                        {
                            if (es.IsLimited() && !codesLimited.Contains(code)) continue;
                            if (enablePrecision) TestRoutines.PrecisionTest(ExperimentType.Continuous, es, code);
                            if (enableRuntime) TestRoutines.RuntimeTest(ExperimentType.Continuous, es, code);
                        }
                    }
                    
                    foreach (ExperimentScenario es in EnumMethods.AllExperimentScenarios().Where(EnumMethods.IsBatchMid))
                    {
                        if (es.IsLimited() && !codesLimited.Contains(code)) continue;
                        if (enablePrecision) TestRoutines.PrecisionTest(ExperimentType.Recovery, es, code);
                        if (enableRuntime) TestRoutines.RuntimeTest(ExperimentType.Recovery, es, code);
                    }

                    if (EnumMethods.EnableStreaming)
                    {
                        foreach (ExperimentScenario es in EnumMethods.AllExperimentScenarios().Where(EnumMethods.IsStreaming))
                        {
                            if (es.IsLimited() && !codesLimited.Contains(code)) continue;
                            if (enablePrecision) TestRoutines.PrecisionTest(ExperimentType.Streaming, es, code);
                            if (enableRuntime) TestRoutines.RuntimeTest(ExperimentType.Streaming, es, code);
                        }
                    }
                }
            }
            void FullRuntimeReplot() //service method
            {
                if (EnumMethods.EnableContinuous)
                {
                    codes.ForEach(c => TestRoutines.RuntimeTestReplot(ExperimentType.Continuous, ExperimentScenario.Missing, c));
                    codes.ForEach(c => TestRoutines.RuntimeTestReplot(ExperimentType.Continuous, ExperimentScenario.Length, c));
                    codesLimited.ForEach(c => TestRoutines.RuntimeTestReplot(ExperimentType.Continuous, ExperimentScenario.Columns, c));
                }

                codes.ForEach(c => TestRoutines.RuntimeTestReplot(ExperimentType.Recovery, ExperimentScenario.Missing, c));
                codes.ForEach(c => TestRoutines.RuntimeTestReplot(ExperimentType.Recovery, ExperimentScenario.Length, c));
                codesLimited.ForEach(c => TestRoutines.RuntimeTestReplot(ExperimentType.Recovery, ExperimentScenario.Columns, c));
            }
            
            FullRun(runPrecision, runRuntime);
            
            //
            // multi-run for 1...N runtime tests and averaging the results from them
            //

            #if false
            {
                for (int i = 1; i <= 3; i++)
                {
                    DataWorks.FolderPlotsRemote = DataWorks.FolderPlotsRemoteBase + i + "/";
                    if (!Directory.Exists(DataWorks.FolderPlotsRemote))
                    {
                        Directory.CreateDirectory(DataWorks.FolderPlotsRemote);
                        AlgoPack.EnsureFolderStructure();
                    }

                    FullRuntime();
                    //FullStreaming();
                }
                DataWorks.FolderPlotsRemote = DataWorks.FolderPlotsRemoteBase;
            }
    
            //SingularExperiments.AverageRTRuns(codes, codesLimited, 5);
            #endif
            
            #if false
            {
                string cmptype = "meaninit_vert";
                
                var sw_prec = new StreamWriter(File.Open($"prec_report_{cmptype}.txt", FileMode.Create));
                var sw_rt = new StreamWriter(File.Open($"rt_report_{cmptype}.txt", FileMode.Create));
                
                SingularExperiments.MsePerformanceReport(codes, codesLimited, sw_prec.WriteLine, cmptype);
                SingularExperiments.SSVIterPerformanceReport(codes, codesLimited, sw_rt.WriteLine, cmptype);
                
                sw_prec.Close();
                sw_rt.Close();
            }
            #endif  
            //
            // time series
            //
            
            #if false
            {
                var data = DataWorks.TimeSeries("BAFU", "*.asc", 3, Utils.Specific.ParseWasserstand, new DateTime(2005, 1, 1), true);
                DataWorks.TimeSeriesMerge(data, "BAFU_total.txt");
            }
            #endif

            FinalSequence();
        }

        private static void FinalSequence()
        {
            if (Utils.DelayedWarnings.Count > 0)
            {
                var writer = new Utils.ContinuousWriter();

                writer.WriteLine($"====={Environment.NewLine}The following delayed warnings were recorded by Testing Framework:" +
                                 $"{Environment.NewLine}===={Environment.NewLine}");

                writer.Indent();

                while (Utils.DelayedWarnings.Count > 0)
                {
                    writer.WriteLine(Utils.DelayedWarnings.Dequeue());
                }
            }
            Console.WriteLine("--- END ---");
        }

        private static void InterruptHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("--- Ctrl+C received ---");
            FinalSequence();
        }

    }
}
