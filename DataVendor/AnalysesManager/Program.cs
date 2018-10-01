﻿using AnalysesManager.Controllers;
using System;

namespace AnalysesManager
{
    class Program
    {
        static void Main(string[] args)
        {
            new Controller().GenerateAnalyses();

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
