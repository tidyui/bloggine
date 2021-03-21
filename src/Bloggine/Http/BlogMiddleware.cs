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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Bloggine.Services;

namespace Bloggine.Http
{
    /// <summary>
    /// Middleware for routing requests.
    /// </summary>
    public sealed class BlogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="next">The next middleware component in the pipeline</param>
        /// <param name="factory">The optional logger factory</param>
        public BlogMiddleware(RequestDelegate next, ILoggerFactory factory = null)
        {
            _next = next;

            if (factory != null)
            {
                _logger = factory.CreateLogger(typeof(BlogMiddleware));
            }
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <param name="blog">The blog service</param>
        /// <returns>The async task of the middleware component next in the pipeline</returns>
        public async Task InvokeAsync(HttpContext context, IBlogService blog)
        {
            // Check if we have a requested url
            if (context.Request.Path.HasValue)
            {
                var url = context.Request.Path.Value.Substring(1);

                if (!string.IsNullOrWhiteSpace(url))
                {
                    // Only rewrite if the url matches one of the available posts
                    var post = blog.GetPosts(p => p.Slug == url).FirstOrDefault();

                    if (post != null)
                    {
                        _logger?.LogInformation($"Rewriting [/{ url }] to [/post/{ url }]");
                        context.Request.Path = new PathString($"/post/{ url }");

                        if (post.Settings.IsCached)
                        {
                            _logger?.LogDebug($"Checking cache headers for [{ url }]");

                            // Check for browser cache
                            var reqHeaders = context.Request.GetTypedHeaders();
                            if (reqHeaders.IfNoneMatch.Any(h => h.Tag == post.Settings.ETag))
                            {
                                // Correct version is cached, return Not Modified
                                _logger?.LogInformation($"ETag [{ post.Settings.ETag }]. Returning Not Modified");
                                context.Response.StatusCode = 304;
                                return;
                            }

                            // Set response cache headers
                            var headers = context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromSeconds(post.Settings.CacheMaxAge.HasValue
                                    ? post.Settings.CacheMaxAge.Value
                                    : blog.Settings.CacheMaxAge)
                            };
                            headers.ETag = new EntityTagHeaderValue(post.Settings.ETag);
                            headers.LastModified = post.LastModified;
                        }
                    }
                    else
                    {
                        _logger?.LogInformation($"Passing through request to [{ context.Request.Path.Value }]");
                    }
                } else
            {
                    _logger?.LogInformation($"Passing through request to [{ context.Request.Path.Value }]");
                }
            }
            await _next(context);
        }
    }
}