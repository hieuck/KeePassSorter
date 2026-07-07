using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KeePassSorter
{
    public sealed class UpdateInfo
    {
        public string LatestVersion;
        public string ReleaseUrl;
        public string AssetUrl;
        public bool IsUpdateAvailable;
    }

    public static class UpdateChecker
    {
        public const string ReleasesApiUrl = "https://api.github.com/repos/hieuck/KeePassSorter/releases";
        public const string ReleasesUrl = "https://github.com/hieuck/KeePassSorter/releases";

        public static string GetCurrentVersion()
        {
            Assembly assembly = typeof(UpdateChecker).Assembly;
            object[] attrs = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (attrs.Length > 0)
            {
                AssemblyInformationalVersionAttribute attr = (AssemblyInformationalVersionAttribute)attrs[0];
                if (!string.IsNullOrEmpty(attr.InformationalVersion))
                    return attr.InformationalVersion;
            }

            Version version = assembly.GetName().Version;
            return (version != null) ? version.ToString(3) : "0.0.0";
        }

        public static bool IsNewerVersion(string currentVersion, string candidateVersion)
        {
            Version current;
            Version candidate;
            if (!TryParseVersion(currentVersion, out current)) return false;
            if (!TryParseVersion(candidateVersion, out candidate)) return false;
            return CompareNormalized(candidate, current) > 0;
        }

        private static int CompareNormalized(Version a, Version b)
        {
            int[] componentsA = new[] { a.Major, a.Minor, a.Build, a.Revision };
            int[] componentsB = new[] { b.Major, b.Minor, b.Build, b.Revision };
            for (int i = 0; i < 4; i++)
            {
                int ca = componentsA[i] < 0 ? 0 : componentsA[i];
                int cb = componentsB[i] < 0 ? 0 : componentsB[i];
                int cmp = ca.CompareTo(cb);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        public static UpdateInfo CheckLatest()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.UserAgent] = "KeePassSorter";
                client.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                string json = client.DownloadString(ReleasesApiUrl);
                string tagName = GetNewestVersionTag(ExtractJsonStrings(json, "tag_name").ToArray());

                UpdateInfo info = new UpdateInfo();
                info.LatestVersion = tagName;
                info.ReleaseUrl = BuildReleaseUrl(tagName);
                info.AssetUrl = BuildPlgxAssetUrl(tagName);
                info.IsUpdateAvailable = IsNewerVersion(GetCurrentVersion(), tagName);
                return info;
            }
        }

        public static string GetNewestVersionTag(string[] tags)
        {
            string newestTag = string.Empty;
            Version newestVersion = null;

            foreach (string tag in tags)
            {
                Version version;
                if (!TryParseVersion(tag, out version)) continue;

                if (newestVersion == null || CompareNormalized(version, newestVersion) > 0)
                {
                    newestVersion = version;
                    newestTag = tag;
                }
            }

            return newestTag;
        }

        private static bool TryParseVersion(string value, out Version version)
        {
            version = null;
            if (string.IsNullOrEmpty(value)) return false;

            string normalized = value.Trim();
            if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(1);

            return Version.TryParse(normalized, out version);
        }

        private static string BuildPlgxAssetUrl(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return ReleasesUrl;
            return "https://github.com/hieuck/KeePassSorter/releases/download/" + tagName + "/KeePassSorter.plgx";
        }

        private static string BuildReleaseUrl(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return ReleasesUrl;
            return ReleasesUrl + "/tag/" + tagName;
        }

        private static List<string> ExtractJsonStrings(string json, string name)
        {
            List<string> values = new List<string>();
            if (string.IsNullOrEmpty(json)) return values;

            MatchCollection matches = Regex.Matches(json, "\"" + Regex.Escape(name) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"");
            foreach (Match match in matches)
            {
                if (match.Success)
                    values.Add(Regex.Unescape(match.Groups["value"].Value));
            }

            return values;
        }
    }
}
