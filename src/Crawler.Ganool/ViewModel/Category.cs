using System.Collections.Generic;

namespace Crawler.Ganool.ViewModel
{
	public class Category
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Link { get; set; }

		public List<Category> SubCategories { get; private set; }

		public Category()
		{
			SubCategories = new List<Category>();
		}
	}
}
