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
        const string UserAgent = "PreComClient/1.0";
        const string UrlBase = "https://pre-com.nl/Mobile/";
        readonly JsonSerializer serializer = JsonSerializer.CreateDefault();
        readonly HttpClient httpClient;

        public static readonly string[] HourKeys = GenerateHourKeys();

        static string[] GenerateHourKeys()
        {
            var hourKeys = new string[24];
            for (var i = 0; i < 24; i++) hourKeys[i] = "Hour" + i;
            return hourKeys;
        }


        public PreComClient(HttpClient httpClient)
        {
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
                var output = serializer.Deserialize<LoginResponse>(reader);
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

        async Task<T> Get<T>(string url)
        {
            using (var stream = await httpClient.GetStreamAsync(UrlBase + url).ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
