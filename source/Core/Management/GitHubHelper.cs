using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace Core.Management
{

    public class GitHubHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private string LatestReleaseUrl {  get; set; }
        private string Token { get; set; }
        private HttpResponseMessage Response { get; set; }

        public async Task<string> GetLatestReleaseAsync(string owner, string repo, string token = null)
        {
            // Basis-URL für den API-Aufruf
            LatestReleaseUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            Token = token;

            Response = await AccessRepo();

            // Ergebnis verarbeiten
            var responseBody = await Response.Content.ReadAsStringAsync();
            var release = JObject.Parse(responseBody);

            // Titel und URL des neuesten Releases zurückgeben
            var tagName = release["tag_name"]?.ToString();
            var releaseUrl = release["html_url"]?.ToString();
            var releaseName = release["name"]?.ToString();

            return releaseName;
        }

        private async Task<HttpResponseMessage> AccessRepo()
        {
            // Optional: Token hinzufügen, falls Authentifizierung erforderlich ist
            if (!string.IsNullOrEmpty(Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }

            // Wichtige Header setzen
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

            // API-Aufruf
            HttpResponseMessage response = await client.GetAsync(LatestReleaseUrl);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async void DownloadLatesRelease(bool isMultiUser)
        {
            JObject releaseInfo = JObject.Parse( await Response.Content.ReadAsStringAsync());
            JToken assets = releaseInfo["assets"];
            int assetsType;

            if (isMultiUser)
            {
                assetsType = 0;
            }
            else
            {
                assetsType= 1;
            }

            string downloadURL = assets[assetsType]?["browser_download_url"]?.ToString();
            string fileName = downloadURL.Split('/').Last();
            
            if (downloadURL != null)
            {
                string downloadFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
                using HttpResponseMessage downloadResponse = await client.GetAsync(downloadURL);
                downloadResponse.EnsureSuccessStatusCode();
                using FileStream fileStream = new(downloadFilePath, FileMode.Create);
                await downloadResponse.Content.CopyToAsync(fileStream);
                /*
                try
                {
                    CloseProcess("Revit");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadFilePath,
                        UseShellExecute = false,
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Starten der Datei: {ex.Message}");
                }
                */
            }
            else
            {
                throw new Exception("Download-URL konnte nicht ermittelt werden.");
            }



        }

        private void CloseProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                try
                {
                    process.CloseMainWindow();
                    process.WaitForExit(5000);
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex) { Console.WriteLine($"Fehler beim Schließen von {processName}: {ex.Message}"); }
            }
        }
    }
}
