using System.Collections.Generic;
using Crawler.Ganool.ViewModel;

namespace Crawler.Ganool.Services
{
	public interface IGanoolService
    {
		/// <summary>
		/// Ganool site URL
		/// </summary>
		string SiteUrl { get; set; }

		/// <summary>
		/// Get category list
		/// </summary>
		/// <returns>All categories</returns>
		List<Category> GetCategories();

		/// <summary>
		/// Get all movies
		/// </summary>
		/// <param name="keyword">Keyword to search</param>
		/// <param name="currentPage">Current page</param>
		/// <param name="pageSize">Total movie per page</param>
		/// <returns>Paged movie list</returns>
		List<Movie> GetMovies(string keyword = null, int currentPage = 1, int pageSize = 10);

		/// <summary>
		/// Get movie detail
		/// </summary>
		/// <param name="slug">Movie slug</param>
		/// <returns></returns>
	    MovieDetail GetMovieDetail(string slug);

		/// <summary>
		/// Get most popular movie list
		/// </summary>
		/// <returns>All most popular movie list</returns>
		List<MovieLink> GetMostPopularMovies();

		/// <summary>
		/// Get most recent added movie list
		/// </summary>
		/// <returns>All recent movie list</returns>
		List<MovieLink> GetRecentMovies();
    }
}
