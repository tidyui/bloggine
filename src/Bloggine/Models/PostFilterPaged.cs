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
    /// The different ways you can filter a paged post query.
    /// </summary>
    public class PostFilterPaged : PostFilter
    {
        /// <summary>
        /// Gets/sets the zero-based index of the page to get.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets/sets the optional page size. If omitted the global
        /// page size will be used.
        /// </summary>
        public int? PageSize { get; set; }
    }
}