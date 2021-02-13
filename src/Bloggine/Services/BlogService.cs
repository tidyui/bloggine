/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui
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
        /// Gets the post matching the given filter.
        /// </summary>
        /// <param name="options">The filter options</param>
        /// <returns>The matching posts</returns>
        public PostInfo[] GetPosts(Action<PostQuery> options = null)
        {
            IEnumerable<PostInfo> query = Posts;
            var filter = new PostQuery();

            options?.Invoke(filter);

            // Filter on slug
            if (!string.IsNullOrWhiteSpace(filter.Slug))
            {
                _logger?.LogDebug($"Filtering on slug [{ filter.Slug }]");
                if (_posts.TryGetValue(filter.Slug, out var post))
                {
                    query = new [] { post };
                }
                else
                {
                    query = new PostInfo[0];
                }
            }

            // Filter on type
            if (filter.Type == PostTypeFilter.Pinned)
            {
                _logger?.LogDebug($"Filtering on type [pinned]");
                query = query.Where(p => p.Settings.IsPinned);
            }
            else if (filter.Type == PostTypeFilter.UnPinned)
            {
                _logger?.LogDebug($"Filtering on type [unpinned]");
                query = query.Where(p => !p.Settings.IsPinned);
            }

            // Filter on category
            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                _logger?.LogDebug($"Filtering on category [{ filter.Category }]");
                query = query.Where(p => p.Category == filter.Category);
            }

            // Filter on tag
            if (!string.IsNullOrWhiteSpace(filter.Tag))
            {
                _logger?.LogDebug($"Filtering on tag [{ filter.Tag }]");
                query = query.Where(p => p.Tags.Contains(filter.Tag));
            }

            // Limit result
            if (filter.Take.HasValue)
            {
                _logger?.LogDebug($"Limiting result to [{ filter.Take }] posts");
                query = query.Take(filter.Take.Value);
            }

            // Return result
            return query.ToArray();
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
                        Body = Markdown.ToHtml(md),
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