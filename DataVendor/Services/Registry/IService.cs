﻿namespace Services.Registry
{
    public interface IService
    {
        /// <summary>
        /// Updates the registry (removes the unused, and enters the new ones based on the market data).
        /// </summary>
        void Update();
    }
}