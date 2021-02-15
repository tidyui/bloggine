using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bloggine;

namespace Template
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBloggine(options =>
            {
                // options.CacheMaxAge = 86400;
                // options.DataAssetPath = "Uploads";
                // options.DataPath = "Data";
                // options.Headline = "Just another markdown blog";
                // options.Title = "Bloggine";
                // options.Theme = "Default";
                // options.UseFileSystemWatcher = true;
                options.UseRazorRuntimeCompilation = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, BlogConfig config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseBloggine(env, config);
        }
    }
}
