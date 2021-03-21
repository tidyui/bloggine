/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

using System.Collections;
using System.Collections.Generic;

namespace Bloggine.Models
{
    /// <summary>
    /// Result from a paged query.
    /// </summary>
    public sealed class PagedResult : IEnumerable<PostInfo>
    {
        /// <summary>
        /// Gets/sets the zero-based index of the current page.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets/sets the total amount of pages in the set.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets/sets the total amount of posts in the set.
        /// </summary>
        public int TotalPosts { get; set; }

        /// <summary>
        /// Gets the number of posts in this result.
        /// </summary>
        public int Count => Posts.Length;

        /// <summary>
        /// Gets/sets if a previous page is available.
        /// </summary>
        public bool HasPrev => CurrentPage > 0;

        /// <summary>
        /// Gets/sets if a next page is available.
        /// </summary>
        public bool HasNext => CurrentPage < TotalPages - 1;

        /// <summary>
        /// Gets/sets the posts in the result.
        /// </summary>
        internal PostInfo[] Posts { get; set; }

        /// <summary>
        /// Gets the enumerator for the result.
        /// </summary>
        public IEnumerator<PostInfo> GetEnumerator()
        {
            return ((IEnumerable<PostInfo>)Posts).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}