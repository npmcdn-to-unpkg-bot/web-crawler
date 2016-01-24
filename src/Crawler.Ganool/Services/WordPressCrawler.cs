using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Extensions;
using Crawler.Ganool.Helper;
using Crawler.Ganool.ViewModel;

namespace Crawler.Ganool.Services
{
	/// <summary>
	/// Collection of Ganool site crawler when they're using WordPress as site
	/// </summary>
	public class WordPressCrawler : IGanoolService
	{
		/// <summary>
		/// Total movies per page from ganool site
		/// </summary>
		private const int GanoolPageSize = 5;
		private string siteUrl;

		public string SiteUrl
		{
			get { return siteUrl; }
			set
			{
				if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value), "Site URL can't be null");
				string url = value.Trim();
				if (!Regex.IsMatch(url, @"^https?://")) url = $"http://{url}";
				siteUrl = url.TrimEnd('/');
			}
		}

		public  List<Category> GetCategories()
		{
			if (string.IsNullOrEmpty(SiteUrl)) throw new ArgumentNullException(nameof(SiteUrl));
			var document = GetHtmlDocument(SiteUrl);
			if (document == null) return new List<Category>();
			var rootMenu = document.GetElementById("menu-main-menu");
			return FindSubMenu(rootMenu);
		}

		public  List<Movie> GetMovies(string keyword = null, int currentPage = 1, int pageSize = 10)
		{
			if (string.IsNullOrEmpty(SiteUrl)) throw new ArgumentNullException(nameof(SiteUrl));
			if (pageSize % GanoolPageSize != 0) throw new ArgumentException($"Page size must be multiplication of {GanoolPageSize}", nameof(pageSize));
			int totalRequest = pageSize / GanoolPageSize;
			var movies = new List<Movie>();
			for (int sequence = 1; sequence <= totalRequest; sequence++)
			{
				string url = SiteUrl;
				if (currentPage > 1 || sequence > 1) url += $"/page/{currentPage * totalRequest - (totalRequest - sequence)}";
				if (!string.IsNullOrEmpty(keyword)) url += $"/?s={WebUtility.UrlEncode(keyword)}";
				movies.AddRange(GetMoviesInternal(url));
			}

			return movies;
		}

		private List<Movie> GetMoviesInternal(string url)
		{
			var body = GetHtmlDocument(url);
			if (body == null) return new List<Movie>();
			var posts = body.QuerySelectorAll("#content .post");
			if (posts.Length < 1) return new List<Movie>();
			var movies = new List<Movie>();
			foreach (IElement post in posts)
			{
				var domTitle = post.QuerySelector(".entry-title a");
				if (domTitle == null) continue;
				var domContent = post.QuerySelector(".entry-content p") ?? post.QuerySelector(".entry-summary p");
				if (domContent == null) continue;
				var movie = new Movie
				{
					Title = domTitle.InnerHtml,
					DetailLink = domTitle.GetAttribute("href"),
					ImageLink = domContent.GetElementsByTagName("img").FirstOrDefault()?.GetAttribute("src")
				};
				foreach (var meta in domContent.GetElementsByTagName("strong"))
				{
					string name = meta.Text().Trim().TrimEnd(':').ToLower();
					if (string.IsNullOrEmpty(name)) continue;
					DefineMeta(name, meta, movie);
				}
				movies.Add(movie);
			}

			return movies;
		}

		public  MovieDetail GetMovieDetail(string slug)
		{
			if (string.IsNullOrEmpty(slug)) throw new ArgumentNullException(nameof(slug));
			string fullUrl = $"{SiteUrl}/{slug}";
			var document = GetHtmlDocument(fullUrl);
			if (document == null || document.BaseUrl.Path == "/") return null;
			var movie = new MovieDetail
			{
				DetailLink = fullUrl,
				Title = document.QuerySelectorAll("#content h1.entry-title").FirstOrDefault()?.InnerHtml
			};
			var post = document.QuerySelectorAll("#content .entry-content p").First();
			movie.ImageLink = post.GetElementsByTagName("img").FirstOrDefault()?.GetAttribute("src");
			foreach (var meta in post.GetElementsByTagName("strong"))
			{
				string name = meta.Text().Trim().TrimEnd(':').ToLower();
				DefineMeta(name, meta, movie);
			}
			movie.Synopsis = document.QuerySelector("#content .entry-content p:nth-of-type(2)").InnerHtml.Trim();
			foreach (var element in document.QuerySelectorAll("#content .entry-content p[style*=center]"))
			{
				string title = (element.QuerySelector("strong:first-child")?.InnerHtml ?? string.Empty).Trim().ToLower();
				if (title.Contains("trailer"))
				{
					var youtubeFrame = element.QuerySelector("iframe[src*='youtube.com']");
					if (youtubeFrame != null)
					{
						var match = Regex.Match(youtubeFrame.GetAttribute("src"), @"youtube.com/(embed/|watch\?v=)(?<code>[\w\-]+)", RegexOptions.IgnoreCase);
						if (match.Success) movie.YoutubeTrailerCode = match.Groups["code"].Value;
					}
				}
				else if (Regex.IsMatch(title, @"screen\s*shot", RegexOptions.IgnoreCase))
				{
					movie.ScreenshotImageLink = element.GetElementsByTagName("a").FirstOrDefault()?.GetAttribute("href");
				}
				else if (title.Contains("watch"))
				{
					movie.StreamingLink = element.GetElementsByTagName("iframe").FirstOrDefault()?.GetAttribute("src");
				}
				else if (title.Contains("download"))
				{
					foreach (var download in element.GetElementsByTagName("strong"))
					{
						string name = download.Text().Trim().TrimEnd(':');
						if (name.Equals("download", StringComparison.CurrentCultureIgnoreCase)) continue;
						movie.DownloadLinks.Add(new MovieDownload
						{
							Provider = name,
							Link = download.NextElementSibling.GetAttribute("href")
						});
					}
				}
			}

			return movie;
		}

		public  List<MovieLink> GetMostPopularMovies()
		{
			if (string.IsNullOrEmpty(SiteUrl)) throw new ArgumentNullException(nameof(SiteUrl));
			var document = GetHtmlDocument(SiteUrl);
			var output = new List<MovieLink>();
			if (document == null) return output;
			var container =
				document.QuerySelector("#main .widget-area ul > li.widget-container.widget_widget_tptn_pop");
			if (container == null) return output;
			string title = (container.QuerySelector(".widget-title")?.InnerHtml ?? string.Empty).Trim().ToLower();
			if (!title.Contains("popular")) return output;
			output.AddRange(container
				.QuerySelectorAll("ul > li a")
				.Select(anchor => new MovieLink
				{
					DetailLink = anchor.GetAttribute("href"),
					Title = anchor.Text()
				}));
			return output;
		}

		public  List<MovieLink> GetRecentMovies()
		{
			if (string.IsNullOrEmpty(SiteUrl)) throw new ArgumentNullException(nameof(SiteUrl));
			var document = GetHtmlDocument(SiteUrl);
			var output = new List<MovieLink>();
			if (document == null) return output;
			var container =
				document.QuerySelector("#main .widget-area ul > li.widget-container.widget_recent_entries");
			if (container == null) return output;
			string title = (container.QuerySelector(".widget-title")?.InnerHtml ?? string.Empty).Trim().ToLower();
			if (!title.Contains("recent")) return output;
			output.AddRange(container
				.QuerySelectorAll("ul > li a")
				.Select(anchor => new MovieLink
				{
					DetailLink = anchor.GetAttribute("href"),
					Title = anchor.Text()
				}));
			return output;
		}

		private static void DefineMeta(string name, IElement dom, Movie movie)
		{
			if (dom == null) throw new ArgumentNullException(nameof(dom));
			if (movie == null) throw new ArgumentNullException(nameof(movie));
			if (new[] { "info", "subtitle", "synopsis" }.Contains(name))
			{
				string link = dom.NextElementSibling.GetAttribute("href");
				if (name == "info") movie.ImdbInfo = link;
				else if (name == "subtitle") movie.SubtitleLink = link;
				//else if (name == "synopsis") movie.DetailLink = link;
				return;
			}
			if (name == "release date")
			{
				string date = dom.NextSibling.NodeValue.Trim();
				date = Regex.Replace(date, @"\([^\)]+\)", string.Empty).Trim();
				movie.ReleaseDate = Regex.IsMatch(date, @"^\d{4}$")
					? new DateTime(int.Parse(date), 1, 1, 0, 0, 0)
					: DateTime.Parse(date);
				return;
			}
			if (new[] { "genre", "stars" }.Contains(name))
			{
				var values = dom.NextSibling.NodeValue.Split(',').Select(word => word.Trim()).ToList();
				if (name == "genre") values.ForEach(movie.Genres.Add);
				else if (name == "stars") values.ForEach(movie.Stars.Add);
				return;
			}
			string value = dom.NextSibling.NodeValue.Trim();
			if (name == "quality") movie.Quality = value;
			else if (name == "source") movie.Source = value;
		}

		private static List<Category> FindSubMenu(IElement element)
		{
			var output = new List<Category>();
			if (element == null) return output;
			foreach (var li in element.Children)
			{
				var category = new Category { Name = li.InnerText().Trim() };
				if (string.IsNullOrEmpty(category.Name)) continue;
				string id = li.GetAttribute("id");
				if (!string.IsNullOrEmpty(id)) category.Id = int.Parse(Regex.Replace(id, @"\D", string.Empty));
				var link = li.GetElementsByTagName("a").FirstOrDefault();
				if (link != null) category.Link = link.GetAttribute("href");
				//var subMenu = li.Descendents<IElement>().FirstOrDefault(el => el.TagName == "ul");
				var subMenu = li.GetElementsByTagName("ul").FirstOrDefault();
				if (subMenu != null) category.SubCategories.AddRange(FindSubMenu(subMenu));
				output.Add(category);
			}

			return output;
		}

		private static IDocument GetHtmlDocument(string url)
		{
			return BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(url).Result;
		}
	}
}
