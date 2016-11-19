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
        private  static string GenerateDateUrl(DateTimeOffset date)
        {
            return string.Format("https://dailylists.magistratesvic.com.au/EFAS/CaseBrowse_Cases_GridData?CaseType=CRI&CourtID=4&HearingDate={0}%2F{1}%2F{2}%2000%3A00%3A00", date.Month, date.Day, date.Year);
        }
        static void Main(string[] args)
        {
            DateTimeOffset today = DateTimeOffset.Now;
            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
            ServicePointManager.DnsRefreshTimeout = Int32.MaxValue;
            ServicePointManager.MaxServicePoints = Int32.MaxValue;
            ServicePointManager.CheckCertificateRevocationList = false;


            ConcurrentExclusiveSchedulerPair sch = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Current, Int32.MaxValue);
            Parallel.For(0, 366, new ParallelOptions { MaxDegreeOfParallelism = -1, TaskScheduler = sch.ConcurrentScheduler }, (int i) =>
            {
                DateTimeOffset procDate = today.AddDays(i);
                CourtDate thisCourtDate = new CourtDate(procDate);
                string url = GenerateDateUrl(procDate);
                Console.WriteLine("{0}: Started", procDate.ToString("yyyy-MMM-dd"));
                WebClient webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
                webClient.Headers.Add(HttpRequestHeader.UserAgent, USERAGENT);
                Console.WriteLine("{0}: Web Request Being Sent", procDate.ToString("yyyy-MMM-dd"));
                string response = webClient.UploadString(url, "sort=CourtLinkCaseNo-desc&page=1&pageSize=500&group=&filter=");
                Console.WriteLine("{0}: Web Response Received [{1} bytes]", procDate.ToString("yyyy-MMM-dd"), response.Length);
                ResponseData responseData = json.Deserialize<ResponseData>(response);
                Console.WriteLine("{0}: Processing hearings [{1} total]", procDate.ToString("yyyy-MMM-dd"), responseData.Data.Length);
                Parallel.ForEach<ResponseDataHearing>(responseData.Data, new ParallelOptions { MaxDegreeOfParallelism = -1, TaskScheduler = sch.ConcurrentScheduler }, (ResponseDataHearing r) =>
                {
                    Console.WriteLine("{0}-{1}: Started", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo);
                    string urlB = string.Format("https://dailylists.magistratesvic.com.au/EFAS/CaseCRI?CaseID={0}", r.CaseID);
                    WebClient webClientB = new WebClient();
                    Console.WriteLine("{0}-{1}: Sending web request", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo);
                    string data = webClientB.DownloadString(urlB);
                    Console.WriteLine("{0}-{1}: Web Response Received [{2} bytes]", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo, data.Length);
                    string hearingTypeStr = data.Substring(data.IndexOf("<label for=\"HearingType\">"));
                    hearingTypeStr = hearingTypeStr.Substring(hearingTypeStr.IndexOf("<td>"));
                    hearingTypeStr = hearingTypeStr.Substring(4, hearingTypeStr.IndexOf("</td>")- 4).Trim();
                    r.HearingType = hearingTypeStr;
                    Console.WriteLine("{0}-{1}: Is a '{2}' hearing", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo, r.HearingType);
                    thisCourtDate.AddHearing(r);
                    Console.WriteLine("{0}-{1}: done", procDate.ToString("yyyy-MMM-dd"), r.CourtLinkCaseNo);
                });

                courtDates.Add(thisCourtDate);
                Console.WriteLine("{0}: done", procDate.ToString("yyyy-MMM-dd"));
            });

            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(today.ToString("yyyyMMdd-hhmmss"), ".efas")),json.Serialize(courtDates.OrderBy(o => o.Date)));
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
        public string CaseID;
        public string CourtLinkCaseNo;
        public string PlaintiffInformantApplicant;
        public string DefendantAccusedRespondent;
        public string InformantDivision;
        public string HearingType;
        
    }
}
