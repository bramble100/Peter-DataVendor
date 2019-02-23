﻿using System;
using Peter.Models.Enums;
using Peter.Models.Implementations;
using Peter.Models.Interfaces;

namespace Peter.Models.Builders
{
    public class RegistryEntryBuilder : IBuilder<IRegistryEntry>
    {
        private bool _isinIsSet;
        private bool _nameIsSet;

        private string _isin;
        private Uri _stockExchangeLink;
        private Uri _ownInvestorLink;
        private string _name;
        private Position _position;
        private IFinancialAnalysis _financialAnalysis;
        private IFinancialReport _financialReport;

        public RegistryEntryBuilder SetIsin(string value)
        {
            if (Validators.Isin.IsValid(value))
            {
                _isin = value;
                _isinIsSet = true;
            }

            return this;
        }

        public RegistryEntryBuilder SetName(string value)
        {
            if (Validators.RegistryEntry.TryParseName(value, out var name))
            {
                _name = name;
                _nameIsSet = true;
            }

            return this;
        }

        public RegistryEntryBuilder SetOwnInvestorLink(string value)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                _ownInvestorLink = new Uri(value);

            return this;
        }

        public RegistryEntryBuilder SetStockExchangeLink(string value)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                _stockExchangeLink = new Uri(value);

            return this;
        }

        public RegistryEntryBuilder SetPosition(string value)
        {
            if (Enum.TryParse<Position>(value, true, out var result))
                _position = result;

            return this;
        }

        public RegistryEntryBuilder SetFinancialAnalysis(IFinancialAnalysis financialAnalysis)
        {
            _financialAnalysis = financialAnalysis;

            return this;
        }

        public RegistryEntryBuilder SetFinancialReport(IFinancialReport financialReport)
        {
            _financialReport = financialReport;

            return this;
        }

        public IRegistryEntry Build() => (_isinIsSet && _nameIsSet)
                ? new RegistryEntry()
                {
                    Isin = _isin,
                    Name = _name,
                    OwnInvestorLink = _ownInvestorLink,
                    StockExchangeLink = _stockExchangeLink,
                    Position = _position,
                    FinancialAnalysis = _financialAnalysis,
                    FinancialReport = _financialReport
                }
                : null;
    }
}