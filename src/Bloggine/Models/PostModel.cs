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
    /// The full post model.
    /// </summary>
    public sealed record PostModel
    {
        /// <summary>
        /// Get/sets the post information.
        /// </summary>
        public PostInfo Post { get; set; }

        /// <summary>
        /// Gets/sets the full post body.
        /// </summary>
        public string Body { get; set; }
    }
}