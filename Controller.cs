using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MeanWords.Models;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using System.Data;
using System.Text.RegularExpressions;

namespace MeanWords
{
    class Controller
    {
        DataTable songs = new DataTable();

        public async Task Run()
        {
            songs.Columns.Add("Song");
            songs.Columns.Add("Lyrics");
            songs.Columns.Add("Words", typeof(System.Int32));

            while (true)
            {
                Console.WriteLine("Please enter the name of the artist you would like to find average song length for:");
                var input = Console.ReadLine();

                Console.WriteLine("Finding artist...");

                var artist = FindArtistByName(input);

                Console.WriteLine($"Artist found. Calculating average song length for artist {artist.Name}");
                Console.WriteLine("Please wait...");

                var average = await GetArtistAverageSongLength(artist);

                if (average <= 0)
                {
                    Console.WriteLine("Unfortunately, we couldn't find the lyrics for any songs by that artist, or all of that artist's works are instrumental.");
                    Console.WriteLine("Would you like to perform another lookup? Y/N");
                    char response = ' ';
                    while (response != 'y' && response != 'n')
                    {
                        response = Console.ReadKey().KeyChar;
                    }

                    if (response == 'n')
                    {
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"{songs.Rows.Count} songs found for {artist.Name}. We found lyrics for {songs.Select("Words > 1").Length} of them at an average of {average} per song.");
                    DisplayOptions();
                    while (true)
                    {
                        char response = ' ';
                        while (response != 'n' && response != 'a' && response != 'l' && response != 'w' && response != 'x')
                        {
                            response = Console.ReadKey().KeyChar;
                        }
                        switch (response)
                        {
                            case 'a':
                                ListAllSongs();
                                break;

                            case 'l':
                                ListLyricSongs();
                                break;

                            case 'w':
                                ListFailSongs();
                                break;

                            case 'x':
                                return;

                            case 'n':
                                Console.WriteLine("");
                                break;
                            default:
                                break;
                        }
                        if (response == 'n')
                        {
                            break;
                        }
                        DisplayOptions();
                    }
                }
            }
        }

        public void ListAllSongs()
        {
            Console.WriteLine("\nListing All Songs...");
            foreach (DataRow dr in songs.Rows)
            {
                Console.WriteLine(dr["Song"]);
            }
        }

        public void ListLyricSongs()
        {
            Console.WriteLine("\nListing Songs With Lyrics...");
            DataRow[] result = songs.Select("Words > 1");
            
            foreach (DataRow dr in result)
            {
                Console.WriteLine(dr["Song"]);
            }
        }

        public void ListFailSongs()
        {
            Console.WriteLine("\nListing Songs Without Lyrics...");
            DataRow[] result = songs.Select("Words <= 1");

            foreach (DataRow dr in result)
            {
                Console.WriteLine(dr["Song"]);
            }
        }
        static void DisplayOptions()
        {
            Console.WriteLine("Press 'n' for new lookup, 'a' to list all songs, 'l' to list songs with lyrics, 'w' to list songs without lyrics or 'x' to exit");
        }
        static IArtist FindArtistByName(string artist)
        {
            // Return the first occurence of the matched artist
            var q = new Query("Mean Words", "1.0", "damon@leedham.com");

            return (q.FindArtists($"name:{artist}")).Results[0].Item;
        }
        public async Task<int> GetArtistAverageSongLength(IArtist artist)
        {
            var q = new Query("Mean Words", "1.0", "damon@leedham.com");

            songs.Clear();
            songs.TableName = artist.Name;            

            // Populate the songs datatable with all song names plus lyrics and the count where possible
            var works = q.BrowseArtistWorks(artist.Id);
            while (works.Offset + works.Results.Count < works.TotalResults)
            {
                for (int i = 0; i < works.Results.Count; i++)
                {
                    var lm = GetSongLength(artist.Name, works.Results[i].Title);
                    if (lm == null)
                    {
                        songs.Rows.Add(works.Results[i].Title, "", 0);
                    }
                    else
                    {
                        songs.Rows.Add(works.Results[i].Title, lm.lyrics, lm.count);
                    }
                }
                works = await works.NextAsync();
            }
            for (int i = 0; i < works.Results.Count; i++)
            {
                var lm = GetSongLength(artist.Name, works.Results[i].Title);
                if (lm == null)
                {
                    songs.Rows.Add(works.Results[i].Title, "", 0);
                }
                else
                {
                    songs.Rows.Add(works.Results[i].Title, lm.lyrics, lm.count);
                }
            }

            int average = Convert.ToInt32(songs.Compute("AVG(Words)", "Words > 1"));
            return average;
        }
        static LyricsModel GetSongLength(string artist, string songTitle)
        {
            // Populate the lyrics model with the lyrics and a sanitised count
            var client = new LyricsClient();
            var trackResponse = client.GetTrack(artist, songTitle);
            if (trackResponse != null)
            {
                var lyricsResponse = client.GetLyrics(trackResponse.id_artist, trackResponse.id_album, trackResponse.id_track);
                string lyrics = SanitiseLyrics(lyricsResponse.lyrics);
                var wordCount = lyrics.Split().Length;
                lyricsResponse.count = wordCount;
                return lyricsResponse;
            }
            return null;
        }

        static string SanitiseLyrics(string lyrics)
        {
            // Remove various characters and double spaces to help get a more acurate word count
            Regex pattern = new Regex("[?.,\r\n]|[  ]");
            string newlyrics = lyrics;
            newlyrics = pattern.Replace(newlyrics, " ");
            while (newlyrics.Contains("  "))
            {
                newlyrics = newlyrics.Replace("  ", " ");
            }
            return newlyrics;
        }
    }
}
