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
    /// Parameters for filter a post query.
    /// </summary>
    public sealed class PostQuery
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
        /// Gets/sets the post type filter.
        /// </summary>
        public PostTypeFilter Type { get; set; } = PostTypeFilter.UnPinned;
    }
}