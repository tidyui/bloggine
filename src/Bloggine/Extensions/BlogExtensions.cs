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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Bloggine;
using Bloggine.Http;
using Bloggine.Services;

/// <summary>
/// Extension methods for easy project startup.
/// </summary>
public static class BlogExtensions
{
    /// <summary>
    /// Adds Marks to the service collection.
    /// </summary>
    /// <param name="services">The current services</param>
    /// <param name="options">The optional service options</param>
    /// <returns>The updated service</returns>
    public static IServiceCollection AddBloggine(this IServiceCollection services, Action<BlogConfig> options = null)
    {
        var config = new BlogConfig();

        options?.Invoke(config);

        // Add services
        services.AddSingleton<BlogConfig>(config);
        services.AddSingleton<IBlogService, BlogService>();

        // Add Razor Pages
        var builder = services.AddRazorPages();

        if (config.UseRazorRuntimeCompilation)
        {
            builder.AddRazorRuntimeCompilation();
        }

        services.Configure<RazorPagesOptions>(options =>
        {
            options.RootDirectory = $"/Themes/{ config.Theme }";
        });

        return services;
    }

    /// <summary>
    /// Adds Marks to the application builder.
    /// </summary>
    /// <param name="app">The current builder</param>
    /// <param name="env">The web host environment</param>
    /// <param name="config">The current blog config</param>
    /// <returns>The updated application builder</returns>
    public static IApplicationBuilder UseBloggine(this IApplicationBuilder app,
        IWebHostEnvironment env, BlogConfig config)
    {
        // Static blog content
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(env.ContentRootPath, config.DataPath, config.DataAssetPath)),
            RequestPath = $"/{ config.DataAssetPath.ToLower() }"
        });

        // Static theme content
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(env.ContentRootPath, "Themes", config.Theme, "Assets")),
            RequestPath = "/theme"
        });

        // Setup middleware and ASP.NET components
        app.UseMiddleware<BlogMiddleware>();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
        });

        if (config.UseFileSystemWatcher)
        {
            var watcher = new FileSystemWatcher()
            {
                Path = Path.Combine(env.ContentRootPath, config.DataPath),
                NotifyFilter = NotifyFilters.LastWrite|NotifyFilters.FileName,
                Filter = "*.md"
            };

            watcher.Changed += (source, e) => {
                try {
                    var blog = app.ApplicationServices.GetService<IBlogService>();
                    blog.Reload(e.FullPath);
                } catch {}
            };
            watcher.Created += (source, e) => {
                try {
                    var blog = app.ApplicationServices.GetService<IBlogService>();
                    blog.Reload(e.FullPath);
                } catch {}
            };
            watcher.Deleted += (source, e) => {
                try {
                    var blog = app.ApplicationServices.GetService<IBlogService>();
                    blog.Delete(e.FullPath);
                } catch {}
            };
            /*
             * TODO
             *
            watcher.Renamed += (source, e) => {
                Should be a delete + reload
            };
             */

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }
        return app;
    }
}