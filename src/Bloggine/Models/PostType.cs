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
    /// The different types of posts that can be fetched.
    /// </summary>
    public enum PostType
    {
        /// <summary>
        /// All of the post.
        /// </summary>
        All,
        /// <summary>
        /// Only pinned posts.
        /// </summary>
        Pinned,
        /// <summary>
        /// Only unpinned posts.
        /// </summary>
        UnPinned
    }
}