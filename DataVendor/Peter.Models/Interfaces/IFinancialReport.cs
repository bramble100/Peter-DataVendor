﻿using System;

namespace Peter.Models.Interfaces
{
    public interface IFinancialReport
    {
        /// <summary>
        /// Earning Per Share.
        /// </summary>
        decimal EPS { get; }
        /// <summary>
        /// True if the report is outdated.
        /// </summary>
        bool IsOutdated { get; }
        /// <summary>
        /// The number of months covered by the financial report.
        /// </summary>
        int MonthsInReport { get; }
        /// <summary>
        /// The date on which the next financial report will be published.
        /// </summary>
        DateTime NextReportDate { get; }
    }
}