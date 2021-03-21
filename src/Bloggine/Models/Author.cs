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

        /// <summary>
        /// Gets/sets the image URL.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets if the user has an image.
        /// </summary>
        public bool HasImage => !string.IsNullOrWhiteSpace(Image);
    }
}