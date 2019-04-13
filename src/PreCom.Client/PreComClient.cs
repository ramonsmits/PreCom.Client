using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PreCom
{
    public class PreComClient
    {
        const string UrlBase = "https://pre-com.nl/Mobile/";
        static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        readonly string UserAgent = "PreComClient/1.1 (https://github.com/ramonsmits/PreCom.Client) ";
        readonly HttpClient httpClient;

        public static readonly string[] HourKeys = GenerateHourKeys();

        static string[] GenerateHourKeys()
        {
            var hourKeys = new string[24];
            for (var i = 0; i < 24; i++) hourKeys[i] = "Hour" + i;
            return hourKeys;
        }

        public PreComClient(HttpClient httpClient, string userAgentSuffix)
        {
            UserAgent += userAgentSuffix;

            this.httpClient = httpClient;
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public async Task<LoginResponse> Login(string username, string password)
        {
            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            };

            var content = new FormUrlEncodedContent(nvc);
            var response = await httpClient.PostAsync(UrlBase + "Token", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var reader = new JsonTextReader(sr))
            {
                var output = Serializer.Deserialize<LoginResponse>(reader);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {output.access_token}");
                return output;
            }
        }

        public Task<User> GetUserInfo()
        {
            return Get<User>(UrlBase + "api/User/GetUserInfo");
        }

        public Task<SchedulerAppointment[]> GetUserSchedulerAppointments(DateTime from, DateTime to)
        {
            return Get<SchedulerAppointment[]>($"api/User/GetUserSchedulerAppointments?from={from:s}&to={to:s}");
        }

        public Task<Group[]> GetAllUserGroups()
        {
            return Get<Group[]>($"api/Group/GetAllUserGroups");
        }

        public Task<Dictionary<DateTime, int>> GetOccupancyLevels(long groupID, DateTime from, DateTime to)
        {
            return Get<Dictionary<DateTime, int>>($"api/Group/GetOccupancyLevels?groupID={groupID}&from={from:s}&to={to:s}");
        }

        public Task<Group> GetAllFunctions(long groupID, DateTime date)
        {
            return Get<Group>($"api/Group/GetAllFunctions?groupID={groupID}&date={date:s}");
        }

        public Task<Receiver> GetReceivers()
        {
            return Get<Receiver>($"api/Msg/GetReceivers");
        }

        public Task<Template> GetTemplates()
        {
            return Get<Template>($"api/Msg/GetTemplates");
        }

        public Task<SendMessageResponse> SendMessage(SendMessageRequest msg)
        {
            return Post<SendMessageResponse, SendMessageRequest>($"api/Msg/SendMessage", msg);
        }

        async Task<T> Get<T>(string url)
        {
            using (var stream = await httpClient.GetStreamAsync(UrlBase + url).ConfigureAwait(false))
            {
                return Deserialize<T>(stream);
            }
        }

        async Task<T> Post<T,K>(string url, K value)
        {
            using(var s = new MemoryStream())
            using (var sw = new StreamWriter(s, System.Text.Encoding.UTF8, 1024, true))
            using (var tw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                Serializer.Serialize(tw, value);
                tw.Flush();
                s.Seek(0, SeekOrigin.Begin);
                using (var content = new StreamContent(s)) 
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await httpClient.PostAsync(UrlBase + url, content).ConfigureAwait(false);
                    return Deserialize<T>(await response.Content.ReadAsStreamAsync());
                }
            }
        }

        static T Deserialize<T>(Stream data)
        {
            using (var sr = new StreamReader(data))
            using (var reader = new JsonTextReader(sr))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }
    }
}

public class SendMessageRequest
{
    public int SendBy { get; set; } = 1;
    public bool Priority {get;set;} = true;
    public bool Response { get; set; } = true;
    public int CalculateGroupID { get; set; }
    public DateTime ValidFrom { get; set; }//"2019-04-13T10:33:15.1801756+02:00"
    public string Message { get; set; }
    public List<Receiver> Receivers { get; set; }
}

public class SendMessageResponse
{

}
