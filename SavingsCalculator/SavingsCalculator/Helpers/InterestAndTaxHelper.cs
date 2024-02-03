using SavingsCalculator.Types;

namespace SavingsCalculator.Helpers
{
    public class InterestAndTaxHelper
    {
        public static TaxYear GetTaxYearForDate(DateOnly date)
        {
            /*
             * Tax years run differently to normal years, and are typically run from the 6th April from one year, to the 5th April the next
             * God knows why... https://www.gov.uk/self-assessment-tax-returns
             *
             * When depositing and withdrawing, we need to know that the total ingoing / outgoing balance for the year will NOT exceed £4000
             * As per the rules for Lifetime ISA's https://www.gov.uk/lifetime-isa This does NOT include interest gained with the current provider,
             * It also does NOT include the 25% bonus the government provides at the end of each tax year
             */
        
            // Get tax year for the current transaction year
            DateOnly startOfTaxYear = new DateOnly(date.Year, 4, 6);
            DateOnly endOfTaxYear = new DateOnly(date.Year + 1, 4, 5);

            // If the transaction is before the start of the current tax year (6th April), then adjust accordingly
            if (date < startOfTaxYear)
            {
                startOfTaxYear = new DateOnly(date.Year - 1, 4, 6);
                endOfTaxYear = new DateOnly(date.Year, 4, 5);
            }

            return new TaxYear
            {
                StartOfTaxYear = startOfTaxYear,
                EndOfTaxYear = endOfTaxYear
            };
        }
        
        public static double GetActualInterestRate(InterestPaidType interestPaidType, double aerAsPercentage)
        {
            switch (interestPaidType)
            {
                case InterestPaidType.Daily:
                    return (aerAsPercentage / 100) / 365;
                case InterestPaidType.Monthly:
                    return (aerAsPercentage / 100) / 12;
                case InterestPaidType.Annually:
                    return (aerAsPercentage / 100) / 1;
                default:
                    return 0;
            }
        }

        public static bool CompoundInterestToday(CompoundType compoundType, DateOnly dateInterestLastCompounded, DateOnly currentDate)
        {
            switch (compoundType)
            {
                case CompoundType.Daily:
                    return dateInterestLastCompounded.AddDays(1) == currentDate;
                case CompoundType.Monthly:
                    return dateInterestLastCompounded.AddMonths(1) == currentDate;
                case CompoundType.Annually:
                    return dateInterestLastCompounded.AddYears(1) == currentDate;
                default:
                    return false;
            }
        }
        
        public static bool PayInterestToday(InterestPaidType interestPaidType, DateOnly dateLastInterestPaid, DateOnly currentDate)
        {
            switch (interestPaidType)
            {
                case InterestPaidType.Daily:
                    return true;
                case InterestPaidType.Monthly:
                    return dateLastInterestPaid.AddMonths(1) == currentDate;
                case InterestPaidType.Annually:
                    return dateLastInterestPaid.AddYears(1) == currentDate;
                default:
                    return false;
            }
        }
    }
}