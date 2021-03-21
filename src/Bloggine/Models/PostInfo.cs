/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

using System;
using System.Collections.Generic;

namespace Bloggine.Models
{
    /// <summary>
    /// The post model used in lists and archives.
    /// </summary>
    public class PostInfo
    {
        /// <summary>
        /// Get/set the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets/sets the unique slug.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets/sets the optional primary image.
        /// </summary>
        public string PrimaryImage { get; set; }

        /// <summary>
        /// Gets/sets the optional excerpt.
        /// </summary>
        public string Excerpt { get; set; }

        /// <summary>
        /// Gets/sets the optional category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets/sets the available tags.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets/sets the published date.
        /// </summary>
        public DateTime Published { get; set; }

        /// <summary>
        /// Gets/sets the last modification date.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets if the post has an category available.
        /// </summary>
        public bool HasCategory => !string.IsNullOrWhiteSpace(Category);

        /// <summary>
        /// Gets if the post has an excerpt available.
        /// </summary>
        public bool HasExcerpt => !string.IsNullOrWhiteSpace(Excerpt);

        /// <summary>
        /// Gets if the post has a primary image available.
        /// </summary>
        public bool HasPrimaryImage => !string.IsNullOrWhiteSpace(PrimaryImage);

        /// <summary>
        /// Gets if the post has an any tags available.
        /// </summary>
        public bool HasTags => Tags.Count > 0;

        /// <summary>
        /// Gets the additional post settings.
        /// </summary>
        /// <value></value>
        public PostSettings Settings { get; internal set; } = new PostSettings();
    }
}