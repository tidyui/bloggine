/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui
 *
 */

namespace Bloggine.Models
{
    /// <summary>
    /// The different ways you can filter a post query.
    /// </summary>
    public class PostFilter
    {
        /// <summary>
        /// Gets/sets the optional category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets/sets the optional slug.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets/sets the optional tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets/sets the post type. The default value is UnPinned.
        /// </summary>
        public PostType Type { get; set; } = PostType.UnPinned;

        /// <summary>
        /// The maximum amount of posts that should be returned.
        /// </summary>
        public int? Take { get; set; }
    }
}