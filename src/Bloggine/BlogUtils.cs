/*
 * Copyright (c) 2021 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/bloggine
 *
 */

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Bloggine
{
    /// <summary>
    /// Internal help methods.
    /// </summary>
    internal static class BlogUtils
    {
        /// <summary>
        /// Generates a ETag from the given name and date.
        /// </summary>
        /// <param name="name">The resource name</param>
        /// <param name="date">The modification date</param>
        /// <returns>The etag</returns>
        public static string GenerateETag(string name, DateTime date)
        {
            var encoding = new UTF8Encoding();

            using var crypto = SHA256.Create();

            var str = name + date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var bytes = crypto.ComputeHash(encoding.GetBytes(str));
            return $"\"{Convert.ToBase64String(bytes)}\"";
        }

        /// <summary>
        /// Generates a slug from the given string.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The slug</returns>
        public static string GenerateSlug(string str)
        {
            // Trim & make lower case
            var slug = str.Trim().ToLowerInvariant();

            // Convert culture specific characters
            slug = slug
                .Replace("å", "a", StringComparison.InvariantCulture)
                .Replace("ä", "a", StringComparison.InvariantCulture)
                .Replace("á", "a", StringComparison.InvariantCulture)
                .Replace("à", "a", StringComparison.InvariantCulture)
                .Replace("ö", "o", StringComparison.InvariantCulture)
                .Replace("ó", "o", StringComparison.InvariantCulture)
                .Replace("ò", "o", StringComparison.InvariantCulture)
                .Replace("é", "e", StringComparison.InvariantCulture)
                .Replace("è", "e", StringComparison.InvariantCulture)
                .Replace("í", "i", StringComparison.InvariantCulture)
                .Replace("ì", "i", StringComparison.InvariantCulture)
                .Replace("ž", "z", StringComparison.InvariantCulture)
                .Replace("š", "s", StringComparison.InvariantCulture)
                .Replace("č", "c", StringComparison.InvariantCulture);

            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z\u0600-\u06FF0-9-/ ]", "")
                .Replace("--", "-", StringComparison.InvariantCulture);

            // Remove whitespaces
            slug = Regex.Replace(slug.Replace("-", " ", StringComparison.InvariantCulture), @"\s+", " ")
                .Replace(" ", "-", StringComparison.InvariantCulture);

            // Remove slashes
            slug = slug.Replace("/", "-", StringComparison.InvariantCulture);

            // Remove multiple dashes
            slug = Regex.Replace(slug, @"[-]+", "-");

            // Remove leading & trailing dashes
            if (slug.EndsWith("-", StringComparison.InvariantCulture))
            {
                slug = slug[..slug.LastIndexOf("-", StringComparison.InvariantCulture)];
            }
            if (slug.StartsWith("-", StringComparison.InvariantCulture))
            {
                slug = slug[Math.Min(slug.IndexOf("-", StringComparison.InvariantCulture) + 1, slug.Length)..];
            }
            return slug;
        }
    }
}