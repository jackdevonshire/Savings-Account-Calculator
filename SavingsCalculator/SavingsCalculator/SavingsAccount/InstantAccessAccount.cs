using SavingsCalculator.Reports;
using SavingsCalculator.Types;

namespace SavingsCalculator.SavingsAccount;

public class InstantAccessAccount : BaseSavingsAccount
{
    private readonly double _annualEquivalentRateAsPercentage;
    private readonly InterestPaidType _interestPaidType;
    private readonly CompoundType _compoundType;
    private double _actualInterestRate;

    public InstantAccessAccount(
        string accountName, 
        DateOnly openingDate,
        double openingBalance, 
        double annualEquivalentRateAsPercentage,
        InterestPaidType interestPaidType = InterestPaidType.Monthly,
        CompoundType compoundType = CompoundType.Monthly
        ) : base(accountName, openingDate, openingBalance)
    {
        _annualEquivalentRateAsPercentage = annualEquivalentRateAsPercentage;
        _interestPaidType = interestPaidType;
        _compoundType = compoundType;
        _actualInterestRate = GetActualInterestRate();
    }

    private double GetActualInterestRate()
    {
        switch (_interestPaidType)
        {
            case InterestPaidType.Daily:
                return (_annualEquivalentRateAsPercentage / 100) / 365;
            case InterestPaidType.Monthly:
                return (_annualEquivalentRateAsPercentage / 100) / 12;
            case InterestPaidType.Annually:
                return (_annualEquivalentRateAsPercentage / 100) / 1;
            default:
                return 0;
        }
    }

    private bool CompoundInterestToday(DateOnly dateInterestLastCompounded, DateOnly currentDate)
    {
        switch (_compoundType)
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
    
    private bool PayInterestToday(DateOnly dateLastInterestPaid, DateOnly currentDate)
    {
        switch (_interestPaidType)
        {
            case InterestPaidType.Monthly:
                return dateLastInterestPaid.AddMonths(1) == currentDate;
            case InterestPaidType.Annually:
                return dateLastInterestPaid.AddYears(1) == currentDate;
            default:
                return false;
        }
    }

    protected override void CalculateFinance(DateOnly? dateTo)
    {
        // First clear current transaction log of interest and benefit payments, and order by date ascending
        Transactions = Transactions
            .Where(x => x.Type is TransactionType.Deposit or TransactionType.Withdraw)
            .OrderBy(x => x.Date)
            .ToList();
        
        // Exit if no transactions at all
        if (Transactions.Count == 0)
            return;
        
        // Now get transactions we want to loop through
        dateTo ??= Transactions.Last().Date;
        var transactionsToCalculate = Transactions;
        transactionsToCalculate = transactionsToCalculate.Where(x => x.Date <= dateTo.Value).ToList();
        
        // Exit if no transactions for selected period
        if (transactionsToCalculate.Count == 0)
            return;
        
        // Now initialise essential variables
        var openingDate = Transactions.First().Date;
        var currentDate = openingDate;
        var dateLastInterestPaid = openingDate;
        var dateInterestLastCompounded = openingDate;
        
        // Variables representing accounts balance
        double balanceToCalculateInterestOn = 0;
        double interestWaitingToCompound = 0;

        while (currentDate <= dateTo)
        {
            // Get transactions for the current date
            var transactionsForDate = Transactions
                .Where(x => x.Date == currentDate)
                .ToList();
            
            // Get balance for the current date, excluding interest
            var depositsForPeriod = transactionsForDate
                .Where(x => x.Type is TransactionType.Deposit)
                .Sum(x => x.Amount);
            
            var withdrawalsForPeriod  = transactionsForDate
                .Where(x => x.Type is TransactionType.Withdraw)
                .Sum(x => x.Amount);
            
            var balanceForPeriod = depositsForPeriod - withdrawalsForPeriod;
            balanceToCalculateInterestOn += balanceForPeriod;
            
            // Find out whether or not interest should be paid today
            var payInterestToday = PayInterestToday(dateLastInterestPaid, currentDate);
            if (payInterestToday)
            {
                // Calculate interest to be paid and add it to the account
                dateLastInterestPaid = currentDate;
                var interestToAdd = balanceToCalculateInterestOn * _actualInterestRate;
                interestWaitingToCompound += interestToAdd;
                
                Transactions.Add(new Transaction
                {
                    Date = currentDate,
                    Amount = interestToAdd,
                    Type = TransactionType.Interest
                });
            }

            if (CompoundInterestToday(dateInterestLastCompounded, currentDate))
            {
                balanceToCalculateInterestOn += interestWaitingToCompound;
                interestWaitingToCompound = 0;
                dateInterestLastCompounded = currentDate;
            }
            
            currentDate = currentDate.AddDays(1);
        }
        
        
        // Finally add new interest and benefits transactions
        Transactions = Transactions
            .OrderBy(x => x.Date)
            .ToList();
    }
}