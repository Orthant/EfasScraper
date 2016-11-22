using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Concurrent;
using JavaScriptSerialiser = System.Web.Script.Serialization.JavaScriptSerializer;
using System.Collections.Specialized;

namespace EfasExtract
{
    [Serializable]
    public class CourtDate
    {
        public int Day { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }
        public DateTimeOffset Date { get; private set; }
        public int TotalHearings
        {
            get
            {
                return this.Hearings.Count();
            }
        }


        public ConcurrentBag<ResponseDataHearing> Hearings = new ConcurrentBag<ResponseDataHearing>();

        public CourtDate(DateTimeOffset date)
        {
            this.Date = date;
            this.Day = date.Day;
            this.Month = date.Month;
            this.Year = date.Year;
        }

        public CourtDate(DateTimeOffset date, IEnumerable<ResponseDataHearing> hearings)
            : this(date)
        {
            this.Hearings = new ConcurrentBag<ResponseDataHearing>(hearings);
        }

        public void AddHearing(ResponseDataHearing hearing)
        {
            Hearings.Add(hearing);
        }

        public int JudicialMonitoring { get { return this.Hearings.Count(k => k.HearingType == "Judicial Monitoring"); } }
        public int ForSentence { get { return this.Hearings.Count(k => k.HearingType == "For Sentence"); } }
        public int SpecialMention { get { return this.Hearings.Count(k => k.HearingType == "Special Mention"); } }
        public int ExParte { get { return this.Hearings.Count(k => k.HearingType == "Ex-Parte"); } }
        public int Bond { get { return this.Hearings.Count(k => k.HearingType == "Bond"); } }
        public int Diversion { get { return this.Hearings.Count(k => k.HearingType == "Diversion"); } }
        public int FamilyViolenceCourtDivision { get { return this.Hearings.Count(k => k.HearingType == "Family Violence Court Division"); } }
        public int PleaGuilty { get { return this.Hearings.Count(k => k.HearingType == "Plea Guilty"); } }
        public int Hearing { get { return this.Hearings.Count(k => k.HearingType == "Hearing"); } }
        public int Breach { get { return this.Hearings.Count(k => k.HearingType == "Breach"); } }
        public int ContestMention { get { return this.Hearings.Count(k => k.HearingType == "Contest Mention"); } }
        public int InfringementWarrant { get { return this.Hearings.Count(k => k.HearingType == "Infringement Warrant"); } }
        public int Enforcement { get { return this.Hearings.Count(k => k.HearingType == "Enforcement"); } }
        public int DrugCourt { get { return this.Hearings.Count(k => k.HearingType == "Drug Court"); } }
        public int RestorationOfSuspendedSentence { get { return this.Hearings.Count(k => k.HearingType == "Restoration Of Suspended Sentence"); } }
        public int Vary { get { return this.Hearings.Count(k => k.HearingType == "Vary"); } }
        public int Application { get { return this.Hearings.Count(k => k.HearingType == "Application"); } }
        public int S26RoadSafetyAct { get { return this.Hearings.Count(k => k.HearingType == "S26 Road Safety Act"); } }
        public int Mention { get { return this.Hearings.Count(k => k.HearingType == "Mention"); } }
    }

    class Program
    {
        // 
        static readonly JavaScriptSerialiser json = new JavaScriptSerialiser { MaxJsonLength = Int32.MaxValue };
        public readonly static string[] hearingTypes = new string[] {
            "Judicial Monitoring",
            "For Sentence",
            "Special Mention",
            "Ex-Parte",
            "Bond",
            "Diversion",
            "Family Violence Court Division",
            "Plea Guilty",
            "Hearing",
            "Breach",
            "Contest Mention",
            "Infringement Warrant",
            "Enforcement",
            "Drug Court",
            "Restoration Of Suspended Sentence",
            "Vary",
            "Application",
            "S26 Road Safety Act",
            "Mention"
        };

        static List<string> hearingTypesEncountered = new List<string>(hearingTypes);

        static ConcurrentBag<CourtDate> courtDates = new ConcurrentBag<CourtDate>();

