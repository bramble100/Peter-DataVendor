﻿using Infrastructure;
using NLog;
using Peter.Models.Interfaces;
using Peter.Repositories.Interfaces;
using Services.DataVendor.Html;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Services.DataVendor
{
    public class WebService : IWebService
    {
        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IEnvironmentVariableReader _environmentVariableReader;
        private readonly IHttpFacade _httpFacade;
        private readonly IMarketDataRepository _marketDataCsvFileRepository;

        public WebService(
            IEnvironmentVariableReader environmentVariableReader,
            IHttpFacade httpFacade,
            IMarketDataRepository marketDataRepository)
        {
            _environmentVariableReader = environmentVariableReader
                ?? throw new ArgumentNullException(nameof(environmentVariableReader));
            _httpFacade = httpFacade
                ?? throw new ArgumentNullException(nameof(httpFacade));
            _marketDataCsvFileRepository = marketDataRepository
                ?? throw new ArgumentNullException(nameof(marketDataRepository));
        }

        public async Task<IEnumerable<IMarketDataEntity>> GetDownloadedDataFromWeb()
        {
            _logger.Info("Downloading market data ...");

            var htmlStrings = new Dictionary<string, string>();
            string htmlContent;
            var entities = new List<IMarketDataEntity>();

            using (var client = _httpFacade.GetHttpClient())
            {
                foreach (var link in Links)
                {
                    LogManager.GetCurrentClassLogger().Info($"Downloading: {link.Key}");
                    htmlContent = await client.GetStringAsync(link.Value);
                    var foundEntities = HtmlProcessor.GetMarketDataEntities(htmlContent, link.Key).ToImmutableArray();
                    _logger.Info($"{link.Key}: {foundEntities.Count()}");
                    entities.AddRange(foundEntities);
                }
            }

            _logger.Info($"{(entities.Any() ? entities.Count.ToString() : "No")} market data entity downloaded.");

            return entities;
        }

        public void Update(IEnumerable<IMarketDataEntity> latestData)
        {
            _logger.Info("Updating and saving market data ...");

            var entities = latestData.ToImmutableList();

            if (entities.Any())
            {
                _marketDataCsvFileRepository.AddRange(entities);
                _logger.Info("Market data updated.");

                _marketDataCsvFileRepository.SaveChanges();
                _logger.Info("Market data saved.");
            }
            else
            {
                _logger.Info("No market data to add.");
            }
        }

        private Dictionary<string, Uri> Links => _environmentVariableReader
            .GetEnvironmentVariable("PeterDataVendorLinks")
            .Split(';')
            .Select(item => GetUriKeyValuePair(item))
            .ToDictionary(item => item.Key, item => item.Value);

        private static KeyValuePair<string, Uri> GetUriKeyValuePair(string input)
        {
            var delimiterPosition = input.IndexOf('=');
            return new KeyValuePair<string, Uri>(
                input.Substring(0, delimiterPosition), 
                new Uri(input.Substring(delimiterPosition + 1)));
        }
    }
}