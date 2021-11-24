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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloggine.Models;
using Markdig;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;

namespace Bloggine.Services
{
    /// <summary>
    /// The main application service.
    /// </summary>
    internal sealed class BlogService : IBlogService, IDisposable
    {
        private readonly ILogger<BlogService> _logger;
        private Dictionary<string, PostInfo> _posts = new();
        private readonly IDeserializer _deserializer;
        private readonly MarkdownPipeline _pipeline;
        private FileSystemWatcher _watcher;

        /// <summary>
        /// Gets/sets the options.
        /// </summary>
        public BlogOptions Options { get; set; }

        /// <summary>
        /// Gets/sets the total number of posts.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the available categories sorted in alphabetical order.
        /// </summary>
        public IEnumerable<Taxonomy> Categories =>
            _posts.Values.Where(p => !string.IsNullOrWhiteSpace(p.Category))
                .GroupBy(p => p.Category)
                .Select(g => new Taxonomy
                {
                    Title = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Title)
                .AsEnumerable();

        /// <summary>
        /// Gets the available tags sorted in alphabetical order.
        /// </summary>
        public IEnumerable<Taxonomy> Tags =>
            _posts.Values.SelectMany(p => p.Tags).GroupBy(p => p)
                .Select(g => new Taxonomy
                {
                    Title = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Title)
                .AsEnumerable();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="options">The blog options</param>
        /// <param name="logger">The optional logger</param>
        //public BlogService(BlogOptions options, ILogger<BlogService> logger = null)
        public BlogService(IOptions<BlogOptions> options, ILogger<BlogService> logger = null)
        {
            // Store services
            Options = options?.Value;
            _logger = logger;

            // Create yaml deserializer
            _deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            // Create markdown pipeline
            _pipeline = new MarkdownPipelineBuilder()
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
            _logger?.LogDebug($"Opening data directory [{ Options.DataPath }]");
            var dir = new DirectoryInfo(Options.DataPath);
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
        /// Initializes the file system watcher.
        /// </summary>
        /// <param name="contentRootPath">The content root path of the application</param>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Logging should catch all exceptions.")]
        public void InitFilewatcher(string contentRootPath)
        {
            _watcher = new()
            {
                Path = Path.Combine(contentRootPath, Options.DataPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.md"
            };

            _watcher.Changed += (source, e) =>
            {
                try
                {
                    Reload(e.FullPath);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Filewatcher.Changed { e.Name }: { ex.Message }");
                }
            };
            _watcher.Created += (source, e) =>
            {
                try
                {
                    Reload(e.FullPath);

                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Filewatcher.Created { e.Name }: { ex.Message }");
                }
            };
            _watcher.Deleted += (source, e) =>
            {
                try
                {
                    Delete(e.FullPath);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Filewatcher.Deleted { e.Name }: { ex.Message }");
                }
            };
            _watcher.Renamed += (source, e) => 
            {
                try
                {
                    Delete(e.OldFullPath);
                    Reload(e.FullPath);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Filewatcher.Renamed { e.OldName }: { ex.Message }");
                }
            };
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Reloads the file with the given path.
        /// </summary>
        /// <param name="path">The full path</param>
        public void Reload(string path)
        {
            var info = new FileInfo(path);

            if (info.Exists)
            {
                var post = LoadFile(info);
                _posts[post.Slug] = post;

            }
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
        public IEnumerable<PostInfo> GetPosts(Func<PostInfo, bool> exp = null, int? take = null)
        {
            // Get the matching posts
            var posts = _posts.Values.OrderByDescending(p => p.Published).AsEnumerable();

            // Execute query
            if (exp != null)
            {
                posts = posts.Where(exp);
            }

            // Limit result
            if (take.HasValue)
            {
                posts = posts.Take(take.Value);
            }

            // Return result
            return posts.AsEnumerable();
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
            var posts = GetPosts(exp);

            // Get the current page size
            pageSize ??= Options.PageSize;

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

                using var sr = new StreamReader(file.OpenRead());

                for (var n = 0; n < info.Settings.BodyStart; n++)
                {
                    await sr.ReadLineAsync().ConfigureAwait(false);
                }
                var md = await sr.ReadToEndAsync().ConfigureAwait(false);

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
            return null;
        }

        /// <summary>
        /// Disposes the service.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }

        private PostInfo LoadFile(FileInfo info)
        {
            _logger?.LogInformation($"Reading meta data for file [{ info.Name }]");

            using var sr = new StreamReader(info.OpenRead());

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

                    if (line.StartsWith("---", StringComparison.InvariantCulture)) break;

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

        /// <summary>
        /// Generates a title from the file path.
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>The generated title</returns>
        private static string GenerateTitle(string path)
        {
            var info = new FileInfo(path);

            if (!string.IsNullOrWhiteSpace(info.Extension))
            {
                return info.Name.Replace(info.Extension, "", StringComparison.InvariantCulture);
            }
            return info.Name;
        }
    }
}