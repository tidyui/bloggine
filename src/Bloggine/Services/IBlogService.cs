/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

using Bloggine.Models;

namespace Bloggine.Services
{
    /// <summary>
    /// The main application service.
    /// </summary>
    public interface IBlogService
    {
        /// <summary>
        /// Gets/sets the current blog options.
        /// </summary>
        BlogOptions Options { get; set; }

        /// <summary>
        /// Gets/sets the total number of posts.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the available categories sorted in
        /// alphabetical order.
        /// </summary>
        IEnumerable<Taxonomy> Categories { get; }

        /// <summary>
        /// Gets the available tags sorted in alphabetical order.
        /// </summary>
        IEnumerable<Taxonomy> Tags { get; }

        /// <summary>
        /// Checks if an item exists with the given slug.
        /// </summary>
        /// <param name="slug">The requested slug</param>
        /// <returns>If it matches a blog item</returns>
        bool Exists(string slug);

        /// <summary>
        /// Gets the available posts matching the given expressions.
        /// </summary>
        /// <param name="exp">The optional expression</param>
        /// <param name="take">The optional number of posts to return at the most</param>
        /// <returns>The matching posts</returns>
        IEnumerable<PostInfo> GetPosts(Func<PostInfo, bool> exp = null, int? take = null);

        /// <summary>
        /// Gets the posts matching the given expression.
        /// </summary>
        /// <param name="exp">The expression</param>
        /// <param name="page">The zero based page index</param>
        /// <param name="pageSize">The optional page size</param>
        /// <param name="take">The optional number of posts to return at the most</param>
        /// <returns>The matching posts</returns>
        PagedResult GetPagedPosts(Func<PostInfo, bool> exp, int page, int? pageSize = null, int? take = null);

        /// <summary>
        /// Gets the full model for the post with the matching slug. Returns
        /// null if the post can't be found.
        /// </summary>
        /// <param name="slug">The unique slug</param>
        /// <returns>The post model</returns>
        Task<Post> GetBySlugAsync(string slug);
    }
}