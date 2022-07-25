using KockpitPortal.Utility;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KockpitPortal.Models
{

    public class Common
    {

    }

    public class Response
    {
        public bool success { get; set; }
        public string msg { get; set; }
    }

    public class MyPlans
    {
        public string PlanName { get; set; }
        public string CompanyName { get; set; }
        public string EmailId { get; set; }
        public string Logo { get; set; }
        public string SubscriptionId { get; set; }
        public int ClientId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        public int TotalKeys { get; set; }
        public int UsedKeys { get; set; }
        public int UnusedKeys { get; set; }
        public int ExpiredKeys { get; set; }
        public int TransferedKeys { get; set; }

        public int Id { get; set; }
        public string LicenseKey { get; set; }
        public int ValidityDays { get; set; }
        public string AssignTo { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string TransferFrom { get; set; }
        public bool? IsExpired { get; set; }
        public bool Iswarranty { get; set; }
        public DateTime? WarrantyEndDate { get; set; }

        public int PlanId { get; set; }

        public bool? IsAMCExpired { get; set; }
        public bool? ISAMCcancelled { get; set; }
        public bool? IsAMCReissue { get; set; }
        public int? AMCId { get; set; }
        public bool IsTransfered { get; set; }

        public int? RemainingDays { get; set; }
        public DateTime? AMCStartDate { get; set; }
        public DateTime? AMCEndDate { get; set; }

    }
    public class WorldClockAPI
    {
        public string id { get; set; }
        public string currentDateTime { get; set; }
        public string utcOffset { get; set; }
        public string isDayLightSavingsTime { get; set; }
        public string dayOfTheWeek { get; set; }
        public string timeZoneName { get; set; }
        public string currentFileTime { get; set; }
        public string ordinalDate { get; set; }
        public string serviceResponse { get; set; }
    }

    public class GetCurrentDateTime
    {
       
        IConfiguration _baseConfig;
        private ApiManager apiManager;
        public GetCurrentDateTime( IConfiguration sconfig)
        {
            _baseConfig = sconfig;
        }

        public async Task<DateTime> CurrentDatetime()
        {
            try
            {
                string apiUrl = _baseConfig["ClockAPI"];
                using (var client = new HttpClient())
                {
                    using (var response = client.GetAsync(apiUrl).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            WorldClockAPI clockAPI = JsonConvert.DeserializeObject<WorldClockAPI>(json);
                            DateTime dtCurrentDatetime;
                            if (DateTime.TryParse(clockAPI.currentDateTime, out dtCurrentDatetime))
                            {
                                return dtCurrentDatetime;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return DateTime.Now;
        }
    }
}
