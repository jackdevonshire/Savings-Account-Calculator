using SavingsCalculator.Types;

namespace SavingsCalculator.Helpers
{
    public class InterestHelper
    {
            
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