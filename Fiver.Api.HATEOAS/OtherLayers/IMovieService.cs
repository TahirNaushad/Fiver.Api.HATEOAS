using Fiver.Api.HATEOAS.Lib;

namespace Fiver.Api.HATEOAS.OtherLayers
{
    public interface IMovieService
    {
        PagedList<Movie> GetMovies(PagingParams pagingParams);
        Movie GetMovie(int id);
        void AddMovie(Movie item);
        void UpdateMovie(Movie item);
        void DeleteMovie(int id);
        bool MovieExists(int id);
    }
}
