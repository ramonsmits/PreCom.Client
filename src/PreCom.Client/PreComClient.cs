using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PreCom
{
    public class PreComClient
    {
        const string UrlBase = "https://pre-com.nl/Mobile/";
        static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        readonly string UserAgent = "PreComClient/1.2 (https://github.com/ramonsmits/PreCom.Client) ";
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

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(stream);
            using var reader = new JsonTextReader(sr);

            var output = Serializer.Deserialize<LoginResponse>(reader);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {output.access_token}");
            return output;
        }

        public Task<User> GetUserInfo(CancellationToken cancellationToken = default)
        {
            return Get<User>(UrlBase + "api/User/GetUserInfo", cancellationToken);
        }

        public Task<SchedulerAppointment[]> GetUserSchedulerAppointments(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            OnlyDate(from, nameof(from));
            OnlyDate(to, nameof(to));
            return Get<SchedulerAppointment[]>($"api/User/GetUserSchedulerAppointments?from={from:s}&to={to:s}", cancellationToken);
        }

        public Task<Group[]> GetAllUserGroups(CancellationToken cancellationToken = default)
        {
            return Get<Group[]>($"api/Group/GetAllUserGroups", cancellationToken);
        }

        public Task<Dictionary<DateTime, int>> GetOccupancyLevels(long groupID, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            OnlyDate(from, nameof(from));
            OnlyDate(to, nameof(to));
            return Get<Dictionary<DateTime, int>>($"api/Group/GetOccupancyLevels?groupID={groupID}&from={from:s}&to={to:s}", cancellationToken);
        }

        public Task<Group> GetAllFunctions(long groupID, DateTime date, CancellationToken cancellationToken = default)
        {
            OnlyDate(date, nameof(date));
            return Get<Group>($"api/Group/GetAllFunctions?groupID={groupID}&date={date:s}", cancellationToken);
        }

        public Task<MsgOut[]> GetMessages(string controlID = default, CancellationToken cancellationToken = default)
        {
            var url = "api/User/GetMessages";
            if (controlID != default) url += $"?controlID={controlID}";
            return Get<MsgOut[]>(url, cancellationToken);
        }

        public Task<MsgInLog[]> GetAlarmMessages(int msgInID = default, int previousOrNext = default, CancellationToken cancellationToken = default)
        {
            return Get<MsgInLog[]>($"api/User/GetAlarmMessages?msgInID={msgInID}&previousOrNext={previousOrNext}", cancellationToken);
        }

        public async Task SetAvailabilityForAlarmMessage(int msgInID, bool available, CancellationToken cancellationToken = default)
        {
            _ = await Post<object>($"api/User/SetAvailabilityForAlarmMessage?msgInID={msgInID}&available={available}", cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<T> Get<T>(string url, CancellationToken cancellationToken)
        {
            using var response = await httpClient.GetAsync(UrlBase + url, cancellationToken).ConfigureAwait(false);
            return await ProcessResponse<T>(url, response).ConfigureAwait(false);
        }

        protected virtual async Task<T> Post<T>(string url, HttpContent content = default, CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.PostAsync(UrlBase + url, content, cancellationToken).ConfigureAwait(false);
            return await ProcessResponse<T>(url, response).ConfigureAwait(false);
        }

        protected virtual async Task<T> ProcessResponse<T>(string url, HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            return await DeserializeResponse<T>(response);
        }

        async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(stream);
            using var reader = new JsonTextReader(sr);
            return Serializer.Deserialize<T>(reader);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static void OnlyDate(DateTime value, string paramName)
        {
            if (value != value.Date) throw new ArgumentException("Must contain only a date", paramName);
        }
    }
}
