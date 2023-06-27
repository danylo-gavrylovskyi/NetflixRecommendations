namespace Features
{
    class Levenshtein
    {
        public static void GetSimilarFilmNames(string filmName, IEnumerable<Movie> movieData)
        {
            
            Dictionary<Movie, int> distancesBetweenFilmesAndEnteredFilm = GetLevenshteinDistanceDictForFilms(movieData, filmName);
            var suggestions = distancesBetweenFilmesAndEnteredFilm.OrderBy(x => x.Value).Take(3);
            Console.WriteLine("No such film found :( Closest candidates:");
            foreach (var suggestion in suggestions)
            {
                Console.WriteLine(suggestion.Key.movie_title);
            }
        }

        private static Dictionary<Movie, int> GetLevenshteinDistanceDictForFilms(IEnumerable<Movie> movieData, string filmName) 
        {
            Dictionary<Movie, int> result = new Dictionary<Movie, int>();
            List<Movie> movies = movieData.ToList(); 
            for (int i = 0; i < movies.Count; i++)
            {
                result[movies[i]] = GetDamerauLevenshteinDistance(filmName, movies[i].movie_title!);
            }
            return result;
        }

        private static int GetDamerauLevenshteinDistance(string firstString, string secondString)
        {
            int[,] matrixx = new int[firstString.Length, secondString.Length];

            for (int i = 0; i < firstString.Length; i++)
            {
                matrixx[i, 0] = i;
            }
            for (int i = 0; i < secondString.Length; i++)
            {
                matrixx[0, i] = i;
            }

            for (int i = 1; i < firstString.Length; i++)
            {
                for (int j = 1; j < secondString.Length; j++)
                {
                    int cost = firstString[i] == secondString[j] ? 0 : 1;
                    matrixx[i, j] = Math.Min(matrixx[i - 1, j - 1] + cost, Math.Min(matrixx[i-1, j] + 1, matrixx[i, j-1] + 1));
                    if (i > 1 && j > 1 && firstString[i] == secondString[j-1] && firstString[i-1] == secondString[j]) 
                    {
                        matrixx[i, j] = Math.Min(matrixx[i, j], matrixx[i - 2, j - 2] + 1);
                    }
                }
            }

            return matrixx[firstString.Length - 1, secondString.Length - 1];
        }
    }

    class Recommendations
    {
        public static Movie GetRecommendedFilm(User me, List<User> users, List<UserRate> ratings, IEnumerable<Movie> movieData)
        {
            List<User> similarUsers = GetSimilarUsers(me, users);
            Movie recommendedMovie = GetLikedMovie(me, similarUsers, ratings, movieData);
            return recommendedMovie;
        }

        private static List<User> GetSimilarUsers(User me, List<User> users)
        {
            List<User> similarUsers = new List<User>();
            int similarUsersCount = 0;
            List<KeyValuePair<string, double>> preferences = me.positionIn8Dimension.OrderBy(x => x.Value).Take(3).ToList();
            for (int i = 0; i < users.Count && similarUsersCount < 3; i++)
            {
                List<KeyValuePair<string, double>> currentUserPreferences = users[i].positionIn8Dimension.OrderBy(x => x.Value).Take(3).ToList();
                if (currentUserPreferences.SequenceEqual(preferences)) similarUsers.Add(users[i]);
            }
            return similarUsers;
        }

        private static Movie GetLikedMovie(User me, List<User> similarUsers, List<UserRate> ratings, IEnumerable<Movie> movieData)
        {
            Movie result = new Movie();
            var highRatedMovies = ratings.Where(x => x.user_id == similarUsers[0].username && Convert.ToDouble(x.rating_val) >= 9).ToList();
            for (int i = 0; i < highRatedMovies.Count; i++)
            {
                if (ratings.Any(x => x.user_id == similarUsers[1].username && 
                                     x.movie_id == highRatedMovies[i].movie_id &&
                                     Convert.ToDouble(x.rating_val) >= 9) &&
                    ratings.Any(x => x.user_id == similarUsers[2].username &&
                                     x.movie_id == highRatedMovies[i].movie_id &&
                                     Convert.ToDouble(x.rating_val) >= 9) &&
                    !ratings.Any(x => x.user_id == me.username &&
                                 x.movie_id == highRatedMovies[i].movie_id)
                    )
                {
                    result = movieData.FirstOrDefault(film => film._id == highRatedMovies[i].movie_id)!;
                }
            }
            return result;
        }
    }

    class Discovery
    {
        public static Movie GetFilmForDiscovery(List<UserRate> ratings, IEnumerable<Movie> movieData, string genre, User me)
        {
            var bestMovies = GetBestMovies(ratings);
            foreach (var bestMovie in bestMovies)
            {
                if (movieData.Any(movie => movie.movie_id == bestMovie.Key && movie.genres!.Contains(genre)))
                {
                    Movie movieToDiscover = movieData.FirstOrDefault(movie => movie.movie_id == bestMovie.Key && movie.genres!.Contains(genre))!;
                    if (!ratings.Any(x => x.user_id == me.username && x.movie_id == movieToDiscover.movie_id)) return movieToDiscover;
                }
            }
            throw new Exception("Error when getting film for discovery mode");
        }
        private static IOrderedEnumerable<KeyValuePair<string, double>> GetBestMovies(List<UserRate> ratings)
        {
            Dictionary<string, double> movieRatings = new Dictionary<string, double>();
            for (int i = 0; i < ratings.Count; i++)
            {
                string movieID = ratings[i].movie_id!;
                if (movieRatings.ContainsKey(movieID))
                {
                    movieRatings[movieID] += Convert.ToDouble(ratings[i].rating_val);
                }
                else movieRatings[movieID] = Convert.ToDouble(ratings[i].rating_val);
            }
            var topMovies = movieRatings.OrderByDescending(x => x.Value);
            return topMovies;
        }
    }
}
