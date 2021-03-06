﻿/* https://github.com/matthewburnett */

using Newtonsoft.Json;
using ShareX.UploadersLib.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX.UploadersLib.URLShorteners
{
    public class FirebaseDynamicLinksURLShortenerService : URLShortenerService
    {
        public override UrlShortenerType EnumValue { get; } = UrlShortenerType.FirebaseDynamicLinks;

        public override Icon ServiceIcon => Resources.Firebase;

        public override bool CheckConfig(UploadersConfig config)
        {
            return !string.IsNullOrEmpty(config.FirebaseWebAPIKey) && !string.IsNullOrEmpty(config.FirebaseDynamicLinkDomain);
        }

        public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            return new FirebaseDynamicLinksURLShortener
            {
                WebAPIKey = config.FirebaseWebAPIKey,
                DynamicLinkDomain = config.FirebaseDynamicLinkDomain,
                IsShort = config.FirebaseIsShort
            };
        }

        public override TabPage GetUploadersConfigTabPage(UploadersConfigForm form) => form.tpFirebaseDynamicLinks;
    }

    public class FirebaseRequest
    {
        public DynamicLinkInfo dynamicLinkInfo { get; set; }
        public Suffix suffix { get; set; }
    }

    public class DynamicLinkInfo
    {
        public string dynamicLinkDomain { get; set; }
        public string link { get; set; }
    }

    public class Suffix
    {
        public string option { get; set; }
    }

    public class FirebaseResponse
    {
        public string shortLink { get; set; }
        public string previewLink { get; set; }
    }

    public sealed class FirebaseDynamicLinksURLShortener : URLShortener
    {
        public string WebAPIKey { get; set; }
        public string DynamicLinkDomain { get; set; }
        public bool IsShort { get; set; }

        public override UploadResult ShortenURL(string url)
        {
            UploadResult result = new UploadResult { URL = url };

            FirebaseRequest request = new FirebaseRequest
            {
                dynamicLinkInfo = new DynamicLinkInfo
                {
                    dynamicLinkDomain = DynamicLinkDomain + ".app.goo.gl",
                    link = url
                }
            };

            string requesturl = "https://firebasedynamiclinks.googleapis.com/v1/shortLinks";

            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "key", WebAPIKey }
            };

            if (IsShort)
            {
                request.suffix = new Suffix
                {
                    option = "SHORT"
                };
            }

            string requestjson = JsonConvert.SerializeObject(request);

            result.Response = SendRequest(HttpMethod.POST, requesturl, requestjson, ContentTypeJSON, args);
            result.ShortenedURL = JsonConvert.DeserializeObject<FirebaseResponse>(result.Response).shortLink;

            return result;
        }
    }
}