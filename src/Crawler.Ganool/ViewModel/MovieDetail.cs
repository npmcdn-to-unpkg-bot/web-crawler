using System.Collections.Generic;

namespace Crawler.Ganool.ViewModel
{
	public class MovieDetail : Movie
    {
	    public string Synopsis { get; set; }
	    public string YoutubeTrailerCode { get; set; }
		public string YoutubeTrailerLink => !string.IsNullOrEmpty(YoutubeTrailerCode) ? $"https://www.youtube.com/watch?v={YoutubeTrailerCode}" : string.Empty;
	    public string ScreenshotImageLink { get; set; }
		public List<MovieDownload> DownloadLinks { get; private set; }
		public string StreamingLink { get; set; }

		public MovieDetail()
		{
			DownloadLinks = new List<MovieDownload>();
		}
    }
}
