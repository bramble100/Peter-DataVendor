﻿namespace Infrastructure
{
    /// <summary>
    /// Provides access to configuration settings from config file.
    /// </summary>
    public interface IConfigReader
    {
        /// <summary>
        /// Provides configuration settings.
        /// </summary>
        ConfigReader.ConfigSettings Settings { get; }
    }
}
