using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace AntiEpos
{
    /// <summary>
    /// Класс клиента для работы с РСААГ и ЭПОС.
    /// (Региональная Система Авторизации и Аутентификации Гражданина)
    /// (Электронная Пермская Образовательная система)
    /// </summary>
    public class AEClient : IDisposable
    {
        /// <summary>
        /// Публичный сервер авторизации РСААГ
        /// </summary>
        private readonly string uri_rsaag = "https://cabinet.permkrai.ru/";

        /// <summary>
        /// Публичный сервер ЭПОС
        /// </summary>
        private readonly string uri_epos = "https://school.permkrai.ru/";

        private CookieContainer cookies;

        private AEHttpHandler hch;

        private HttpClient hc;

        public AEClient()
        {
            cookies = new CookieContainer();
            hch = new AEHttpHandler(new HttpClientHandler()
            {
                CookieContainer = cookies,
                UseCookies = true,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            { DoNotPrintInDebug = true };
            hc = new HttpClient(hch, true);
        }

        private AEUserInfo? user = null;
        private bool disposedValue;

        public AEUserInfo? UserInfo => user;

        private class AEClientHeader
        {
            public string? strvalue;
            public ICollection<string>? strvalues;

            public AEClientHeader(string onlyonevalue)
            {
                strvalue = onlyonevalue;
            }

            public AEClientHeader(IEnumerable<string> morevalues)
            {
                strvalues = new List<string>();
                foreach (var i in morevalues)
                {
                    strvalues.Add(i);
                }
            }
        }

        private readonly IDictionary<string, AEClientHeader> myheaders = new Dictionary<string, AEClientHeader>();

        private void AddHeader(string name, string value)
        {
            myheaders[name] = new AEClientHeader(value);
        }

        private void AddHeader(string name, IEnumerable<string> values)
        {
            myheaders[name] = new AEClientHeader(values);
        }

        private string CookieByName(string uri, string name)
        {
            var ccoll = cookies.GetCookies(new Uri(uri));
            for (int i = 0; i < ccoll.Count; ++i)
            {
                if (ccoll[i].Name == name)
                {
                    return ccoll[i].Value;
                }
            }

            return "";
        }

        private HttpRequestMessage StampHeaders(HttpRequestMessage msg)
        {
            msg.Version = new Version(2, 0);
            msg.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
            //msg.Headers.Connection.Clear();
            //msg.Headers.Connection.Add("keep-alive");
            //msg.Headers.Add("Sec-Fetch-Mode", "cors");
            //msg.Headers.Add("Sec-Fetch-Site", "same-origin");
            msg.Headers.Add("Sec-CH-UA-Platform", "Windows");
            msg.Headers.Add("Sec-CH-UA-Mobile", "?0");
            msg.Headers.Add("Sec-CH-UA", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"98\", \"Google Chrome\";v=\"98\"");
            msg.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
            msg.Headers.Accept.ParseAdd("application/json, text/plain, */*");
            msg.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            msg.Headers.AcceptEncoding.ParseAdd("gzip, deflate");
            //msg.Headers.Referrer = new Uri(uri_rsaag);
            msg.Headers.Pragma.ParseAdd("no-cache");
            //msg.Headers.Add("Origin", uri_rsaag.TrimEnd('/'));
            //msg.Headers.Add("Cookie", cookies.GetCookieHeader(msg.RequestUri));

            foreach (var kvp in myheaders)
            {
                if (kvp.Value.strvalues is null)
                {
                    msg.Headers.Add(kvp.Key, kvp.Value.strvalue);
                }
                else
                {
                    msg.Headers.Add(kvp.Key, kvp.Value.strvalues);
                }
            }

            return msg;
        }

        /// <summary>
        /// Обновляет CSRF токены и прочую хрень которую намудрили они там
        /// </summary>
        /// <returns>хрень</returns>
        private async Task RefreshCsrf()
        {
            var tokenresp = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Get,
                uri_rsaag + "login"
            )))).EnsureSuccessStatusCode();

            var html = await tokenresp.Content.ReadAsStringAsync();
            var stpos = html.IndexOf("\"csrf-token\" content=\"") + "\"csrf-token\" content=\"".Length;
            var endpos = html.IndexOf("\" id=\"csrf\"");
            var csrftoken = html.Substring(stpos, endpos - stpos);

            AddHeader("X-CSRF-Token", csrftoken);
            AddHeader("X-XSRF-Token", CookieByName(uri_rsaag, "XSRF-TOKEN"));
            AddHeader("X-Requested-With", "XMLHttpRequest");
        }

        /// <summary>
        /// Производит вход в систему РСААГ, нужно вызывать перед входом в ЭПОС.
        /// </summary>
        /// <param name="login">почта</param>
        /// <param name="password">пароль</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Невозможный логин или пароль</exception>
        /// <exception cref="AEException">Произошла ошибка авторизации</exception>
        public async Task Login(string? login, string? password)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login is null or empty.", nameof(login));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is null or empty.", nameof(password));

            try
            {
                await RefreshCsrf();

                var resp = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Post, uri_rsaag + "login") {
                    Content = new FormUrlEncodedContent(
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("_token", myheaders["X-CSRF-Token"].strvalue ?? ""),
                            new KeyValuePair<string, string>("login", login),
                            new KeyValuePair<string, string>("password", password)
                        }
                    )
                }))).EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exc)
            {
                throw new AEException("Ошибка во время HTTP запроса.", exc);
            }
        }

        /// <summary>
        /// Авторизует вошедшего пользователя как authMode
        /// </summary>
        /// <param name="authMode">Режим аутентификации пользователя</param>
        /// <returns></returns>
        /// <exception cref="AEException">Ошибка аутентификации</exception>
        public async Task Authenticate(AEAuthMode authMode)
        {
            try
            {
                await RefreshCsrf();

                var resp = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Get,
                    uri_epos + "authenticate?mode=oauth&app=" + authMode.Mode) {
                    Content = null
                }))).EnsureSuccessStatusCode();

                // его поредиректит и успокоит.... надеюсь...
                var html = await resp.Content.ReadAsStringAsync();

                user = new AEUserInfo();
                user.ProfileId = ulong.Parse(CookieByName(uri_epos, "profile_id"));
                user.AId = ulong.Parse(CookieByName(uri_epos, "aid"));
                user.AuthToken = CookieByName(uri_epos, "auth_token");

                AddHeader("profile-id", user.ProfileId.ToString());
                AddHeader("auth-token", user.AuthToken);

                var sess = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Post,
                    uri_epos + "lms/api/sessions?pid=" + user.ProfileId.ToString()) {
                    Content = JsonContent.Create(new AETokenData(user.AuthToken))
                }))).EnsureSuccessStatusCode();

                var sessjson = await sess.Content.ReadFromJsonAsync<AESessionsData>();
                if (sessjson is null)
                    throw new AEException("Не могу получить данные сессии.");

                user.Sessions = sessjson;

                var years = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Get,
                    uri_epos + "core/api/academic_years?pid=" + user.ProfileId.ToString()) {
                    Content = null
                }))).EnsureSuccessStatusCode();

                var yearsjson = await years.Content.ReadFromJsonAsync<AEAcademicYear[]>();
                if (yearsjson is null)
                    throw new AEException("Не могу получить данные об академических годах.");

                user.AcademicYears = yearsjson;
            }
            catch (HttpRequestException exc)
            {
                throw new AEException("Ошибка во время HTTP запроса.", exc);
            }
        }

        public async Task<AEProgressData[]> FetchProgress(ulong yearId, bool hideHalfYears = true)
        {
            try
            {
                if (user is null)
                    throw new AEException("Не выполнен вход.");

                var progress = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Get,
                    uri_epos + $"reports/api/progress/json?academic_year_id={yearId}&hide_half_years={hideHalfYears.ToString().ToLowerInvariant()}&pid={user.ProfileId}&student_profile_id={user.ProfileId}") {
                    Content = null
                }))).EnsureSuccessStatusCode();

                var progressjson = await progress.Content.ReadFromJsonAsync<AEProgressData[]>();
                if (progressjson is null)
                    throw new AEException("Ошибка получения данных о прогрессе.");

                return progressjson;
            }
            catch (HttpRequestException exc)
            {
                throw new AEException("Ошибка во время HTTP запроса.", exc);
            }
        }

        public async Task<AEAgreementResponse> CheckAgreement()
        {
            try
            {
                await RefreshCsrf();

                var agresp = (await hc.SendAsync(StampHeaders(new HttpRequestMessage(HttpMethod.Post, uri_rsaag + "check_agreement")
                {
                    Content = null
                }))).EnsureSuccessStatusCode();

                var ag = await agresp.Content.ReadFromJsonAsync<AEAgreementResponse>();

                // ag cannot be null here
                if (ag is null)
                    throw new AEException("Нет данных о соглашении пользователя, логин не был успешен.");

                return ag;
            }
            catch (HttpRequestException exc)
            {
                throw new AEException("Ошибка во время HTTP запроса.", exc);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    hc.Dispose();
                    hch.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
