using System;
using Crawler.Ganool.Services;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crawler.Ganool
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			// Set up configuration sources.
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets();
			}

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddOptions();
			/*services.Configure<ViewModel.Settings>(setting =>
			{
				setting.Url = Configuration["Site:Target"];
				setting.Secure = bool.Parse(Configuration["Site:Secure"]);
				setting.PagingSize = int.Parse(Configuration["Site:Paging"]);
			});*/
			services.Configure<ViewModel.Settings>(Configuration.GetSection("Site"));
			services.AddSingleton<IMemoryCache>(provider => new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromHours(1) }));
			services.AddSingleton<IGanoolService, WordPressCrawler>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			//loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			if (env.IsDevelopment())
			{
				loggerFactory.AddDebug();
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());
			app.UseStaticFiles();
			app.UseCors(builder =>
			{
				builder.AllowAnyOrigin();
				builder.AllowAnyHeader();
				builder.WithMethods("GET", "POST");
			});
			app.Use(async (context, next) =>
			{
				context.Response.Headers.Remove("Server");
				await next();
			});
			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
		}

		// Entry point for the application.
		public static void Main(string[] args) => WebApplication.Run<Startup>(args);
	}
}
