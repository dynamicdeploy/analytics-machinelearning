/*  
Copyright (c) Microsoft.  All rights reserved.  Licensed under the MIT License.  See LICENSE in the root of the repository for license information 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using CommandLine;
using log4net;
using log4net.Core;
using AnomalyClientLib;
using CsvHelper;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace AnomalyClient
{
    /// <summary>
    /// Options for command line arguments.
    /// </summary>
    class Options
    {
        [Option('f', "input", Required = false,
          HelpText = "Input CSV file to be processed.", DefaultValue = "")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false,
        HelpText = "Output directory to write output files to. Default is a [guid].json the same directory as the executable.")]
        public string OutputFile { get; set; }

   
     
        [Option('v', "verbose", Required = false,
       HelpText = "Verbose output.")]
        public bool Verbose { get; set; }

        [Option('m', "max_anoms", Required = false,
     HelpText = "Maximum anomalies to return. Default is 0.1 (10%)", DefaultValue = 0.1)]
        public double MaxAnomaliesToReturn { get; set; }

        [Option('d', "direction", Required = false,
    HelpText = "Directionality of the anomalies to be detected. Supported values: [pos,neg,both]", DefaultValue = "both")]
        public string Direction { get; set; }

        [Option('t', "threshold", Required = false,
 HelpText = "Filter all negative anomalies and those anomalies whose magnitude is smaller than one of the specified thresholds which include: the median of the daily max values (med_max), the 95th percentile of the daily max values (p95), and the 99th percentile of the daily max values (p99). Supported Values: [None,med_max,p95,p99]", DefaultValue = "None")]
        public string Threshold { get; set; }

        [Option('a', "alpha", Required = false,
  HelpText = "The level of statistical significance with which to accept or reject anomalies.", DefaultValue = 0.05)]
        public double Alpha { get; set; }

        [Option('l', "onlylast", Required = false,
HelpText = "Find and report anomalies only within the last day or hr in the time series. Supported Values: [None,day,hr]", DefaultValue = "None")]
        public string OnlyLast { get; set; }

        [Option('e', "expectedvalue", Required = false, 
    HelpText = "Add an additional column to the anoms output containing the expected value.")]
        public bool AddExpectedValueColumn { get; set; }

        [Option('w', "piecewise", Required = false,
 HelpText = "The piecewise median time window as described in Vallis, Hochenbaum, and Kejariwal (2014). Defaults to 2.", DefaultValue = 2)]
        public int PiecewiseMedianTimeWindowInWeeks { get; set; }

        [Option('p', "plot", Required = false,
  HelpText = "A flag indicating if a plot with both the time series and the estimated anoms, indicated by circles, should also be returned.")]
        public bool CreatePlot { get; set; }

        [Option('g', "y_log", Required = false,
HelpText = "Apply log scaling to the y-axis. This helps with viewing plots that have extremely large positive anomalies relative to the rest of the data.")]
        public bool ApplyLogScaling { get; set; }

        [Option('x', "xlabel", Required = false,
HelpText = "X-axis label to be added to the output plot.", DefaultValue = "X")]
        public string XAxisLabel { get; set; }

        [Option('y', "ylabel", Required = false,
HelpText = "Y-axis label to be added to the output plot.", DefaultValue = "Y")]
        public string YAxisLabel { get; set; }

        [Option('z', "title", Required = false,
HelpText = "Title for the output plot.", DefaultValue = "Anomalies")]
        public string TitleForPlot { get; set; }

        [Option('n', "NA", Required = false,
HelpText = "Remove any NAs in timestamps.(default: FALSE).")]
        public bool RemoveNA { get; set; }

        [Option('r', "random", Required = false,
HelpText = "Generate random data.(default: FALSE).")]
        public bool GenerateRandomData { get; set; }

        [Option('s', "longterm", Required = false,
HelpText = "Increase anom detection efficacy for time series that are greater than a month. This option should be set when the input time series is longer than a month. The option enables the approach described in Vallis, Hochenbaum, and Kejariwal (2014).")]
        public bool IsLongtermTimeSeries { get; set; }
    }
  

    class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static bool verbose = false;
        static bool generateRandomData = false;

        
        static void Main(string[] args)
        {
            AnomalyDetectionInput inputParams = new AnomalyDetectionInput();
            //Process command line arguments
            try
            {
                var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
                if (!result.Errors.Any())
                {
                    string[,] inputData = null;
                    // Values are available here
                    if (!string.IsNullOrEmpty(result.Value.InputFile))
                    {
                        inputParams.InputDataFileName = result.Value.InputFile;
                        inputData = ReadCSVFile(inputParams.InputDataFileName);

                    }else
                    {
                        inputData = GenerateValues();
                    }

                    if (!string.IsNullOrEmpty(result.Value.OutputFile))
                        inputParams.OutputFileName = result.Value.OutputFile;


                   

                    verbose = result.Value.Verbose;
                    if (verbose)
                    {
                        SetVerbose();
                    }

                    //Web Service parameters
                    inputParams.AddExpectedValueColumn = result.Value.AddExpectedValueColumn;
                    inputParams.Alpha = result.Value.Alpha;
                    inputParams.ApplyLogScaling = result.Value.ApplyLogScaling;
                    inputParams.CreatePlot = result.Value.CreatePlot;
                    inputParams.Direction = result.Value.Direction;
                    inputParams.IsLongtermTimeSeries = result.Value.IsLongtermTimeSeries;
                    inputParams.MaxAnomaliesToReturn = result.Value.MaxAnomaliesToReturn;
                    inputParams.OnlyLast = result.Value.OnlyLast;
                    inputParams.PiecewiseMedianTimeWindowInWeeks = result.Value.PiecewiseMedianTimeWindowInWeeks;
                    inputParams.RemoveNAs = result.Value.RemoveNA;
                    inputParams.Threshold = result.Value.Threshold;
                    if(inputParams.CreatePlot)
                    {
                        inputParams.TitleForPlot = result.Value.TitleForPlot;
                        inputParams.XAxisLabel = result.Value.XAxisLabel;
                        inputParams.YAxisLabel = result.Value.YAxisLabel;
                    }

                    generateRandomData = result.Value.GenerateRandomData;
                    if(generateRandomData && inputData == null)
                    {
                        inputData = GenerateValues();
                    }

                    Log.Info("Calling AnomalyWSClient.InvokeRequestResponseService");

                    try
                    {
                       
                        AnomalyWSClient.InvokeRequestResponseService(inputData, inputParams).Wait();

                        Log.Info("End AnomalyWSClient.InvokeRequestResponseService");

                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("Error occurred while processing {0}.", ex.Message);
                        Log.Debug(ex);
                    }

                  
                    Log.Info("Processing complete.");
                   

                }//if
                else
                {
                    if (!(result.Errors.Count() == 1 && result.Errors.Take(1).FirstOrDefault().GetType() == typeof(HelpRequestedError)))
                    {

                        Log.Fatal("Error in input arguments");
                    }
                }




            }
            catch (Exception ex)
            {
               
                Log.ErrorFormat("Error occurred while processing {0}.",ex.Message);
                Log.Debug(ex);
            }
           

          //  Console.ReadLine();
        }

        private static string[,] GenerateValues()
        {
            Random r = new Random();
            string[,] bigArr = new string[3000, 2];
            int MAX_COUNT = 3000;
            for(int i=0; i < MAX_COUNT; i++)
            {
                bigArr[i, 0] = System.DateTime.UtcNow.AddMinutes(-1*(MAX_COUNT- i)).ToString("s");
                bigArr[i, 1] = ((i % 25 == 0)?i/r.NextDouble():(i * r.NextDouble())).ToString();

                
            }

            return bigArr;
        }

        private static void SetVerbose()
        {
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Level.All;
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
        }
     
        private static string[,] ReadCSVFile(string inputFile)
        {
            try
            {
                IList<string> errors = new List<string>();
                IList<string> validTimeRecord = new List<string>();
                IList<string> validValueRecord = new List<string>();
                bool hasHeaders = HasHeaders(inputFile);
                using (TextReader reader = File.OpenText(inputFile))
                {
                   
                    var csv = new CsvReader(reader);
                    if(hasHeaders)
                    {
                        csv.Configuration.HasHeaderRecord = true;
                    }
                    while (csv.Read())
                    {
                        try
                        {
                           
                            var time = csv.GetField<DateTime>(0);
                            bool validRow = false;
                            DateTime timeField;
                            if (csv.TryGetField(0, out timeField))
                            {
                                double valueField;
                                if (csv.TryGetField(1, out valueField))
                                {
                                    validRow = true;
                                    validTimeRecord.Add(timeField.ToString("s"));
                                    validValueRecord.Add(valueField.ToString());
                                }

                            }

                            if(!validRow)
                            {
                                errors.Add(csv.Row.ToString());
                            }

                        }catch(Exception yx)
                        {
                            Log.ErrorFormat("Error parsing CSV field. Omitting field. {0}", yx);
                        }
                    }//while

                   if(errors.Count > 0)
                    {
                        File.WriteAllLines("badrecords.log", errors.ToArray());
                    }
                   
                    int MAX_COUNT = validTimeRecord.Count;
                    string[,] bigArr = new string[MAX_COUNT, 2];
                    for (int i = 0; i < MAX_COUNT; i++)
                    {
                        bigArr[i, 0] = validTimeRecord[i];
                        bigArr[i, 1] = validValueRecord[i];


                    }

                    return bigArr;

                }

            }catch(Exception ex)
            {
                Log.FatalFormat("Error while reading csv file {0}.", ex.Message);

                throw ex;
            }

           
        }

        private static bool HasHeaders(string inputFile)
        {
            bool hasHeaders = false;
            using (TextReader reader = File.OpenText(inputFile))
            {

                var csv = new CsvReader(reader);

                csv.Read();
                try
                {
                    var time = csv.GetField<DateTime>(0);

                }catch(Exception)
                {
                    hasHeaders = true;

                }
            }

            return hasHeaders;

        }
    }
}
