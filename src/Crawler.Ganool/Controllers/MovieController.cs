using System.Collections.Generic;
using Crawler.Ganool.Services;
using Crawler.Ganool.ViewModel;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;

namespace Crawler.Ganool.Controllers
{
	[Route("api/[controller]")]
    public class MovieController : Controller
    {
		private readonly Settings setting;
		private readonly IMemoryCache cache;
		private readonly IGanoolService ganool;
		private string BaseUrl => $"http://{setting.Url}";

		public MovieController(IGanoolService ganool, IMemoryCache cache, IOptions<Settings> optionSetting)
		{
			setting = optionSetting.Value;
			this.cache = cache;
			this.ganool = ganool;
			this.ganool.SiteUrl = BaseUrl;
		}

		[HttpGet]
		[HttpGet("page/{page?}")]
		public List<Movie> GetList(int page = 1)
		{
			var movies = ganool.GetMovies(currentPage: page, pageSize: setting.PagingSize);
			movies.ForEach(movie => movie.InternalLink = Url.Link("MovieDetail", new { slug = movie.Slug }));
			return movies;
		}

		[HttpGet("popular", Order = 1)]
		public List<MovieLink> GetPopular()
		{
			var links = cache.Get<List<MovieLink>>("popular");
			if (links != null) return links;
			links = ganool.GetMostPopularMovies();
			links.ForEach(link => link.InternalLink = Url.Link("MovieDetail", new { slug = link.Slug }));
			cache.Set("popular", links);

			return links;
		}

		[HttpGet("recent", Order = 2)]
		public List<MovieLink> GetRecent()
		{
			var links = cache.Get<List<MovieLink>>("recent");
			if (links != null) return links;
			links = ganool.GetRecentMovies();
			links.ForEach(link => link.InternalLink = Url.Link("MovieDetail", new { slug = link.Slug }));
			cache.Set("recent", links);

			return links;
		}

		[HttpGet("search/{keyword}", Order = 3)]
		[HttpGet("search/{keyword}/page/{page}", Order = 3)]
		public List<Movie> Search(string keyword, int page = 1)
		{
			if (string.IsNullOrEmpty(keyword)) return new List<Movie>();
			var movies = ganool.GetMovies(keyword, page);
			movies.ForEach(movie => movie.InternalLink = Url.Link("MovieDetail", new { slug = movie.Slug }));
			return movies;
		}

		[HttpGet(@"{slug}", Name = "MovieDetail", Order = 4)]
        public IActionResult GetDetail(string slug)
		{
			var movie = ganool.GetMovieDetail(slug);
	        return movie != null ? Json(movie) : (IActionResult)HttpNotFound();
        }
    }
}
