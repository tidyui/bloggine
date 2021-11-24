/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

using System;
using System.IO;
using Bloggine.Http;
using Bloggine.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Bloggine
{
    /// <summary>
    /// Startup filter for setting up Bloggine.
    /// </summary>
    internal class BlogStartup : IStartupFilter
    {
        /// <summary>
        /// Configures the blog application.
        /// </summary>
        /// <param name="next">The next startup filter in the pipeline.</param>
        /// <returns>The configuration action</returns>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // Get the configured blog options
                var options = app.ApplicationServices.GetService<IOptions<BlogOptions>>().Value;

                // Static content
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Blog.Environment.ContentRootPath, options.DataPath, options.DataAssetPath)),
                    RequestPath = $"/{ options.DataAssetPath }"
                });

                // Static theme content
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Blog.Environment.ContentRootPath, "Themes", options.Theme, "Assets")),
                    RequestPath = "/theme"
                });

                // Setup middleware
                app.UseMiddleware<BlogMiddleware>();
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });

                if (options.UseFileSystemWatcher)
                {
                    var blog = app.ApplicationServices.GetService<IBlogService>();
                    blog.InitFilewatcher(Blog.Environment.ContentRootPath);
                }
                next(app);
            };
        }
    }
}
