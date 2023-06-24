using CsvHelper;
using System.Globalization;
class Movie
{
    public string? _id { get; set; }
    public string? genres { get; set; }
    public string? movie_id { get; set; }
    public string? movie_title { get; set; }
}

class UserRate
{
    public string? _id { get; set; }
    public string? user_id { get; set; }
    public string? movie_id { get; set; }
    public string? rating_val { get; set; }
}

class User
{
    public string? _id { get; set; }
    public string? display_name { get; set; }
    public string? username { get; set; }

    public Dictionary<string, double> positionIn8Dimension = new Dictionary<string, double>();
}

class Program
{
    static void Main(string[] args)
    {
        var config = new CsvHelper.Configuration.CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null
        };
        const string pathToUsersFile = "C:\\Meine\\C#\\Netflix\\users_export.csv";
        const string pathToMovieDataFile = "C:\\Meine\\C#\\Netflix\\movie_data.csv";
        const string pathToRatingsFile = "C:\\Meine\\C#\\Netflix\\ratings_export.csv";
        var users = GetUsers(pathToUsersFile);
        var ratings = GetRatings(pathToRatingsFile);
        var movieData = GetMovieData(pathToMovieDataFile, config).Where(x => x.genres.Contains("Drama") ||
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
        for (int i = 0; i < users.Count; i++)
        {
            Dictionary<string, int> genresRatingCount = GetUserGenresRatingsCount(ratings, users[i]);
            users[i].positionIn8Dimension = ConvertUserGenresRatingCountTo8Dimension(genresRatingCount);
            foreach (var item in users[i].positionIn8Dimension)
            {
                Console.WriteLine($"{item.Key} | {item.Value}");
            }
        }

        Dictionary<string, int> GetUserGenresRatingsCount(List<UserRate> ratings, User user)
        {
            var result = GetEmptyDictWithGenres();

            for (int i = 0; i < ratings.Count; i++)
            {
                if (ratings[i].user_id == user.username)
                {
                    var film = movieData.FirstOrDefault(x => x.movie_id == ratings[i].movie_id);
                    if (film == null || film.genres == null) continue;
                    string[] genres = film.genres.Split(new Char[] { '[', ']', '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < genres.Length; j++)
                    {
                        if (result.ContainsKey(genres[j])) result[genres[j]]++;
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
                { "Adventure", 0 },
                { "Fiction", 0 },
                { "Romance", 0 },
                { "Comedy", 0 },
                { "Science Fiction", 0 },
                { "Fantasy", 0 },
                { "Animation", 0 },
                { "Thriller", 0 },
                { "Documentary", 0 }
            };
        }
    }
    public static List<Movie> GetMovieData(string pathToFile, CsvHelper.Configuration.CsvConfiguration config)
    {
        using (var reader = new StreamReader(pathToFile))
        using (var csv = new CsvReader(reader, config))
        {
            var records = csv.GetRecords<Movie>();
            return new List<Movie>(records);
        }
    }
    public static List<UserRate> GetRatings(string pathToFile)
    {
        using (var reader = new StreamReader(pathToFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<UserRate>();
            return new List<UserRate>(records);
        }
    }
    public static List<User> GetUsers(string pathToFile)
    {
        using (var reader = new StreamReader(pathToFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<User>();
            return new List<User>(records);
        }
    }
}
