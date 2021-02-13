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
using System.Threading.Tasks;
using Bloggine.Models;

namespace Bloggine.Services
{
    /// <summary>
    /// The main application service.
    /// </summary>
    public interface IBlogService
    {
        /// <summary>
        /// Gets/sets the blog settings.
        /// </summary>
        BlogConfig Settings { get; set; }

        /// <summary>
        /// Gets/sets the total number of posts.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets all of the available posts in descending
        /// chronological order.
        /// </summary>
        PostInfo[] Posts { get; }

        /// <summary>
        /// Gets the available categories sorted in
        /// alphabetical order.
        /// </summary>
        Taxonomy[] Categories { get; }

        /// <summary>
        /// Gets the available tags sorted in alphabetical order.
        /// </summary>
        Taxonomy[] Tags { get; }

        /// <summary>
        /// Initializes the content structure from disc.
        /// </summary>
        void Init();

        /// <summary>
        /// Reloads the post with the given path.
        /// </summary>
        /// <param name="path">The full path</param>
        void Reload(string path);

        /// <summary>
        /// Deletes the post with the given path.
        /// </summary>
        /// <param name="path">The full path</param>
        void Delete(string path);

        /// <summary>
        /// Checks if an item exists with the given slug.
        /// </summary>
        /// <param name="slug">The requested slug</param>
        /// <returns>If it matches a blog item</returns>
        bool Exists(string slug);

        /// <summary>
        /// Gets the post matching the given filter.
        /// </summary>
        /// <param name="options">The filter options</param>
        /// <returns>The matching posts</returns>
        PostInfo[] GetPosts(Action<PostQuery> options = null);

        /// <summary>
        /// Gets the full model for the post with the matching slug. Returns
        /// null if the post can't be found.
        /// </summary>
        /// <param name="slug">The unique slug</param>
        /// <returns>The post model</returns>
        Task<Post> GetBySlugAsync(string slug);
    }
}