using System;
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
            
            List<string> scenarios = new List<string>();

            Dictionary<string, string> config =
                args.Length == 0
                    ? Utils.ReadConfigFile()
                    : Utils.ReadConfigFile(args[0]);

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
                        
                    case "Scenarios":
                        scenarios.AddRange(entry.Value.Split(',').Select(x => x.Trim().ToLower()));
                        break;
                    
                    case "EnableStreaming":
                        EnumMethods.EnableStreaming = Convert.ToBoolean(entry.Value);
                        break;
                    
                    case "EnableContinuous":
                        EnumMethods.EnableContinuous = Convert.ToBoolean(entry.Value);
                        break;
                        
                    case "EnableBatchMid":
                        EnumMethods.EnableBatchMid = Convert.ToBoolean(entry.Value);
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

            AlgoPack.ListAlgorithms = AlgoPack.ListAlgorithms.Where(x => ignoreList.Contains(x.AlgCode.ToLower())).ToArray();
            AlgoPack.ListAlgorithmsStreaming = AlgoPack.ListAlgorithms.Where(x => x.IsStreaming).ToArray();
            AlgoPack.ListAlgorithmsMulticolumn = AlgoPack.ListAlgorithms.Where(x => x.IsMulticolumn).ToArray();
            
            AlgoPack.CleanUncollectedResults();
            
            List<ExperimentScenario> activeScenarios = new List<ExperimentScenario>();
            
            foreach (string scen in scenarios)
            {
                bool found = false;
                foreach (ExperimentScenario es in EnumMethods.AllExperimentScenarios())
                {
                    if (scen == es.ToLongString())
                    {
                        found = true;
                        activeScenarios.Add(es);
                    }
                }
                if (!found)
                {
                    throw new InvalidProgramException("Invalid config file: list of scenarios contains entries which are not supported by the testing framework");
                }
            }
            
            void FullRun(bool enablePrecision, bool enableRuntime)
            {
                foreach (string code in codes)
                {
                    if (EnumMethods.EnableContinuous)
                    {
                        AlgoPack.EnsureFolderStructure(ExperimentType.Continuous, activeScenarios.Where(EnumMethods.IsContinuous).Select(es => es.ToLongString()).ToList());

                        foreach (ExperimentScenario es in activeScenarios.Where(EnumMethods.IsContinuous))
                        {
                            if (es.IsLimited() && !codesLimited.Contains(code)) continue;
                            if (enablePrecision) TestRoutines.PrecisionTest(ExperimentType.Continuous, es, code);
                            if (enableRuntime) TestRoutines.RuntimeTest(ExperimentType.Continuous, es, code);
                        }
                    }
                    
                    if (EnumMethods.EnableBatchMid)
                    {
                        AlgoPack.EnsureFolderStructure(ExperimentType.Recovery, activeScenarios.Where(EnumMethods.IsBatchMid).Select(es => es.ToLongString()).ToList());

                        foreach (ExperimentScenario es in activeScenarios.Where(EnumMethods.IsBatchMid))
                        {
                            if (es.IsLimited() && !codesLimited.Contains(code)) continue;
                            if (enablePrecision) TestRoutines.PrecisionTest(ExperimentType.Recovery, es, code);
                            if (enableRuntime) TestRoutines.RuntimeTest(ExperimentType.Recovery, es, code);
                        }
                    }
                    
                    if (EnumMethods.EnableStreaming)
                    {
                        AlgoPack.EnsureFolderStructure(ExperimentType.Streaming, activeScenarios.Where(EnumMethods.IsStreaming).Select(es => es.ToLongString()).ToList());

                        foreach (ExperimentScenario es in activeScenarios.Where(EnumMethods.IsStreaming))
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
