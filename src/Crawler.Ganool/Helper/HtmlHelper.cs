using System;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Extensions;

namespace Crawler.Ganool.Helper
{
	public static class HtmlHelper
    {
	    public static string InnerText(this IElement element)
	    {
		    if (element == null) throw new ArgumentNullException(nameof(element));
		    return element.Descendents<IText>().FirstOrDefault()?.Data ?? String.Empty;
	    }
    }
}
