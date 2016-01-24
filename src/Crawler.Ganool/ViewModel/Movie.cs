using System;
using System.Collections.Generic;

namespace Crawler.Ganool.ViewModel
{
	public class Movie : MovieLink
    {
		public string ImdbInfo { get; set; }
	    public List<string> Genres { get; private set; }
	    public DateTime ReleaseDate { get; set; }
	    public List<string> Stars { get; private set; }
	    public string Source { get; set; }
	    public string Quality { get; set; }
		public string SubtitleLink { get; set; }
		public string ImageLink { get; set; }

		public Movie()
		{
			Genres = new List<string>();
			Stars = new List<string>();
		}
	}
}
