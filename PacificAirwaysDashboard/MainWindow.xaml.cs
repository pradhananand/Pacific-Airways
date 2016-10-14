using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PacificAirwaysDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Declare a System.Threading.CancellationTokenSource.
        CancellationTokenSource cts;
        StatsService para = new StatsService();

        private DateTime startDate;
        private DateTime endDate;

        public MainWindow()
        {
            InitializeComponent();

            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("Seattle");
            list.Add("LosAngeles");
            list.Add("PacificEurope");
            list.Add("Denver");
            list.Add("NewYork");
            list.Add("Chicago");
            list.Add("Miami");
            list.Add("PAY");
            this.comboBox.ItemsSource = list;

        }


        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: RESET ???

            //// ... Get the ComboBox.
            //var comboBox = sender as ComboBox;

            //// ... Set SelectedItem as Window Title.
            //string value = comboBox.SelectedItem as string;
            //this.Title = "Selected: " + value;
        }

        private void DoIndependentWork()
        {
            this.Error.Text = "Processing Logs...";
        }

        private async void downloadPireps_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate the CancellationTokenSource.
            cts = new CancellationTokenSource();

            // check if file was downloaded today
            // check for file modified date
            // if file modified date == today
            // return (do not download)

            try
            {
                this.downloadPireps.IsEnabled = false;
                this.calculateHubStats.IsEnabled = false;
                DoIndependentWork();

                await AccessTheWebAsync(cts.Token);

                Reset(true);
                this.Error.Text = "Download is complete!";
                this.calculateHubStats.IsEnabled = true;
                this.downloadPireps.IsEnabled = false;
                this.ResetAllBtn.IsEnabled = true;
            }
            catch (OperationCanceledException)
            {
                this.Error.Text += "\r\nDownloads canceled.\r\n";
            }
            catch (Exception exception)
            {
                this.Error.Text += "\r\nDownloads failed.\r\n";
                this.Error.Text += exception.Message.ToString();
            }

            cts = null;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }


        async Task AccessTheWebAsync(CancellationToken ct)
        {
            HttpClient client = new HttpClient() { MaxResponseContentBufferSize = Int32.MaxValue };

            // Make a list of web addresses.
            Dictionary<string, string> urlList = SetUpURLList();

            // ***Create a query that, when executed, returns a collection of tasks.
            IEnumerable<Task<int>> downloadTasksQuery =
                from url in urlList select ProcessURL(url.Key, url.Value, client, ct);

            // ***Use ToList to execute the query and start the tasks. 
            List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

            // ***Add a loop to process the tasks one at a time until none remain.
            while (downloadTasks.Count > 0)
            {
                // Identify the first task that completes.
                Task<int> firstFinishedTask = await Task.WhenAny(downloadTasks);

                // ***Remove the selected task from the list so that you don't
                // process it more than once.
                downloadTasks.Remove(firstFinishedTask);

                // Await the completed task.
                int length = await firstFinishedTask;
                //resultsTextBox.Text += String.Format("\r\nLength of the download:  {0}", length);
            }
        }

        private Dictionary<string, string> SetUpURLList()
        {
            Dictionary<string, string> urls = new Dictionary<string, string>
            {
                {"Seattle", "http://prs.pacificairways.net/api/hubs/PAY/1/logbooks" },
                {"Miami", "http://prs.pacificairways.net/api/hubs/PAY/2/logbooks" },
                {"Denver", "http://prs.pacificairways.net/api/hubs/PAY/3/logbooks" },
                {"Chicago", "http://prs.pacificairways.net/api/hubs/PAY/5/logbooks" },
                {"NewYork", "http://prs.pacificairways.net/api/hubs/PAY/6/logbooks" },
                {"LosAngeles", "http://prs.pacificairways.net/api/hubs/PAY/7/logbooks" },
                {"PacificEurope", "http://prs.pacificairways.net/api/hubs/PAY/8/logbooks" },
            };
            return urls;
        }


        async Task<int> ProcessURL(string hub, string url, HttpClient client, CancellationToken ct)
        {
            // GetAsync returns a Task<HttpResponseMessage>. 
            HttpResponseMessage response = await client.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                // Retrieve the website contents from the HttpResponseMessage.
                byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

                if (urlContents.Length == 0)
                {
                    //resultsTextBox.Text += String.Format("\r\nNo data for:  {0}", hub);
                    return -1;
                }


                // convert string to stream
                using (MemoryStream stream = new MemoryStream(urlContents))
                {
                    // Reference: http://www.anotherchris.net/csharp/6-ways-to-get-the-current-directory-in-csharp/
                    using (var fileStream = File.Create(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory) + hub + ".txt"))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                return urlContents.Length;
            }
            return -1;
        }

        private void calculateHubStats_Click(object sender, RoutedEventArgs e)
        {
            var hubLogBooks = new List<Log>();
            var paths = new List<string>();
            var hub = comboBox.SelectedItem as string;
            
            switch (hub)
            {
                case "Seattle":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Seattle.txt");
                    break;
                case "LosAngeles":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "LosAngeles.txt");
                    break;
                case "PacificEurope":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "PacificEurope.txt");
                    break;
                case "Denver":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Denver.txt");
                    break;
                case "NewYork":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "NewYork.txt");
                    break;
                case "Chicago":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Chicago.txt");
                    break;
                case "Miami":
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Miami.txt");
                    break;
                case "PAY":
                default:
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Seattle.txt");
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "LosAngeles.txt");
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "PacificEurope.txt");
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Denver.txt");
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "NewYork.txt");
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Chicago.txt");
                    paths.Add(@AppDomain.CurrentDomain.BaseDirectory + "Miami.txt");
                    break;
            }

            foreach (var path in paths)
            {
                string json = File.ReadAllText(path);
                hubLogBooks.AddRange(JsonConvert.DeserializeObject<List<Log>>(json));
            }

            // Apply filter on Pireps
            startDate = this.StartDate.SelectedDate.HasValue ? this.StartDate.SelectedDate.Value.Date : DateTime.MinValue.Date;
            endDate = this.EndDate.SelectedDate.HasValue ? this.EndDate.SelectedDate.Value.Date : DateTime.MaxValue.Date;
            if (hub == null) hub = "PAY";
            List<Log> filteredPireps = para.GetFilteredLogs(hubLogBooks, hub, startDate, endDate );

            // Generate Report on filtered Pireps

            try
            {
                Reset(true);
                if (filteredPireps.Count > 0)
                {
                    this.TotalHours.Text = this.para.GetTotalHours(filteredPireps).ToString();
                    this.TotalFlights.Text = this.para.GetTotalFlights(filteredPireps).ToString();
                    this.TotalVatsimHours.Text = this.para.GetTotalVatsimHours(filteredPireps).ToString();
                    this.TotalVatsimFlights.Text = this.para.GetTotalVastimFlights(filteredPireps).ToString();
                    this.TotalCharterFlights.Text = this.para.GetCountOfTotalChartersFlown(filteredPireps).ToString();
                    this.SpecialEventsFlown.Text = this.para.GetCountOfTotalSpecialEventsParticipation(filteredPireps).ToString();



                    List<dynamic> Top5PilotsByNumberOfFlights;
                    Top5PilotsByNumberOfFlights = this.para.GetTopPilotByFlights(filteredPireps);
                    foreach (dynamic item in Top5PilotsByNumberOfFlights)
                    {
                        Top5PilotsByNumberOfFlightsText.Text += item.Pid + " " + item.Name + " " + item.ProductCount + " flights.\n";
                    }


                    List<dynamic> Top5PilotsByHours;
                    Top5PilotsByHours = this.para.GetTop5PilotsByHours(filteredPireps);
                    foreach (dynamic item in Top5PilotsByHours)
                    {
                        Top5PilotsByHoursText.Text += item.Pid + " " + item.Name + " " + item.TotalFlyingTime + " hours.\n";
                    }

                    List<dynamic> Top5PilotsByVatsimHours;
                    Top5PilotsByVatsimHours = this.para.GetTop5PilotsByVatsimHours(filteredPireps);
                    foreach (dynamic item in Top5PilotsByVatsimHours)
                    {
                        Top5PilotsByVatsimHoursText.Text += item.Pid + " " + item.Name + " " + item.TotalFlyingTime + " hours.\n";
                    }

                    List<dynamic> Top5PilotsByNumberOfChartersFlown;
                    Top5PilotsByNumberOfChartersFlown = this.para.GetTop5PilotsByNumberOfCharterFlown(filteredPireps);
                    foreach (dynamic item in Top5PilotsByNumberOfChartersFlown)
                    {
                        Top5PilotsByChartersFlown.Text += item.Pid + " " + item.Name + " " + item.ProductCount + " flights.\n";
                    }

                    List<dynamic> Top5PilotsByNumberOfSpecialEventsFlown;
                    Top5PilotsByNumberOfSpecialEventsFlown = this.para.GetTop5PilotsByNumberOfSpecialEventsFlown(filteredPireps);
                    foreach (dynamic item in Top5PilotsByNumberOfSpecialEventsFlown)
                    {
                        Top5PilotsByNumberOfSpecialEventsFlownText.Text += item.Pid + " " + item.Name + " " + item.ProductCount + " flights.\n";
                    }

                    List<dynamic> Top10MostflownRoutes;
                    Top10MostflownRoutes = this.para.GetTop10MostFlownRoutes(filteredPireps);
                    foreach (dynamic item in Top10MostflownRoutes)
                    {
                        Top10MostFlownRoutesTxt.Text += "Dep: " + item.Dep + " Arr: " + item.Arr + " # " + item.NumberOfTimesFlown + ".\n";

                    }

                    //List<dynamic> HubTop10PilotsByFlights;
                    //HubTop10PilotsByFlights = this.para.GetHubTop10PilotsByFlights(filteredPireps);
                    //foreach (dynamic item in HubTop10PilotsByFlights)
                    //{
                    //    HubTop10PilotsByFlightsTxt.Text += "Rank " + item.rank + " " + item.pid + " Flights # " + item.count + ".\n";
                    //}


                    //this.CalclulateHubStatsBtn.IsEnabled = false;
                    this.calculateHubStats.IsEnabled = false;
                    this.ResetAllBtn.IsEnabled = true;
                    this.Error.Text = "Successfully Processed Log file!!!";
                }
                else
                {
                    throw new System.ArgumentException("Please Enter path to log file or click Get Hub Logs", "filteredPireps");
                }
            }
            catch (Exception ex)
            {
                this.Error.Text = ex.Message.ToString();
            }
        }

        private void Reset(bool check)
        {
            if (check)
            {
                this.TotalHours.Text = "";
                this.TotalVatsimHours.Text = "";
                this.TotalFlights.Text = "";
                this.TotalVatsimFlights.Text = "";
                this.TotalCharterFlights.Text = "";
                this.SpecialEventsFlown.Text = "";
                this.Top5PilotsByHoursText.Text = "";
                this.Top5PilotsByNumberOfFlightsText.Text = "";
                this.Top5PilotsByChartersFlown.Text = "";
                this.Top10MostFlownRoutesTxt.Text = "";
                //this.HubTop10PilotsByFlightsTxt.Text = "";
                this.Top5PilotsByNumberOfSpecialEventsFlownText.Text = "";
                this.Top5PilotsByVatsimHoursText.Text = "";
            }
            //this.CalclulateHubStatsBtn.IsEnabled = false;
            this.calculateHubStats.IsEnabled = true;
            this.ResetAllBtn.IsEnabled = false;
        }

        private void ResetAllBtn_Click(object sender, RoutedEventArgs e)
        {
            Reset(true);
        }
    }
}

