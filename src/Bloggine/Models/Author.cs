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
    /// The author information.
    /// </summary>
    public sealed class Author
    {
        /// <summary>
        /// Gets/sets the author name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets/sets the website URL.
        /// </summary>
        public string Website { get; set; }
    }
}