using SavingsCalculator.Reports;
using SavingsCalculator.Types;

namespace SavingsCalculator.SavingsAccount;

public class LifetimeIsaAccount : BaseSavingsAccount
{
    private readonly double _annualEquivalentRateAsPercentage;

    public LifetimeIsaAccount(
        string accountName, 
        DateOnly openingDate,
        double openingBalance, 
        double annualEquivalentRateAsPercentage
    ) : base(accountName, openingDate, openingBalance)
    {
        _annualEquivalentRateAsPercentage = annualEquivalentRateAsPercentage;
    }
    
    public TaxYear GetTaxYearForDate(DateOnly date)
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
    
    public BonusPeriod GetBonusPeriodForDate(DateOnly date)
    {
        /*
         * Bonuses are calculated based on deposits within a certain bonus period,
         * and then paid at the end of the current month. This method will get the
         * bonus period for the current date.
         */
        
        DateOnly startOfBonusPeriod = new DateOnly(date.Year, date.Month, 6);
        DateOnly endOfBonusPeriod = new DateOnly(date.Year, date.Month, 5).AddMonths(1);

        if (date < startOfBonusPeriod)
        {
            startOfBonusPeriod = new DateOnly(date.Year, date.Month - 1, 6);
            endOfBonusPeriod = new DateOnly(date.Year, date.Month, 5);
        }
        return new BonusPeriod()
        {
            StartOfBonusPeriod = startOfBonusPeriod,
            EndOfBonusPeriod = endOfBonusPeriod
        };
    }

    public bool CanDeposit(DateOnly transactionDate, double depositAmount)
    {
        var currentTaxYear = GetTaxYearForDate(transactionDate);
        var transactionsForTaxYear = Transactions.Where(x =>
            x.Date >= currentTaxYear.StartOfTaxYear &&
            x.Date <= currentTaxYear.EndOfTaxYear
        ).ToList();

        var totalDeposits = transactionsForTaxYear.Where(x => x.Type == TransactionType.Deposit).Sum(x => x.Amount);
        var totalWithdrawals = transactionsForTaxYear.Where(x => x.Type == TransactionType.Withdraw).Sum(x => x.Amount);
        var totalBalance = totalDeposits - totalWithdrawals;

        var newBalance = totalBalance + depositAmount;

        if (newBalance <= 4000)
            return true;

        return false;
    }
    
    protected override void CalculateFinance(DateOnly? dateTo)
    {
        // First clear current transaction log of interest and benefit payments, and order by date ascending
        Transactions = Transactions.Where(x => x.Type is TransactionType.Deposit or TransactionType.Withdraw).ToList();
        Transactions = Transactions.OrderBy(x => x.Date).ToList();

        /* Get the starting month so we know when to pay interest into account for it to compound
         * Based on the AER, interest should roughly be paid on the account birthday. Whilst not
         * entirely accurate, it's a very good estimation and is how most online calculators appear to
         * be doing it. This only really has an impact when we start seeing large, large sums of money -
         * and in that case you should probably be seeing a financial advisor, and not using a random
         * program found on Github.
         */
        var payInterestOn = Transactions.First().Date.AddYears(1);
        
        // Now loop through months and years etc
        var dateFrom = Transactions.First().Date;
        dateTo ??= Transactions.Last().Date;
        
        var currentDate = dateFrom;
        
        double totalBalance = 0;
        double totalInterestForYear = 0;

        BonusPeriod currentBonusPeriod = GetBonusPeriodForDate(currentDate);

        while (currentDate < dateTo)
        {
            var transactionsForMonth = Transactions
                .Where(x => x.Date.Month == currentDate.Month && x.Date.Year == currentDate.Year)
                .OrderBy(x => x.Date)
                .ToList();

            var depositsForMonth = transactionsForMonth.Where(x => x.Type is 
                TransactionType.Deposit or
                TransactionType.GovernmentISABenefit
            ).Sum(x => x.Amount);
            var withdrawalsForMonth = transactionsForMonth.Where(x => x.Type == TransactionType.Withdraw).Sum(x => x.Amount);

            var balanceForMonth = depositsForMonth - withdrawalsForMonth;
            var interestForMonth = (totalBalance + balanceForMonth) * ((_annualEquivalentRateAsPercentage / 100) / 12);

            totalBalance += balanceForMonth;
            totalInterestForYear += interestForMonth;
            
            Transactions.Add(new Transaction
            {
                Type = TransactionType.Interest,
                Date = new DateOnly(currentDate.Year, currentDate.Month, 28),
                Amount = interestForMonth
            });
            
            var newDate = currentDate.AddMonths(1);
            
            // If in a new bonus period, add government bonus for the previous periods transactions
            BonusPeriod newBonusPeriod = GetBonusPeriodForDate(currentDate);
            if (currentBonusPeriod.StartOfBonusPeriod.Month != newBonusPeriod.StartOfBonusPeriod.Month)
            {
                var transactionsForBonusPeriod = Transactions
                    .Where(x => x.Date >= currentBonusPeriod.StartOfBonusPeriod && x.Date <= currentBonusPeriod.EndOfBonusPeriod)
                    .OrderBy(x => x.Date)
                    .ToList(); 
                
                var depositsForBonusPeriod = transactionsForBonusPeriod.Where(x => x.Type is 
                    TransactionType.Deposit
                ).Sum(x => x.Amount);
                var withdrawalsForBonusPeriod = transactionsForBonusPeriod.Where(x => x.Type == TransactionType.Withdraw).Sum(x => x.Amount);

                var balanceForBonusPeriod = depositsForBonusPeriod - withdrawalsForBonusPeriod;
                var bonus = balanceForBonusPeriod * 0.25;
                if (bonus > 1000)
                    bonus = 1000;
                
                Transactions.Add(new Transaction
                {
                    Type = TransactionType.GovernmentISABenefit,
                    Amount = bonus,
                    Date = new DateOnly(currentBonusPeriod.EndOfBonusPeriod.Year, currentBonusPeriod.EndOfBonusPeriod.Month, 28)
                });
                currentBonusPeriod = newBonusPeriod;
                totalBalance += bonus;
            }
            
            // If account birthday, pay interest into balance so that it can be compounded
            if (newDate.Year == payInterestOn.Year && newDate.Month == payInterestOn.Month)
            {
                totalBalance += totalInterestForYear;
                totalInterestForYear = 0;
                payInterestOn = payInterestOn.AddYears(1);
            }
            currentDate = newDate;
;        }

        // Finally add new interest and benefits transactions
        Transactions = Transactions.OrderBy(x => x.Date).ToList();
    } 
}