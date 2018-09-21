﻿using FluentAssertions;
using NUnit.Framework;
using Peter.Models.Validators;

namespace Models.UnitTests
{
    [TestFixture]
    public class MarketDataEntityValidator
    {
        private readonly string _validString = "12,34";
        private readonly decimal _validDecimal = 12.34m;
        private readonly string _zero = "0";
        private readonly string _negative = "-1,7";
        private readonly string _empty = string.Empty;

        [Test]
        public void TryParseClosingPrice_WithValid_ShouldReturnTrue()
        {
            MarketDataEntity
                .TryParseClosingPrice(_validString, out var result)
                .Should()
                .BeTrue();

            result.Should().Be(_validDecimal);
        }

        [Test]
        public void TryParseClosingPrice_WithEmpty_ShouldReturnFalse()
        {
            MarketDataEntity
                .TryParseClosingPrice(_empty, out var result)
                .Should()
                .BeFalse();
        }

        [Test]
        public void TryParseClosingPrice_WithZero_ShouldReturnFalse()
        {
            MarketDataEntity
                .TryParseClosingPrice(_zero, out var result)
                .Should()
                .BeFalse();
        }

        [Test]
        public void TryParseClosingPrice_WithNegative_ShouldReturnFalse()
        {
            MarketDataEntity
                .TryParseClosingPrice(_negative, out var result)
                .Should()
                .BeFalse();
        }
    }
}