        const string USERAGENT = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36 EfasScraper/0.95.3.2126";
        private static string GenerateDateUrl(DateTimeOffset date)
        {
            return string.Format("https://dailylists.magistratesvic.com.au/EFAS/CaseBrowse_Cases_GridData?CaseType=CRI&CourtID=4&HearingDate={0}%2F{1}%2F{2}%2000%3A00%3A00", date.Month, date.Day, date.Year);
        }
        static DateTimeOffset started = DateTimeOffset.Now;
        static void WriteTee(string message)
        {
            Console.WriteLine("{0} - {1}", DateTimeOffset.Now, message);
        }
        static void Main(string[] args)
        {
            started = DateTimeOffset.Now;
            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
            ServicePointManager.DnsRefreshTimeout = Int32.MaxValue;
            ServicePointManager.MaxServicePoints = Int32.MaxValue;
            ServicePointManager.CheckCertificateRevocationList = false;

            //string url = "https://dailylists.magistratesvic.com.au/EFAS/CaseSearch_GridData";
            //string response;
            //using (WebClient webClient = new WebClient())
            //{
            //    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            //    webClient.Headers.Add(HttpRequestHeader.UserAgent, USERAGENT);
            //    webClient.Headers.Add(HttpRequestHeader.Accept, "application/json; charset=utf-8");

            //    response = webClient.UploadString(url, "sort=&page=1&pageSize=150000&group=&filter=&CaseType=CRI&CourtID=4&HearingDate=&CourtLinkCaseNo=&PlaintiffInformantApplicant=&DefendantAccusedRespondent=&HearingType=");
            //}

            //ResponseData responseData = json.Deserialize<ResponseData>(response);

            ConcurrentExclusiveSchedulerPair sch = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Current, Int32.MaxValue);
            //Parallel.ForEach<ResponseDataHearing>(responseData.Data, new ParallelOptions { MaxDegreeOfParallelism = -1, TaskScheduler = sch.ConcurrentScheduler }, (ResponseDataHearing h) =>
            //{
            //    string urlB = string.Format("https://dailylists.magistratesvic.com.au/EFAS/Case{0}?CaseID={1}", string.IsNullOrWhiteSpace(h.CaseType) ? "CRI" : h.CaseType, h.CaseID);
            //    string data;
            //    using (WebClient hearingClient = new WebClient())
            //    {
            //        data = hearingClient.DownloadString(urlB);
            //    }
            //    string dataRead = data.Substring(data.IndexOf("<label for=\"ProsecutingAgency\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    dataRead = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();
            //    h.ProsecutingAgency = dataRead;

            //    dataRead = data.Substring(data.IndexOf("<label for=\"Informant\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.Informant = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

            //    dataRead = data.Substring(data.IndexOf("<label for=\"ProsecutorRepresentative\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.ProsecutorRepresentative = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

            //    dataRead = data.Substring(data.IndexOf("<label for=\"Accused\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.Accused = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

            //    dataRead = data.Substring(data.IndexOf("<label for=\"AccusedRepresentative\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.AccusedRepresentative = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

            //    dataRead = data.Substring(data.IndexOf("<label for=\"HearingType\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.HearingType = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

            //    dataRead = data.Substring(data.IndexOf("<label for=\"Plea\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.Plea = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

            //    dataRead = data.Substring(data.IndexOf("<label for=\"CourtRoom\">"));
            //    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
            //    h.CourtRoom = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();
            //});

            //IEnumerable<CourtDate> hearings = responseData.Data
            //    .OrderBy(d => d.HearingDateTime)
            //    .GroupBy(k => k.HearingDateTime.Date)
            //    .Select(k => new CourtDate(k.Key, k))
            //    .ToArray();

