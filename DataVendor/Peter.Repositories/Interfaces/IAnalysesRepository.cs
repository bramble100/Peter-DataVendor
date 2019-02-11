﻿using Peter.Models.Interfaces;
using System.Collections.Generic;

namespace Peter.Repositories.Interfaces
{
    /// <summary>
    /// Repository with analyses.
    /// </summary>
    public interface IAnalysesRepository
    {
        /// <summary>
        /// Adds an entity.
        /// </summary>
        /// <param name="analysis"></param>
        void Add(KeyValuePair<string, IAnalysis> analysis);

        /// <summary>
        /// Adds many entities.
        /// </summary>
        /// <param name="analysis"></param>
        void AddRange(IEnumerable<KeyValuePair<string, IAnalysis>> entities);
        /// <summary>
        /// Finds the analysis for the stock with the given ISIN.
        /// </summary>
        /// <param name="isin"></param>
        /// <returns></returns>
        IFinancialAnalysis Find(string isin);
        /// <summary>
        /// Returns a copy of the entities.
        /// Key: ISIN. 
        /// Value: the analysis.
        /// </summary>
        IDictionary<string, IAnalysis> GetAll();
        /// <summary>
        /// Saves the content of the repository.
        /// </summary>
        void SaveChanges();
    }
}
