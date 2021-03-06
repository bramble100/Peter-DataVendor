﻿namespace Services.DataVendor
{
    /// <summary>
    /// Adds the missing ISIN data to market data entries without ISIN.
    /// </summary>
    public interface IIsinAdderService
    {
        /// <summary>
        /// Adds the missing ISIN data to market data entries without ISIN.
        /// </summary>
        void AddIsinsToMarketData();
    }
}