            Parallel.For(0, 366, new ParallelOptions { MaxDegreeOfParallelism = -1, TaskScheduler = sch.ConcurrentScheduler }, (int i) =>
            {
                DateTimeOffset procDate = started.AddDays(i);
                CourtDate thisCourtDate = new CourtDate(procDate);
                string urlD = GenerateDateUrl(procDate);
                string responsed;
                //Console.WriteLine("{0}: Started", procDate.ToString("yyyy-MMM-dd"));
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, USERAGENT);
                    //Console.WriteLine("{0}: Web Request Being Sent", procDate.ToString("yyyy-MMM-dd"));
                    responsed = webClient.UploadString(urlD, "sort=CourtLinkCaseNo-desc&page=1&pageSize=500&group=&filter=");
                }
                //Console.WriteLine("{0}: Web Response Received [{1} bytes]", procDate.ToString("yyyy-MMM-dd"), response.Length);
                ResponseData responseData = json.Deserialize<ResponseData>(responsed);
                //Console.WriteLine("{0}: Processing hearings [{1} total]", procDate.ToString("yyyy-MMM-dd"), responseData.Data.Length);
                Parallel.ForEach<ResponseDataHearing>(responseData.Data, new ParallelOptions { MaxDegreeOfParallelism = -1, TaskScheduler = sch.ConcurrentScheduler },
                    (ResponseDataHearing h) =>
                {
                    //Console.WriteLine("{0}-{1}: Started", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo);
                    string urlB = string.Format("https://dailylists.magistratesvic.com.au/EFAS/Case{0}?CaseID={1}", string.IsNullOrWhiteSpace(h.CaseType) ? "CRI" : h.CaseType, h.CaseID);
                    string data;
                    using (WebClient webClientB = new WebClient())
                    {
                        //Console.WriteLine("{0}-{1}: Sending web request", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo);
                        data = webClientB.DownloadString(urlB);
                        //Console.WriteLine("{0}-{1}: Web Response Received [{2} bytes]", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo, data.Length);
                    }
                    string dataRead = data.Substring(data.IndexOf("<label for=\"ProsecutingAgency\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    dataRead = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();
                    h.ProsecutingAgency = dataRead;

                    dataRead = data.Substring(data.IndexOf("<label for=\"Informant\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.Informant = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    dataRead = data.Substring(data.IndexOf("<label for=\"ProsecutorRepresentative\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.ProsecutorRepresentative = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    dataRead = data.Substring(data.IndexOf("<label for=\"Accused\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.Accused = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    dataRead = data.Substring(data.IndexOf("<label for=\"AccusedRepresentative\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.AccusedRepresentative = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    dataRead = data.Substring(data.IndexOf("<label for=\"HearingType\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.HearingType = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    dataRead = data.Substring(data.IndexOf("<label for=\"Plea\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.Plea = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    dataRead = data.Substring(data.IndexOf("<label for=\"CourtRoom\">"));
                    dataRead = dataRead.Substring(dataRead.IndexOf("<td>"));
                    h.CourtRoom = dataRead.Substring(4, dataRead.IndexOf("</td>") - 4).Trim();

                    //Console.WriteLine("{0}-{1}: Is a '{2}' hearing", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo, r.HearingType);
                    thisCourtDate.AddHearing(h);

                    //Console.WriteLine("{0}-{1}: done", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo);
                });

                courtDates.Add(thisCourtDate);
                //Console.WriteLine("{0}: done", procDate.ToString("yyyy-MMM-dd"));
            });

            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(started.ToString("yyyyMMdd-HHmm"), ".json")), json.Serialize(courtDates));
            Console.Write("Done");
            Console.Beep();
            Console.ReadKey();
        }
    }
    public class ResponseData
    {
        public ResponseDataHearing[] Data;
        public int Total;
        public object[] AggregateResults;
        public object[] Errors;
    }


    public class ResponseDataHearing
    {
        public int CaseID { get; set; }
        public string CourtLinkCaseNo { get; set; }
        public string PlaintiffInformantApplicant { get; set; }
        public string CaseType { get; set; }
        public string DefandantAccusedRespondent { get; set; }
        public string Court { get; set; }
        public string InformantDivision { get; set; }
        public string ProsecutingAgency { get; set; }
        public string Informant { get; set; }
        public string ProsecutorRepresentative { get; set; }
        public string Accused { get; set; }
        public string AccusedRepresentative { get; set; }
        public string HearingType { get; set; }
        public string Plea { get; set; }
        public string CourtRoom { get; set; }
        public DateTimeOffset HearingDateTime { get; set; }

    }
}
