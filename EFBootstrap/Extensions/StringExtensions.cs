// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Encapsulates a series of time saving extension methods to <see cref="T:System.String">String</see>s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Extensions
{
    #region Using
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text; 
    #endregion

    /// <summary>
    /// Encapsulates a series of time saving extension methods to <see cref="T:System.String">String</see>s.
    /// </summary>
    public static class StringExtensions
    {
        #region Cryptography
        /// <summary>
        /// Creates an MD5 fingerprint of the String.
        /// </summary>
        /// <param name="expression">The <see cref="T:System.String">String</see> instance that this method extends.</param>
        /// <returns>An MD5 fingerprint of the String.</returns>
        public static string ToMD5Fingerprint(this string expression)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(expression.ToCharArray());

            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] hash = md5.ComputeHash(bytes);

                // Concatenate the hash bytes into one long String.
                return hash.Aggregate(
                    new StringBuilder(32),
                    (sb, b) => sb.Append(b.ToString("X2", CultureInfo.InvariantCulture)))
                    .ToString();
            }
        }
        #endregion
    }
}
