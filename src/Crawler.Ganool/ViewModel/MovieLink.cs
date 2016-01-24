using System;
using Newtonsoft.Json;

namespace Crawler.Ganool.ViewModel
{
	public class MovieLink
    {
		public string Title { get; set; }
		public string DetailLink { get; set; }
		public string InternalLink { get; set; }
		public string Slug => DetailUri?.AbsolutePath.Trim('/');

		[JsonIgnore]
		public Uri DetailUri => !string.IsNullOrEmpty(DetailLink) ? new Uri(DetailLink) : null;
	}
}
