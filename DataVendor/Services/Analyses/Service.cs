﻿using Infrastructure;
using NLog;
using Peter.Models.Builders;
using Peter.Models.Enums;
using Peter.Models.Interfaces;
using Peter.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Services.Analyses
{
    public class Service : IService
    {
        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IConfigReader _configReader;
        private readonly IAnalysesRepository _analysesCsvFileRepository;
        private readonly IMarketDataRepository _marketDataRepository;
        private readonly IRegistryRepository _registryRepository;

        private readonly IFundamentalAnalyser _fundamentalAnalyser;
        private readonly ITechnicalAnalyser _technicalAnalyser;

        private readonly int _fastMovingAverage;
        private readonly int _slowMovingAverage;
        private readonly int _buyingPacketInEuro;

        public Service(
            IAnalysesRepository analysesRepository,
            IMarketDataRepository marketDataRepository,
            IRegistryRepository registryRepository,
            IConfigReader config)
        {
            try
            {
                _analysesCsvFileRepository = analysesRepository;
                _marketDataRepository = marketDataRepository;
                _registryRepository = registryRepository;

                _fundamentalAnalyser = new FundamentalAnalyser();
                _technicalAnalyser = new TechnicalAnalyser();

                _configReader = config;

                _buyingPacketInEuro = _configReader.Settings.BuyingPacketInEuro;
                _fastMovingAverage = _configReader.Settings.FastMovingAverage;
                _slowMovingAverage = _configReader.Settings.SlowMovingAverage;

                _logger.Debug($"Buying Packet is {_buyingPacketInEuro} EUR from config file.");
                _logger.Debug($"Fast Moving Average subset size is {_fastMovingAverage} from config file.");
                _logger.Debug($"Slow Moving Average subset size is {_slowMovingAverage} from config file.");

                if (_fastMovingAverage >= _slowMovingAverage)
                {
                    throw new ServiceException("The timespan for the fast moving average must be lower than of the slow moving average.");
                }
                if (_buyingPacketInEuro <= 0 || _fastMovingAverage <= 0 || _slowMovingAverage <= 0)
                {
                    throw new ServiceException("Buying packet and moving average subset sizes must be positive numbers.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new ServiceException($"Error when initializing {GetType().Name}.", ex);
            }
        }

        public void GenerateAnalyses()
        {
            _logger.Info("Generating analyses ...");

            var marketData = _marketDataRepository.GetAll().ToList();
            if (ContainsDataWithoutIsin(marketData))
            {
                throw new ServiceException("There are marketdata without ISIN. No analysis generated.");
            }

            _logger.Info("Removing discontinued market data rows.");
            var latestDate = marketData.Max(d => d.DateTime).Date;
            RemoveEntriesWithoutUptodateData(marketData, latestDate);
            _logger.Info($"Having discontinued market data rows removed {marketData.Count} data entries remained.");

            var groupedMarketData = from data in marketData
                                    group data by data.Isin into dataByIsin
                                    select dataByIsin
                                        .OrderByDescending(d => d.DateTime)
                                        .Take(_slowMovingAverage);

            var analyses = groupedMarketData.Select(GetAnalysis).ToImmutableList();

            if (!analyses.Any())
            {
                _logger.Info("No analyses generated.");
                return;
            }

            _logger.Info($"{analyses.Count} analyses generated.");

            _logger.Debug("Adding analyses to repository ...");
            _analysesCsvFileRepository.AddRange(analyses);
            _logger.Debug("Analyses added.");

            _logger.Debug("Saving analyses to repository ...");
            _analysesCsvFileRepository.SaveChanges();
            _logger.Debug("Analyses saved.");

            _logger.Info("*** *** ***");
        }

        private KeyValuePair<string, IAnalysis> GetAnalysis(IEnumerable<IMarketDataEntity> marketDataInput)
        {
            if (marketDataInput == null)
                throw new ArgumentNullException(nameof(marketDataInput));

            try
            {
                if (!marketDataInput.Any())
                    throw new ArgumentException("Market data set cannot be empty", nameof(marketDataInput));

                var marketData = marketDataInput.ToImmutableArray();

                var isin = marketData.First().Isin;
                if (string.IsNullOrEmpty(isin))
                    throw new ServiceException("No ISIN found in market data set.");

                var closingPrice = marketData.First().ClosingPrice;

                var stockBaseData = _registryRepository.GetById(isin);
                if (stockBaseData is null)
                    throw new ServiceException($"No registry entry found for {isin}");

                var fundamentalAnalysis = _fundamentalAnalyser.GetAnalysis(closingPrice, stockBaseData);
                if (fundamentalAnalysis is null)
                    throw new ServiceException($"No fundamental analysis can be created for {isin}");

                var technicalAnalysis = _technicalAnalyser.GetAnalysis(marketData, _fastMovingAverage, _slowMovingAverage);
                if (technicalAnalysis is null)
                    throw new ServiceException($"No technical analysis can be created for {isin}");

                var analysis = new AnalysisBuilder()
                    .SetClosingPrice(closingPrice)
                    .SetName(stockBaseData.Name)
                    .SetQtyInBuyingPacket((int)Math.Floor(_buyingPacketInEuro / closingPrice))
                    .SetFundamentalAnalysis(fundamentalAnalysis)
                    .SetTechnicalAnalysis(technicalAnalysis)
                    .Build();

                if (analysis is null)
                    throw new ServiceException($"No analysis can be created for {isin}");

                return new KeyValuePair<string, IAnalysis>(isin, analysis);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"No analysis can be created.", ex);
            }
        }

        internal static bool ContainsDataWithoutIsin(List<IMarketDataEntity> marketData) =>
            marketData.Any(d => string.IsNullOrWhiteSpace(d.Isin));

        // TODO investigate
        internal static void RemoveEntriesWithoutUptodateData(
            List<IMarketDataEntity> marketData,
            DateTime latestDate) =>
                marketData.RemoveAll(d => marketData.Where(d2 => string.Equals(d.Isin, d2.Isin)).Max(d3 => d3.DateTime).Date < latestDate);
    }
}
