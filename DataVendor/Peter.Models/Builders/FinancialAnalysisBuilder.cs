﻿using Peter.Models.Implementations;
using Peter.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace Peter.Models.Builders
{
    public class FinancialAnalysisBuilder : IBuilder<IFinancialAnalysis>
    {
        private static HashSet<int> ValidMonthsInReportValues = new HashSet<int>() { 3, 6, 9, 12 };

        private bool _closingPriceSet;
        private bool _EPSset;
        private bool _monthsInReportSet;

        private decimal _closingPrice;
        private decimal _eps;
        private int _monthsInReport;

        public FinancialAnalysisBuilder()
        {
        }

        public FinancialAnalysisBuilder SetClosingPrice(decimal value)
        {
            if(value > 0)
            {
                _closingPrice = value;
                _closingPriceSet = true;
            }

            return this;
        }

        public FinancialAnalysisBuilder SetEPS(decimal? value)
        {
            if (value.HasValue && value.Value != 0)
            {
                _eps = value.Value;
                _EPSset = true;
            }
            return this;
        }

        public FinancialAnalysisBuilder SetMonthsInReport(int? value)
        {
            if (value.HasValue && ValidMonthsInReportValues.Contains(value.Value))
            {
                _monthsInReport = value.Value;
                _monthsInReportSet = true;
            }
            return this;
        }

        public IFinancialAnalysis Build()
        {
            if (!_closingPriceSet || !_EPSset || !_monthsInReportSet)
            {
                return null;
            }

            return new FinancialAnalysis()
            {
                PE = Math.Round(_closingPrice * _monthsInReport / _eps / 12, 2)
            };
        }
    }
}