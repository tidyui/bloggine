/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Bloggine.Services;

namespace Bloggine.Http
{
    /// <summary>
    /// Middleware for routing requests.
    /// </summary>
    /// <remarks>
    /// Default constructor.
    /// </remarks>
    /// <param name="next">The next middleware component in the pipeline</param>
    /// <param name="logger">The optional logger</param>
    internal sealed class BlogMiddleware(RequestDelegate next, ILogger<BlogMiddleware> logger = null)
    {
        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <param name="blog">The blog service</param>
        /// <returns>The async task of the middleware component next in the pipeline</returns>
        /// <exception cref="ArgumentNullException">If context is null</exception>
        /// <exception cref="ArgumentNullException">If blog is null</exception>
        public async Task InvokeAsync(HttpContext context, IBlogService blog)
        {
            // Validate input params
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(blog);

            // Check if we have a requested url
            if (context.Request.Path.HasValue)
            {
                var url = context.Request.Path.Value[1..];

                if (!string.IsNullOrWhiteSpace(url))
                {
                    // Only rewrite if the url matches one of the available posts
                    var post = blog.GetPosts(p => p.Slug == url).FirstOrDefault();

                    if (post != null)
                    {
                        logger?.LogInformation($"Rewriting [/{ url }] to [/post/{ url }]");
                        context.Request.Path = new PathString($"/post/{ url }");

                        if (post.Settings.IsCached)
                        {
                            logger?.LogDebug($"Checking cache headers for [{ url }]");

                            // Check for browser cache
                            var reqHeaders = context.Request.GetTypedHeaders();
                            if (reqHeaders.IfNoneMatch.Any(h => h.Tag == post.Settings.ETag))
                            {
                                // Correct version is cached, return Not Modified
                                logger?.LogInformation($"ETag [{ post.Settings.ETag }]. Returning Not Modified");
                                context.Response.StatusCode = 304;
                                return;
                            }

                            // Set response cache headers
                            var headers = context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromSeconds(post.Settings.CacheMaxAge ?? blog.Options.CacheMaxAge)
                            };
                            headers.ETag = new EntityTagHeaderValue(post.Settings.ETag);
                            headers.LastModified = post.LastModified;
                        }
                    }
                    else
                    {
                        logger?.LogDebug($"Passing through request to [{ context.Request.Path.Value }]");
                    }
                }
                else
                {
                    logger?.LogDebug($"Passing through request to [{ context.Request.Path.Value }]");
                }
            }
            await next(context).ConfigureAwait(false);
        }
    }
}