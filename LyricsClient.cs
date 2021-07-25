using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MeanWords.Models;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;

namespace MeanWords
{
    class LyricsClient
    {
        private static HttpClient client = new HttpClient();

        static LyricsClient()
        {
            //client.BaseAddress = new Uri("https://api.lyrics.ovh/v1/");
            // The lyrics.ovh api has been down throughout so I has to find another resource
            // I'm not sure this one has the most reliable results but it was the best I could find
            client.BaseAddress = new Uri("https://api.happi.dev/v1/music");
        }

        public LyricsModel GetLyrics(int id_artist, int id_album, int id_track)
        {            
            LyricsResponseModel lyrics = null;

            // Search for lyrics for specific song

            string apiUrl = "https://api.happi.dev/v1/music/artists/{0}/albums/{1}/tracks/{2}/lyrics?apikey={3}";
            string requestUri = string.Format(apiUrl, id_artist, id_album, id_track, Settings1.Default.apiKey.Trim());
            var response = client.GetAsync(requestUri).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                string responseString = responseContent.ReadAsStringAsync().Result;
                lyrics = JsonConvert.DeserializeObject<LyricsResponseModel>(responseString);
            }
            // Crude way to throttle the requests to the Api as it has a limit of 100 per second
            System.Threading.Thread.Sleep(500);

            return lyrics.result;
        }

        public TrackModel GetTrack(string artist, string title)
        {
            TrackResponseModel trackresponse = null;

            // Search for Artist<space>title removing forward slashes
 
            string apiUrl = "https://api.happi.dev/v1/music?q={0}&limit=1&apikey={1}&type=&lyrics=1";
            string searchTerm = Uri.EscapeUriString($"{artist.Replace("/", " ")} {title.Replace("/", " ")}");
            string requestUri = string.Format(apiUrl, searchTerm.Trim(), Settings1.Default.apiKey.Trim());

            var response = client.GetAsync(requestUri).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                string responseString = responseContent.ReadAsStringAsync().Result;
                trackresponse = JsonConvert.DeserializeObject<TrackResponseModel>(responseString);
            }

            // Crude way to throttle the requests to the Api as it has a limit of 100 per second
            // System.Threading.Thread.Sleep(1000);

            // response can still be 200 even if it fails so check that here and return null
            if (trackresponse.length == 0)
            {
                return null;
            }
            return trackresponse.result[0];
        }
    }
}
