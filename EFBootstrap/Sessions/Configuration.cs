// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Allows for the retrieval of configuration settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Sessions
{
    using System.Configuration;

    /// <summary>
    /// Allows for the retrieval of configuration settings.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Gets the cache length in minutes.
        /// </summary>
        public static int CacheLength
        {
            get
            {
                // AppSettings are cached by default so we can query them without caching.
                int fallback;
                int.TryParse(ConfigurationManager.AppSettings["EFBootStrapCacheLength"], out fallback);

                return fallback;
            }
        }
    }
}
