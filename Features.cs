namespace RateFilm
{
    class RateCommand
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
}
