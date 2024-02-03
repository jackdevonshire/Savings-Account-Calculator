using SavingsCalculator.Types;

namespace SavingsCalculatorTests.Helpers.InterestAndTaxHelper
{
    public class TaxYearTestCase
    {
        public DateOnly TransactionDate;
        public DateOnly ExpectedTaxYearStart;
        public DateOnly ExpectedTaxYearEnd;
        public TaxYear Result;

        public TaxYearTestCase()
        {
            Result = new TaxYear();
        }
    }
    
    [Parallelizable]
    public class GetCurrentTaxYearTests
    {
        private readonly List<TaxYearTestCase> _testCases;

        public GetCurrentTaxYearTests()
        {
            _testCases = new List<TaxYearTestCase>
            {
                new TaxYearTestCase
                {
                    TransactionDate = new DateOnly(2023, 01, 01),
                    ExpectedTaxYearStart = new DateOnly(2022, 04, 06),
                    ExpectedTaxYearEnd = new DateOnly(2023, 04, 05)
                },
                new TaxYearTestCase
                {
                    TransactionDate = new DateOnly(2023, 06, 01),
                    ExpectedTaxYearStart = new DateOnly(2023, 04, 06),
                    ExpectedTaxYearEnd = new DateOnly(2024, 04, 05)
                },
                new TaxYearTestCase
                {
                    TransactionDate = new DateOnly(2023, 04, 01),
                    ExpectedTaxYearStart = new DateOnly(2022, 04, 06),
                    ExpectedTaxYearEnd = new DateOnly(2023, 04, 05)
                }
            };
        }

        [SetUp]
        public void Setup()
        {
            foreach (var testCase in _testCases)
            {
                testCase.Result = SavingsCalculator.Helpers.InterestAndTaxHelper.GetTaxYearForDate(testCase.TransactionDate);
            }
        }

        [Test]
        public void ThenTheTaxYearIsCorrect()
        {
            foreach (var testCase in _testCases)
            {
                Assert.That(testCase.Result.StartOfTaxYear, Is.EqualTo(testCase.ExpectedTaxYearStart));
                Assert.That(testCase.Result.EndOfTaxYear, Is.EqualTo(testCase.ExpectedTaxYearEnd));
            }
        }
    }
}