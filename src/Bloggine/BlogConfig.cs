/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui
 *
 */

namespace Bloggine
{
    /// <summary>
    /// The different configuration options available.
    /// </summary>
    public class BlogConfig
    {
        /// <summary>
        /// Gets/sets the global max-age in seconds. The default
        /// value is one day, i.e 86400.
        /// </summary>
        public int CacheMaxAge { get; set; } = 86400;

        /// <summary>
        /// Gets/sets the data path. The default value is Data.
        /// </summary>
        public string DataPath { get; set; } = "Data";

        /// <summary>
        /// Gets/sets the data asset path. The default value is Uploads.
        /// </summary>
        public string DataAssetPath { get; set; } = "Uploads";

        /// <summary>
        /// Gets/sets the selected theme.
        /// </summary>
        public string Theme { get; set; } = "Default";

        /// <summary>
        /// Gets/sets the blog title.
        /// </summary>
        public string Title { get; set; } = "Bloggine";

        /// <summary>
        /// Gets/sets the blog headline.
        /// </summary>
        public string Headline { get; set; } = "Just another markdown blog";

        /// <summary>
        /// Gets/sets if runtime compilation of razor pages should
        /// be used. The default value is false.
        /// </summary>
        public bool UseRazorRuntimeCompilation { get; set; } = false;
    }
}