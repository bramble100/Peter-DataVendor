﻿using DataVendor.Controllers.Interfaces;
using DataVendor.Services;
using NLog;
using System;

namespace DataVendor.Controllers.Implementations
{
    public class Controller : IController
    {
        protected readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        private IIsinAdderService _isinAdderService;
        private IWebService _webService;

        public Controller(
            IIsinAdderService isinAdderService, 
            IWebService webService)
        {
            _isinAdderService = isinAdderService;
            _webService = webService;
        }

        public void WebToCsv()
        {
            _logger.Info("*** Web To Csv ***");

            try
            {
                _webService.Update(_webService.DownloadFromWeb());
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            _logger.Info("*** *** ***");
        }

        public void AddIsins()
        {
            _logger.Info("*** Add Isins ***");

            try
            {
                _isinAdderService.AddIsinsToEntities();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            _logger.Info("*** *** ***");
        }
    }
}