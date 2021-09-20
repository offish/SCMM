﻿using System.Text.Json.Serialization;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SteamAuth
{
    /// <summary>
    /// Handles logging the user into the mobile Steam website. Necessary to generate OAuth token and session cookies.
    /// </summary>
    public class UserLogin
    {
        public string Username;
        public string Password;
        public ulong SteamID;

        public bool RequiresCaptcha;
        public string CaptchaGID = null;
        public string CaptchaText = null;

        public bool RequiresEmail;
        public string EmailDomain = null;
        public string EmailCode = null;

        public bool Requires2FA;
        public string TwoFactorCode = null;

        public SessionData Session = null;
        public bool LoggedIn = false;

        private readonly CookieContainer _cookies = new CookieContainer();

        public UserLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public LoginResult DoLogin()
        {
            var postData = new NameValueCollection();
            var cookies = _cookies;
            string response = null;

            if (cookies.Count == 0)
            {
                //Generate a SessionID
                cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
                cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));
                cookies.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));

                var headers = new NameValueCollection
                {
                    { "X-Requested-With", "com.valvesoftware.android.steam.community" }
                };

                SteamWeb.MobileLoginRequest("https://steamcommunity.com/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client", "GET", null, cookies, headers);
            }

            postData.Add("donotcache", (TimeAligner.GetSteamTime() * 1000).ToString());
            postData.Add("username", Username);
            response = SteamWeb.MobileLoginRequest(APIEndpoints.COMMUNITY_BASE + "/login/getrsakey", "POST", postData, cookies);
            if (response == null || response.Contains("<BODY>\nAn error occurred while processing your request."))
            {
                return LoginResult.GeneralFailure;
            }

            var rsaResponse = JsonSerializer.Deserialize<RSAResponse>(response);

            if (!rsaResponse.Success)
            {
                return LoginResult.BadRSA;
            }

            Thread.Sleep(350); //Sleep for a bit to give Steam a chance to catch up??

            var secureRandom = new RNGCryptoServiceProvider();
            byte[] encryptedPasswordBytes;
            using (var rsaEncryptor = new RSACryptoServiceProvider())
            {
                var passwordBytes = Encoding.ASCII.GetBytes(Password);
                var rsaParameters = rsaEncryptor.ExportParameters(false);
                rsaParameters.Exponent = Util.HexStringToByteArray(rsaResponse.Exponent);
                rsaParameters.Modulus = Util.HexStringToByteArray(rsaResponse.Modulus);
                rsaEncryptor.ImportParameters(rsaParameters);
                encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);
            }

            var encryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

            postData.Clear();
            postData.Add("donotcache", (TimeAligner.GetSteamTime() * 1000).ToString());

            postData.Add("password", encryptedPassword);
            postData.Add("username", Username);
            postData.Add("twofactorcode", TwoFactorCode ?? "");

            postData.Add("emailauth", RequiresEmail ? EmailCode : "");
            postData.Add("loginfriendlyname", "");
            postData.Add("captchagid", RequiresCaptcha ? CaptchaGID : "-1");
            postData.Add("captcha_text", RequiresCaptcha ? CaptchaText : "");
            postData.Add("emailsteamid", (Requires2FA || RequiresEmail) ? SteamID.ToString() : "");

            postData.Add("rsatimestamp", rsaResponse.Timestamp);
            postData.Add("remember_login", "true");
            postData.Add("oauth_client_id", "DE45CD61");
            postData.Add("oauth_scope", "read_profile write_profile read_client write_client");

            response = SteamWeb.MobileLoginRequest(APIEndpoints.COMMUNITY_BASE + "/login/dologin", "POST", postData, cookies);
            if (response == null)
            {
                return LoginResult.GeneralFailure;
            }

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(response);

            if (loginResponse.Message != null)
            {
                if (loginResponse.Message.Contains("There have been too many login failures"))
                {
                    return LoginResult.TooManyFailedLogins;
                }

                if (loginResponse.Message.Contains("Incorrect login"))
                {
                    return LoginResult.BadCredentials;
                }
            }

            if (loginResponse.CaptchaNeeded)
            {
                RequiresCaptcha = true;
                CaptchaGID = loginResponse.CaptchaGID;
                return LoginResult.NeedCaptcha;
            }

            if (loginResponse.EmailAuthNeeded)
            {
                RequiresEmail = true;
                SteamID = loginResponse.EmailSteamID;
                return LoginResult.NeedEmail;
            }

            if (loginResponse.TwoFactorNeeded && !loginResponse.Success)
            {
                Requires2FA = true;
                return LoginResult.Need2FA;
            }

            if (loginResponse.OAuthData == null || loginResponse.OAuthData.OAuthToken == null || loginResponse.OAuthData.OAuthToken.Length == 0)
            {
                return LoginResult.GeneralFailure;
            }

            if (!loginResponse.LoginComplete)
            {
                return LoginResult.BadCredentials;
            }
            else
            {
                var readableCookies = cookies.GetCookies(new Uri("https://steamcommunity.com"));
                var oAuthData = loginResponse.OAuthData;

                var session = new SessionData
                {
                    OAuthToken = oAuthData.OAuthToken,
                    SteamID = oAuthData.SteamID
                };
                session.SteamLogin = session.SteamID + "%7C%7C" + oAuthData.SteamLogin;
                session.SteamLoginSecure = session.SteamID + "%7C%7C" + oAuthData.SteamLoginSecure;
                session.WebCookie = oAuthData.Webcookie;
                try
                {
                    session.SessionID = readableCookies["sessionid"].Value;
                }
                catch (Exception)
                {

                }
                Session = session;
                LoggedIn = true;
                return LoginResult.LoginOkay;
            }
        }

        private class LoginResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("login_complete")]
            public bool LoginComplete { get; set; }

            [JsonPropertyName("oauth")]
            public string OAuthDataString { get; set; }

            public OAuth OAuthData => OAuthDataString != null ? JsonSerializer.Deserialize<OAuth>(OAuthDataString) : null;

            [JsonPropertyName("captcha_needed")]
            public bool CaptchaNeeded { get; set; }

            [JsonPropertyName("captcha_gid")]
            public string CaptchaGID { get; set; }

            [JsonPropertyName("emailsteamid")]
            public ulong EmailSteamID { get; set; }

            [JsonPropertyName("emailauth_needed")]
            public bool EmailAuthNeeded { get; set; }

            [JsonPropertyName("requires_twofactor")]
            public bool TwoFactorNeeded { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            internal class OAuth
            {
                [JsonPropertyName("steamid")]
                public ulong SteamID { get; set; }

                [JsonPropertyName("oauth_token")]
                public string OAuthToken { get; set; }

                [JsonPropertyName("wgtoken")]
                public string SteamLogin { get; set; }

                [JsonPropertyName("wgtoken_secure")]
                public string SteamLoginSecure { get; set; }

                [JsonPropertyName("webcookie")]
                public string Webcookie { get; set; }
            }
        }

        private class RSAResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("publickey_exp")]
            public string Exponent { get; set; }

            [JsonPropertyName("publickey_mod")]
            public string Modulus { get; set; }

            [JsonPropertyName("timestamp")]
            public string Timestamp { get; set; }

            [JsonPropertyName("steamid")]
            public ulong SteamID { get; set; }
        }
    }

    public enum LoginResult
    {
        LoginOkay,
        GeneralFailure,
        BadRSA,
        BadCredentials,
        NeedCaptcha,
        Need2FA,
        NeedEmail,
        TooManyFailedLogins,
    }
}
