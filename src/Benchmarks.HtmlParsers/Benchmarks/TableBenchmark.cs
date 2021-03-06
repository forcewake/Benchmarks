using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using BenchmarkDotNet;
using BenchmarkDotNet.Tasks;
using Benchmarks.HtmlParsers.Benchmarks.Base;
using CsQuery;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace Benchmarks.HtmlParsers.Benchmarks
{
    [BenchmarkTask]
    public class TableBenchmark : HtmlBenchmarkBase
    {
        protected override string ResourcePath
        {
            get { return "Benchmarks.HtmlParsers.Examples.02.Table.html"; }
        }

        #region Benchmarks

        /// <summary>
        /// Extract exchange currency table using HtmlAgilityPack
        /// </summary>
        [Benchmark]
        public List<BranchBankCurrency> HtmlAgilityPack()
        {
            string currentBankName = null;
            var currencies = new List<BranchBankCurrency>();
            var rateFactory = new RateFactory();
            var descriptionFactory = new BranchBankDescriptionFactory();
            var currencyFactory = new BranchBankCurrencyFactory();

            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(Html);

            foreach (HtmlNode row in htmlSnippet.DocumentNode.SelectNodes("//table[@id='curr_table']/tbody/tr"))
            {
                if (!row.GetAttributeValue("class", string.Empty).Contains("tablesorter-childRow"))
                {
                    var cellNodes = row.SelectNodes("td");
                    if (cellNodes != null)
                    {
                        var currentBankCell = row.SelectNodes("td").Skip(1).First();
                        currentBankName = currentBankCell.InnerText;
                    }

                    continue;
                }


                HtmlNodeCollection cells = row.SelectNodes("td");

                var cellsText = cells.Select(x => x.InnerText).ToArray();

                var description = descriptionFactory.GetDescription(cellsText.ElementAt(0));

                var rates = rateFactory.CreateRatesFromRawData(cellsText.Skip(1).ToArray());

                var currency = currencyFactory.GetBranchBankCurrency(currentBankName, description, rates);

                currencies.Add(currency);
            }

            return currencies;
        }

        /// <summary>
        /// Extract exchange currency table using Fizzler
        /// </summary>
        [Benchmark]
        public List<BranchBankCurrency> Fizzler()
        {
            string currentBankName = null;
            var currencies = new List<BranchBankCurrency>();
            var rateFactory = new RateFactory();
            var descriptionFactory = new BranchBankDescriptionFactory();
            var currencyFactory = new BranchBankCurrencyFactory();

            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(Html);

            foreach (HtmlNode row in htmlSnippet.DocumentNode.QuerySelector("table").QuerySelectorAll("tr").Skip(1))
            {
                if (!row.GetAttributeValue("class", string.Empty).Contains("tablesorter-childRow"))
                {
                    var cellNodes = row.SelectNodes("td");
                    if (cellNodes != null)
                    {
                        var currentBankCell = row.SelectNodes("td").Skip(1).First();
                        currentBankName = currentBankCell.InnerText;
                    }
                    
                    continue;
                }

                HtmlNodeCollection cells = row.SelectNodes("td");

                var cellsText = cells.Select(x => x.InnerText).ToArray();

                var description = descriptionFactory.GetDescription(cellsText.ElementAt(0));

                var rates = rateFactory.CreateRatesFromRawData(cellsText.Skip(1).ToArray());

                var currency = currencyFactory.GetBranchBankCurrency(currentBankName, description, rates);

                currencies.Add(currency);
            }

            return currencies;
        }

        /// <summary>
        /// Extract exchange currency table using CsQuery
        /// </summary>
        [Benchmark]
        public List<BranchBankCurrency> CsQuery()
        {
            string currentBankName = null;
            var currencies = new List<BranchBankCurrency>();
            var rateFactory = new RateFactory();
            var descriptionFactory = new BranchBankDescriptionFactory();
            var currencyFactory = new BranchBankCurrencyFactory();

            CQ doc = CQ.Create(Html);
            var currencyTable = doc["#curr_table"];

            foreach (var row in currencyTable.Find("tr").Skip(1))
            {
                if (!row.Cq().HasClass("tablesorter-childRow"))
                {
                    var bankCells = row.Cq().Find("td").ToList();
                    if (bankCells.Any())
                    {
                        var currentBankCell = bankCells.Skip(1).First();
                        currentBankName = currentBankCell.Cq().Text();
                        continue;
                    }
                }

                var cellsText = row.Cq().Find("td").Select(td => td.Cq().Text()).ToArray();

                if (cellsText.Any())
                {
                    var description = descriptionFactory.GetDescription(cellsText.ElementAt(0));

                    var rates = rateFactory.CreateRatesFromRawData(cellsText.Skip(1).ToArray());

                    var currency = currencyFactory.GetBranchBankCurrency(currentBankName, description, rates);

                    currencies.Add(currency);
                }
            }

            return currencies;
        }

        /// <summary>
        /// Extract exchange currency table using AngleSharp
        /// </summary>
        [Benchmark]
        public List<BranchBankCurrency> AngleSharp()
        {
            string currentBankName = null;
            var currencies = new List<BranchBankCurrency>();
            var rateFactory = new RateFactory();
            var descriptionFactory = new BranchBankDescriptionFactory();
            var currencyFactory = new BranchBankCurrencyFactory();

            var parser = new HtmlParser();
            var document = parser.Parse(Html);
            var currencyTable = document.QuerySelector<IHtmlTableElement>("table");

            foreach (var row in currencyTable.Rows.Skip(1))
            {
                if (!row.ClassList.Contains("tablesorter-childRow"))
                {
                    var currentBankCell = row.Cells[1];
                    currentBankName = currentBankCell.Text();
                    continue;
                }

                var cellsText = row.Cells.Select(x => x.Text()).ToArray();

                var description = descriptionFactory.GetDescription(cellsText.ElementAt(0));

                var rates = rateFactory.CreateRatesFromRawData(cellsText.Skip(1).ToArray());

                var currency = currencyFactory.GetBranchBankCurrency(currentBankName, description, rates);

                currencies.Add(currency);
            }

            return currencies;
        }

        /// <summary>
        /// Extract exchange currency table using AngleSharp
        /// </summary>
        [Benchmark]
        public List<BranchBankCurrency> Regex()
        {
            string currentBankName = null;
            var currencies = new List<BranchBankCurrency>();
            var rateFactory = new RateFactory();
            var descriptionFactory = new BranchBankDescriptionFactory();
            var currencyFactory = new BranchBankCurrencyFactory();


            Regex rowRegex = new Regex("<tr[^>]*?>(?<rowContent>((?!</tr>).)*)</tr>",
                RegexOptions.Compiled | RegexOptions.Singleline);
            Regex cellRegex = new Regex("<td[^>]*?>(?<cell>((?!</td>).)*)</td>",
                RegexOptions.Compiled | RegexOptions.Singleline);
            Regex attributeRegex = new Regex("(\\S+)=[\"']?((?:.(?![\"']?\\s+(?:\\S+)=|[>\"']))+.)[\"']?",
                RegexOptions.Compiled | RegexOptions.Singleline);
            Regex cleanTagRegex = new Regex("\\<[^\\>]*\\>", 
                RegexOptions.Compiled | RegexOptions.Singleline);

            foreach (var row in rowRegex.Matches(Html).Cast<Match>().Select(match => new
            {
                InnerHtml = WebUtility.HtmlDecode(match.Groups["rowContent"].ToString()),
                Content = WebUtility.HtmlDecode(match.ToString())
            }).Skip(1))
            {
                var cells = cellRegex.Matches(row.InnerHtml)
                    .Cast<Match>()
                    .Select(match => WebUtility.HtmlDecode(match.Groups["cell"].ToString()))
                    .ToList();

                var attributes = attributeRegex.Matches(row.Content)
                    .Cast<Match>()
                    .FirstOrDefault(m => m.ToString().Contains("tablesorter-childRow"));

                if (cells.Any(cell => !string.IsNullOrWhiteSpace(cell)) && cells.Count > 2)
                {
                    if (attributes == null)
                    {
                        currentBankName = cleanTagRegex.Replace(cells[1], string.Empty);
                        continue;
                    }

                    var rawDescription = cleanTagRegex.Replace(cells.ElementAt(0), string.Empty);

                    var description = descriptionFactory.GetDescription(rawDescription);

                    var rates = rateFactory.CreateRatesFromRawData(cells.Skip(1).ToArray());

                    var currency = currencyFactory.GetBranchBankCurrency(currentBankName, description, rates);

                    currencies.Add(currency);
                }
            }

            return currencies;
        }

        #endregion

        #region Infrastructure

        public class RateFactory
        {
            public Rate CreateRate(string rate, string exchangeCode, ExchangeDirection direction)
            {
                return new Rate
                {
                    CurrencyRate = double.Parse(rate),
                    ExchangeCode = exchangeCode,
                    Direction = direction
                };
            }

            public Rate CreateUsdBuyRate(string rate)
            {
                return CreateRate(rate, "USD", ExchangeDirection.Buy);
            }

            public Rate CreateUsdSellRate(string rate)
            {
                return CreateRate(rate, "USD", ExchangeDirection.Sell);
            }

            public Rate CreateEurBuyRate(string rate)
            {
                return CreateRate(rate, "EUR", ExchangeDirection.Buy);
            }

            public Rate CreateEurSellRate(string rate)
            {
                return CreateRate(rate, "EUR", ExchangeDirection.Sell);
            }

            public Rate CreateRubBuyRate(string rate)
            {
                return CreateRate(rate, "RUB", ExchangeDirection.Buy);
            }

            public Rate CreateRubSellRate(string rate)
            {
                return CreateRate(rate, "RUB", ExchangeDirection.Sell);
            }

            public IEnumerable<Rate> CreateRatesFromRawData(string[] rawRates)
            {
                var rates = new List<Rate>
                {
                    CreateUsdBuyRate(rawRates[0]),
                    CreateUsdSellRate(rawRates[1]),
                    CreateEurBuyRate(rawRates[2]),
                    CreateEurSellRate(rawRates[3]),
                    CreateRubBuyRate(rawRates[4]),
                    CreateRubSellRate(rawRates[5])
                };

                return rates;
            }
        }

        public class BranchBankDescriptionFactory
        {
            public BranchBankDescription GetDescription(string address)
            {
                var addressParts = address.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

                return new BranchBankDescription
                {
                    Name = addressParts[0].Trim(' ', '-'),
                    FullAddress = string.Join(string.Empty, addressParts.Skip(1)).Trim(),
                };
            }
        }

        public class BranchBankCurrencyFactory
        {
            public BranchBankCurrency GetBranchBankCurrency(string bankName, BranchBankDescription description, IEnumerable<Rate> rates)
            {
                return new BranchBankCurrency
                {
                    Bank = bankName,
                    Name = description.Name,
                    FullAddress = description.FullAddress,
                    Rates = rates
                };
            }
        }

        #endregion

        #region Models

        public class BranchBankDescription
        {
            public string Name { get; set; }
            public string FullAddress { get; set; }
        }

        public class Rate
        {
            public ExchangeDirection Direction { get; set; }
            public string ExchangeCode { get; set; }
            public double CurrencyRate { get; set; }
        }

        public class BranchBankCurrency
        {
            public string Bank { get; set; }

            public string Name { get; set; }

            public string FullAddress { get; set; }

            public IEnumerable<Rate> Rates { get; set; }
        }

        public enum ExchangeDirection
        {
            Sell,
            Buy
        }

        #endregion
    }
}