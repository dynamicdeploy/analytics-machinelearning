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
using System.Configuration;
using log4net;


namespace AnomalyClientLib
{
   public class AnomalyWSClient
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Dictionary<string, string> InputParameters = new Dictionary<string, string>() {
        { "Max anomalies to return", "0.1" },
        { "Direction", "both" },
        { "Alpha", "0.05" },
        { "Only Last", "None" },
        { "Threshold", "None" },
        { "Add Expected Value Column", "FALSE" },
        { "Longterm Time Series", "" },
        { "Piecewise median time window", "2" },
        { "Log Scaling", "" },
        { "Remove NAs", "TRUE" },
            { "Create Plot", "TRUE" },
        { "X-axis label", "X" },
        { "Y-axis label", "Y" },
        { "Title for plot", "Anomalies" }
        };

        private static void PrintInputParameters()
        {
            foreach(KeyValuePair<string, string> kv in InputParameters)
            {
                Log.InfoFormat("{0}={1}", kv.Key, kv.Value);
            }

           
        }
        private static void FillInputParameters(AnomalyDetectionInput inputParameters)
        {
            if(inputParameters != null)
            {
                InputParameters["Max anomalies to return"] = inputParameters.MaxAnomaliesToReturn.ToString();

                InputParameters["Direction"] = inputParameters.Direction;

            
                InputParameters["Alpha"] = inputParameters.Alpha.ToString();

                InputParameters["Only Last"] = inputParameters.OnlyLast;
                InputParameters["Threshold"] = inputParameters.Threshold;

             

                InputParameters["Add Expected Value Column"] = inputParameters.AddExpectedValueColumn.ToString().ToUpper();

                if(inputParameters.IsLongtermTimeSeries != null)
                {
                    InputParameters["Longterm Time Series"] = inputParameters.IsLongtermTimeSeries.ToString().ToUpper();
                }

                InputParameters["Piecewise median time window"] = inputParameters.PiecewiseMedianTimeWindowInWeeks.ToString();

                if(inputParameters.ApplyLogScaling != null)
                {
                    InputParameters["Log Scaling"] = inputParameters.ApplyLogScaling.ToString().ToUpper();

                }

                InputParameters["Remove NAs"] = inputParameters.RemoveNAs.ToString().ToUpper();

                InputParameters["Create Plot"] = inputParameters.CreatePlot.ToString().ToUpper();

                InputParameters["X-axis label"] = inputParameters.XAxisLabel;

                InputParameters["Y-axis label"] = inputParameters.YAxisLabel;

                InputParameters["Title for plot"] = inputParameters.TitleForPlot;
            }

        }
        public static async Task<Rootobject> InvokeRequestResponseService(string[,] timeSeriesValues, AnomalyDetectionInput inputParameters=null)
        {
            Log.Info("InvokeRequestResponseService");
            if(inputParameters != null)
            {
                Log.Info("Detected input parameters.");
                FillInputParameters(inputParameters);
                if(Log.IsInfoEnabled)
                {
                    PrintInputParameters();
                }
                
            }
          

            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                               ColumnNames = new string[] {"timestamp", "count"},
                               // Values = new string[,] {  { "", "0" },  { "", "0" },  }
                                Values = timeSeriesValues
                            }
                        },
                                        },
                    GlobalParameters = InputParameters
                };
                string apiKey = ConfigurationManager.AppSettings["AnomalyDetectionApiKey"]; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                string url = ConfigurationManager.AppSettings["AnomalyDetectionWebServiceUrl"];
                Log.InfoFormat("Url {0}", url);
                client.BaseAddress = new Uri(url);

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Log.InfoFormat("Result: {0}", result);
                                     
                
                    if (!string.IsNullOrEmpty(result))
                    {
                        

                        Rootobject ro = JsonConvert.DeserializeObject<Rootobject>(result);

                        Results r = ro.Results;

                        Value v = r.output1.value;

                        string[][] values = v.Values;

                        StringBuilder finalMatrixStrb = new StringBuilder();
                        StringBuilder columnNamesBuffer = new StringBuilder();
                        foreach (string cn in v.ColumnNames)
                        {

                            columnNamesBuffer.Append(string.Format("{0},", cn));
                           
                        }
                        string columnNames = columnNamesBuffer.ToString();
                        if (columnNames.EndsWith(","))
                        {
                            columnNames = columnNames.Remove(columnNames.LastIndexOf(','), 1);
                        }
                        finalMatrixStrb.AppendLine(columnNames);
                       
                                              
                        for(int i = 0; i< values.Length; i++)
                        {
                            StringBuilder eachRow = new StringBuilder();
                            for(int j = 0; j < values[i].Length; j++)
                            {
                                string s = values[i][j];
                                string si = string.Format("{0},", s);
                                eachRow.Append(si);
                                
                            }
                            string finalRowTemp = eachRow.ToString();
                            if (finalRowTemp.EndsWith(","))
                            {
                                finalRowTemp = finalRowTemp.Remove(finalRowTemp.LastIndexOf(','), 1);
                            }
                            finalMatrixStrb.AppendLine(finalRowTemp);
                           
                           
                      
                        }



                        string resultGuid = System.Guid.NewGuid().ToString("N");
                        string fileName = string.Format("{0}.csv", resultGuid);

                        if (!string.IsNullOrEmpty(inputParameters.OutputFileName))
                        {
                            fileName = inputParameters.OutputFileName;
                        }
                        File.WriteAllText(fileName, finalMatrixStrb.ToString());

                        string opr = r.output2.value.Values[0][0];

                     
                        JObject jo = JObject.Parse(opr);

                        if (jo != null)
                        {
                            JToken jt = jo.Last;
                            var children = jo.Children();
                            string imgb64 = null;
                            int i = 0;
                            foreach (JProperty j in children)
                            {
                                if (j.Name == "Graphics Device")
                                {
                                    imgb64 = j.Value.First.Value<string>();

                                    if (!string.IsNullOrEmpty(imgb64))
                                    {
                                        Image ig = Base64ToImage(imgb64);
                                        //string guid = System.Guid.NewGuid().ToString("N");
                                        string fn = Path.GetFileNameWithoutExtension(fileName);
                                        fileName = string.Format("{0}-{1}.png", fn, i++);
                                        ig.Save(fileName);
                                    }
                                }

                            }

                            //var v = imstr.FirstOrDefault();

                        }
                        return ro;
                    }
                }
                else
                {
                    Log.FatalFormat("The request failed with status code: {0}", response.StatusCode);

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Log.Fatal(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Log.Fatal(responseContent);
                }
            }

            Log.Info("End InvokeRequestResponseService");
            return null;
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
    }
}
