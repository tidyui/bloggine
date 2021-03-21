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

            using (var crypto = MD5.Create())
            {
                var str = name + date.ToString("yyyy-MM-dd HH:mm:ss");
                var bytes = crypto.ComputeHash(encoding.GetBytes(str));
                return $"\"{Convert.ToBase64String(bytes)}\"";
            }
        }

        /// <summary>
        /// Generates a slug from the given string.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The slug</returns>
        public static string GenerateSlug(string str)
        {
            // Trim & make lower case
            var slug = str.Trim().ToLower();

            // Convert culture specific characters
            slug = slug
                .Replace("å", "a")
                .Replace("ä", "a")
                .Replace("á", "a")
                .Replace("à", "a")
                .Replace("ö", "o")
                .Replace("ó", "o")
                .Replace("ò", "o")
                .Replace("é", "e")
                .Replace("è", "e")
                .Replace("í", "i")
                .Replace("ì", "i")
                .Replace("ž", "z")
                .Replace("š", "s")
                .Replace("č", "c");

            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z\u0600-\u06FF0-9-/ ]", "").Replace("--", "-");

            // Remove whitespaces
            slug = Regex.Replace(slug.Replace("-", " "), @"\s+", " ").Replace(" ", "-");

            // Remove slashes
            slug = slug.Replace("/", "-");

            // Remove multiple dashes
            slug = Regex.Replace(slug, @"[-]+", "-");

            // Remove leading & trailing dashes
            if (slug.EndsWith("-"))
                slug = slug.Substring(0, slug.LastIndexOf("-"));
            if (slug.StartsWith("-"))
                slug = slug.Substring(Math.Min(slug.IndexOf("-") + 1, slug.Length));
            return slug;
        }
    }
}