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
    /// Categories and tags are represented as taxonomies.
    /// </summary>
    public sealed class Taxonomy
    {
        /// <summary>
        /// Gets/sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets/sets the number of posts related to the taxonomy.
        /// </summary>
        public int Count { get; set; }
    }
}