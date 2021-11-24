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
using Bloggine.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

[assembly: CLSCompliant(false)]
namespace Bloggine
{
    /// <summary>
    /// The main entry point for creating a new blog application.
    /// </summary>
    public sealed class Blog
    {
        internal static IWebHostEnvironment Environment;
        private static BlogOptions _options;

        /// <summary>
        /// Creates a new blog web application.
        /// </summary>
        /// <param name="args">The application arguments</param>
        /// <param name="options">The optional blog options</param>
        /// <returns>The web application</returns>
        public static WebApplication Create(string[] args, Action<BlogOptions> options = null)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            ConfigureServices(builder, options);

            Environment = builder.Environment;

            // Build app 
            return builder.Build();
        }

        /// <summary>
        /// Configures the needed services.
        /// </summary>
        /// <param name="builder">The web application builder</param>
        /// <param name="options">The current blog options</param>
        private static void ConfigureServices(WebApplicationBuilder builder, Action<BlogOptions> options)
        {
            // Add services
            builder.Services.Configure<BlogOptions>(o => 
            {
                options?.Invoke(o);
                _options = o;
            });
            builder.Services.AddSingleton<IBlogService, BlogService>();
            builder.Services.AddTransient<IStartupFilter, BlogStartup>();

            // Add frameworks
            var mvcBuilder = builder.Services.AddRazorPages();
            if (builder.Environment.EnvironmentName.ToUpperInvariant() == "DEVELOPMENT")
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }

            // Configure
            builder.Services.Configure<RazorPagesOptions>(o =>
            {
                o.RootDirectory = $"/Themes/{ _options.Theme }";
            });
        }
    }
}
