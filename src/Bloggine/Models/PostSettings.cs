/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

namespace Bloggine.Models
{
    /// <summary>
    /// Settings that can be applied to posts using
    /// YAML headers.
    /// </summary>
    public sealed class PostSettings
    {
        /// <summary>
        /// Gets/sets the optional meta keywords.
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Gets/sets the optional meta description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the optional author.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Gets/sets if the post is pinned.
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets/sets if caching is enabled. The default
        /// value is true.
        /// </summary>
        public bool IsCached { get ; set; } = true;

        /// <summary>
        /// Gets/sets the cache max-age in seconds. If the setting is
        /// left empty the global max-age for the blog is used.
        /// </summary>
        public int? CacheMaxAge { get; set; }

        /// <summary>
        /// Gets/sets the entity tag.
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets/sets the filesystem path to the post.
        /// </summary>
        internal string Path { get; set; }

        /// <summary>
        /// Gets/sets the line number at which the main
        /// post body starts.
        /// </summary>
        internal int BodyStart { get; set; }
    }
}