using System.Collections.Generic;
using Crawler.Ganool.Services;
using Crawler.Ganool.ViewModel;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;

namespace Crawler.Ganool.Controllers
{
	[Route("api/[controller]")]
    public class CategoryController : Controller
    {
	    private readonly IMemoryCache cache;
		private readonly Settings setting;
		private readonly IGanoolService ganool;
		private string BaseUrl => $"http://{setting.Url}";

		public CategoryController(IGanoolService ganool, IMemoryCache cache, IOptions<Settings> options)
	    {
			setting = options.Value;
			this.ganool = ganool;
			this.ganool.SiteUrl = BaseUrl;
			this.cache = cache;
	    }

	    [HttpGet]
        public IEnumerable<Category> Get()
	    {
		    var categories = cache.Get<List<Category>>("categories");
		    if (categories == null)
		    {
			    categories = ganool.GetCategories();
			    cache.Set("categories", categories);
		    }

		    return categories;
	    }
    }
}
