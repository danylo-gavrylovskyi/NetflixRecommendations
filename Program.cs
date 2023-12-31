﻿using CsvHelper;
using System.Globalization;
using Features;
class Program
{
    static void Main()
    {
        // getting data
        var config = new CsvHelper.Configuration.CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null
        };
        const string pathToUsersFile = "C:\\Meine\\C#\\Netflix\\users_export.csv";
        const string pathToMovieDataFile = "C:\\Meine\\C#\\Netflix\\movie_data.csv";
        const string pathToRatingsFile = "C:\\Meine\\C#\\Netflix\\ratings_export.csv";
        var users = GetUsers(pathToUsersFile);
        var ratings = GetRatings(pathToRatingsFile);
        var movieData = GetMovieData(pathToMovieDataFile, config).Where(x => x.genres!.Contains("Drama") ||
                                                                             x.genres.Contains("Action") ||
                                                                             x.genres.Contains("Adventure") ||
                                                                             x.genres.Contains("Fiction") ||
                                                                             x.genres.Contains("Romance") ||
                                                                             x.genres.Contains("Comedy") ||
                                                                             x.genres.Contains("Science Fiction") ||
                                                                             x.genres.Contains("Fantasy") ||
                                                                             x.genres.Contains("Animation") ||
                                                                             x.genres.Contains("Thriller") ||
                                                                             x.genres.Contains("Documentary")
                                                                             );
        // User commands
        int ratedFilmsCount = 0;
        User me = new User() { username="me" };
        users.Add(me);
        while (true)
        {
            Console.WriteLine("Choose command to use:\n\t1. rate <Film Name> <Your Rate from 0-10>\n\t2. discovery\n\t3. recommend (you need to rate at least 5 films)\n\t4. exit");
            string answer = Console.ReadLine()!;
            if (answer.Contains("rate") && ratedFilmsCount <= 10 || answer == "1") {
                string filmName = answer.Substring(5, answer.Length - 7);
                string rating = answer.Substring(answer.Length - 2);
                Movie movie;
                // If this film exists
                if (movieData.Any(film => film.movie_title == filmName))
                {
                    movie = movieData.FirstOrDefault(film => film.movie_title == filmName)!;
                    ratings.Add(new UserRate() { user_id="me", movie_id = movie.movie_id, rating_val = rating });
                    ratedFilmsCount++;
                    Console.WriteLine($"You've rated a film '{filmName}' ({movie.movie_id}) as {rating}");
                    continue;
                } 
                // Maybe user mean this film
                Levenshtein.GetSimilarFilmNames(filmName, movieData);
            }
            else if (answer == "recommend" || answer == "3")
            {
                if (ratedFilmsCount < 5)
                {
                    Console.WriteLine("We need to know what do you like, rate at least 5 films in order to receive recommendations");
                    continue;
                }

                // Set users positions in 8-dimensional space of preference
                for (int i = 0; i < users.Count; i++)
                {
                    Dictionary<string, int> genresRatingCount = GetUserGenresRatingsCount(ratings, users[i]);
                    users[i].positionIn8Dimension = ConvertUserGenresRatingCountTo8Dimension(genresRatingCount);
                }
                Movie recommendedFilm = Recommendations.GetRecommendedFilm(me, users, ratings, movieData);
                Console.WriteLine($"Try to watch {recommendedFilm.movie_title}, i think you will like it");
            }
            else if (answer == "discovery" || answer == "2")
            {
                List<string> necessaryGenres = new List<string>() {"Drama", "Comedy", "Action", "Romance", "Fiction", "Animation", "Thriller", "Documentary" };
                for (int i = 0; i < necessaryGenres.Count; i++)
                {
                    Movie filmToDiscover = Discovery.GetFilmForDiscovery(ratings, movieData, necessaryGenres[i], me);
                    Console.WriteLine($"Have you seen '{filmToDiscover.movie_title}'? Y/N");
                    string answerToYesNo = Console.ReadLine()!;
                    if (answerToYesNo == "Yes" || answerToYesNo == "Y" || answerToYesNo == "yes") 
                    {
                        Console.WriteLine("How would you rate it ?");
                        string rate = Console.ReadLine()!;
                        ratedFilmsCount++;
                        ratings.Add(new UserRate() { movie_id = filmToDiscover.movie_id, user_id = me.username, rating_val = rate });
                    }
                }
            }
            else if (answer == "exit" || answer == "4") break;
        }



        Dictionary<string, int> GetUserGenresRatingsCount(List<UserRate> ratings, User user)
        {
            var result = GetEmptyDictWithGenres();

            for (int i = 0; i < ratings.Count; i++)
            {
                if (ratings[i].user_id == user.username)
                {
                    var film = movieData!.FirstOrDefault(x => x.movie_id == ratings[i].movie_id);
                    if (film == null || film.genres == null) continue;
                    string[] genres = film.genres.Split(new Char[] { '[', ']', '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < genres.Length; j++)
                    {
                        if (result.ContainsKey(genres[j])) result[genres[j]]++;
                        else if (genres.Contains("Adventure")) result["Action"]++;
                        else if (genres.Contains("Science Fiction") || genres.Contains("Fantasy")) result["Fiction"]++;
                    }
                }
            }
            return result;
        }

        Dictionary<string, double> ConvertUserGenresRatingCountTo8Dimension(Dictionary<string, int> dict)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (var keyValue in dict)
            {
                result[keyValue.Key] = keyValue.Value > 1000 ? 1 : (float)keyValue.Value / 1000;
            }
            return result;
        }

        Dictionary<string, int> GetEmptyDictWithGenres()
        {
            return new Dictionary<string, int>()
        {
                { "Drama", 0 },
                { "Action", 0 },
                { "Fiction", 0 },
                { "Romance", 0 },
                { "Comedy", 0 },
                { "Animation", 0 },
                { "Thriller", 0 },
                { "Documentary", 0 }
            };
        }
    }
    private static List<Movie> GetMovieData(string pathToFile, CsvHelper.Configuration.CsvConfiguration config)
    {
        using (var reader = new StreamReader(pathToFile))
        using (var csv = new CsvReader(reader, config))
        {
            var records = csv.GetRecords<Movie>();
            return new List<Movie>(records);
        }
    }
    private static List<UserRate> GetRatings(string pathToFile)
    {
        using (var reader = new StreamReader(pathToFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<UserRate>();
            return new List<UserRate>(records);
        }
    }
    private static List<User> GetUsers(string pathToFile)
    {
        using (var reader = new StreamReader(pathToFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<User>();
            return new List<User>(records);
        }
    }
}
