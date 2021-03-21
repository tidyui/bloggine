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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using Bloggine.Models;

namespace Bloggine.Services
{
    /// <summary>
    /// The main application service.
    /// </summary>
    public sealed class BlogService : IBlogService
    {
        private readonly ILogger _logger;
        private Dictionary<string, PostInfo> _posts = new Dictionary<string, PostInfo>();
        private readonly IDeserializer _deserializer;
        private readonly MarkdownPipeline _pipeline;

        /// <summary>
        /// Gets/sets the settings.
        /// </summary>
        public BlogConfig Settings { get; set; }

        /// <summary>
        /// Gets/sets the total number of posts.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets all of the available posts in descending
        /// chronological order.
        /// </summary>
        public PostInfo[] Posts =>
            _posts.Values.OrderByDescending(p => p.Published).ToArray();

        /// <summary>
        /// Gets the available categories sorted in alphabetical order.
        /// </summary>
        public Taxonomy[] Categories =>
            _posts.Values.Where(p => !string.IsNullOrWhiteSpace(p.Category))
                .GroupBy(p => p.Category)
                .Select(g => new Taxonomy
                {
                    Title = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Title)
                .ToArray();

        /// <summary>
        /// Gets the available tags sorted in alphabetical order.
        /// </summary>
        public Taxonomy[] Tags =>
            _posts.Values.SelectMany(p => p.Tags).GroupBy(p => p)
                .Select(g => new Taxonomy
                {
                    Title = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Title)
                .ToArray();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="config">The blog config</param>
        /// <param name="factory">The logger factory</param>
        public BlogService(BlogConfig config, ILoggerFactory factory = null)
        {
            // Store services
            Settings = config;

            // Create logger
            if (factory != null)
            {
                _logger = factory.CreateLogger(typeof(BlogService));
            }

            // Create yaml deserializer
            _deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            // Create markdown pipeline
            _pipeline = new Markdig.MarkdownPipelineBuilder()
                .UsePipeTables()
                .Build();

            // Load the structure
            Init();
        }

        /// <summary>
        /// Initializes the content structure.
        /// </summary>
        public void Init()
        {
            // Reset the post collection
            _posts = new Dictionary<string, PostInfo>();

            // Open data directory
            _logger?.LogInformation($"Opening data directory [{ Settings.DataPath }]");
            var dir = new DirectoryInfo(Settings.DataPath);
            var files = dir.GetFiles("*.md");

            // Set post count
            Count = files.Length;

            // Scan all files for meta-data
            foreach (var info in files)
            {
                var post = LoadFile(info);
                _posts[post.Slug] = post;
            }
        }

        /// <summary>
        /// Reloads the file with the given path.
        /// </summary>
        /// <param name="path">The full path</param>
        public void Reload(string path)
        {
            var info = new FileInfo(path);
            var post = LoadFile(info);

            _posts[post.Slug] = post;
        }

        /// <summary>
        /// Deletes the post with the given path.
        /// </summary>
        /// <param name="path">The full path</param>
        public void Delete(string path)
        {
            var post = _posts.Values.FirstOrDefault(p => p.Settings.Path == path);

            if (post != null)
            {
                _posts.Remove(post.Slug);
            }
        }

        /// <summary>
        /// Checks if an item exists with the given slug.
        /// </summary>
        /// <param name="slug">The requested slug</param>
        /// <returns>If it matches a blog item</returns>
        public bool Exists(string slug)
        {
            return _posts.ContainsKey(slug);
        }

        /// <summary>
        /// Gets the posts matching the given expression.
        /// </summary>
        /// <param name="exp">The optional expression</param>
        /// <param name="take">The optional number of posts to return at the most</param>
        /// <returns>The matching posts</returns>
        public PostInfo[] GetPosts(Func<PostInfo, bool> exp = null, int? take = null)
        {
            // Get the matching posts
            var posts = exp != null ? Posts.Where(exp) : Posts;

            // Limit result
            if (take.HasValue)
            {
                posts = posts.Take(take.Value);
            }

            // Return result
            return posts.ToArray();
        }

        /// <summary>
        /// Gets the posts matching the given expression.
        /// </summary>
        /// <param name="exp">The expression</param>
        /// <param name="page">The zero based page index</param>
        /// <param name="pageSize">The optional page size</param>
        /// <param name="take">The optional number of posts to return at the most</param>
        /// <returns>The matching posts</returns>
        public PagedResult GetPagedPosts(Func<PostInfo, bool> exp, int page, int? pageSize = null, int? take = null)
        {
            var result = new PagedResult();

            // Get the matching posts
            var posts = Posts.Where(exp);

            // Get the current page size
            pageSize = pageSize.HasValue ? pageSize.Value : Settings.PageSize;

            // Store paging info
            result.CurrentPage = page;
            result.TotalPosts = posts.Count();
            result.TotalPages = (int)Math.Ceiling(result.TotalPosts / (double)pageSize);

            // Select the correct page
            posts = posts.Skip(pageSize.Value * page).Take(pageSize.Value);

            // Limit result
            if (take.HasValue)
            {
                _logger?.LogDebug($"Limiting result to [{ take }] posts");
                posts = posts.Take(take.Value);
            }

            // Store the posts
            result.Posts = posts.ToArray();

            // Return result
            return result;
        }

        /// <summary>
        /// Gets the full model for the post with the matching slug. Returns
        /// null if the post can't be found.
        /// </summary>
        /// <param name="slug">The unique slug</param>
        /// <returns>The post model</returns>
        public async Task<Post> GetBySlugAsync(string slug)
        {
            if (_posts.TryGetValue(slug, out var info))
            {
                var file = new FileInfo(info.Settings.Path);

                using (var sr = new StreamReader(file.OpenRead()))
                {
                    for (var n = 0; n < info.Settings.BodyStart; n++)
                    {
                        await sr.ReadLineAsync();
                    }
                    var md = await sr.ReadToEndAsync();

                    return new Post
                    {
                        Title = info.Title,
                        Slug = info.Slug,
                        PrimaryImage = info.PrimaryImage,
                        Excerpt = info.Excerpt,
                        Category = info.Category,
                        Tags = info.Tags,
                        Published = info.Published,
                        LastModified = info.LastModified,
                        Body = Markdown.ToHtml(md, _pipeline),
                        Settings = info.Settings
                    };
                }
            }
            return null;
        }

        private PostInfo LoadFile(FileInfo info)
        {
            _logger?.LogInformation($"Reading meta data for file [{ info.Name }]");

            using (var sr = new StreamReader(info.OpenRead()))
            {
                var sb = new StringBuilder();
                var post = new PostInfo();
                var start = 0;

                if (!sr.EndOfStream && sr.Peek() == '-')
                {
                    sr.ReadLine();
                    start++;

                    while (!sr.EndOfStream)
                    {
                        start++;
                        var line = sr.ReadLine();

                        if (line.StartsWith("---")) break;

                        sb.AppendLine(line);
                    }
                    post = _deserializer.Deserialize<PostInfo>(sb.ToString());
                    post.Settings = _deserializer.Deserialize<PostSettings>(sb.ToString());
                }

                // Ensure title
                if (string.IsNullOrWhiteSpace(post.Title))
                    post.Title = GenerateTitle(info.FullName);
                // Ensure slug
                if (string.IsNullOrWhiteSpace(post.Slug))
                    post.Slug = BlogUtils.GenerateSlug(post.Title);
                // Ensure published & last modification dates
                if (post.Published == DateTime.MinValue)
                    post.Published = info.CreationTimeUtc;
                post.LastModified = info.LastWriteTimeUtc;

                // Ensure ETag
                if (string.IsNullOrWhiteSpace(post.Settings.ETag))
                    post.Settings.ETag = BlogUtils.GenerateETag(post.Title, post.LastModified);

                // Store path & start line of the body
                post.Settings.Path = info.FullName;
                post.Settings.BodyStart = start;

                return post;
            }
        }

        /// <summary>
        /// Generates a title from the file path.
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>The generated title</returns>
        private string GenerateTitle(string path)
        {
            var info = new FileInfo(path);

            if (!string.IsNullOrWhiteSpace(info.Extension))
            {
                return info.Name.Replace(info.Extension, "");
            }
            return info.Name;
        }
    }
}