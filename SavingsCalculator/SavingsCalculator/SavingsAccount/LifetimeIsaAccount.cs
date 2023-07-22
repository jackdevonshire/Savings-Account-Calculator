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
    
    public TaxYear GetTaxYearForDate(DateOnly transactionDate)
    {
        /* Tax years run differently to normal years, and are typically run from the 6th April from one year, to the 5th April the next
         * God knows why... https://www.gov.uk/self-assessment-tax-returns
         * 
         * When depositing and withdrawing, we need to know that the total ingoing / outgoing balance for the year will NOT exceed £4000
         * As per the rules for Lifetime ISA's https://www.gov.uk/lifetime-isa This does NOT include interest gained with the current provider,
         * It also does NOT include the 25% bonus the government provides at the end of each tax year
         */
        
        // Get tax year for the current transaction year
        DateOnly startOfTaxYear = new DateOnly(transactionDate.Year, 4, 6);
        DateOnly endOfTaxYear = new DateOnly(transactionDate.Year + 1, 4, 5);

        // If the transaction is before the start of the current tax year (6th April), then adjust accordingly
        if (transactionDate < startOfTaxYear)
        {
            startOfTaxYear = new DateOnly(transactionDate.Year - 1, 4, 6);
            endOfTaxYear = new DateOnly(transactionDate.Year, 4, 5);
        }

        return new TaxYear
        {
            StartOfTaxYear = startOfTaxYear,
            EndOfTaxYear = endOfTaxYear
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
        Transactions = Transactions.Where(x => x.Type is TransactionType.Deposit or TransactionType.Withdraw or TransactionType.Penalty).ToList();
        Transactions = Transactions.OrderBy(x => x.Date).ToList();

        // Now loop through months and years etc
        var dateFrom = Transactions.First().Date;
        dateTo ??= Transactions.Last().Date;
        
        var currentDate = dateFrom;
        double totalBalance = 0;

        double totalGovBonusForEachTaxYear = 0;
        var currentTaxYear = GetTaxYearForDate(currentDate);
        double totalInterestForYear = 0;
        while (currentDate < dateTo)
        {
            var transactionsForMonth = Transactions
                .Where(x => x.Date.Month == currentDate.Month && x.Date.Year == currentDate.Year)
                .OrderBy(x => x.Date)
                .ToList();
            
            var monthlyIn = transactionsForMonth.Where(x => x.Type is 
                TransactionType.Deposit // Government added benefits are accounted for later
                ).Sum(x => x.Amount);
            var monthlyOut = transactionsForMonth.Where(x => x.Type is 
                TransactionType.Withdraw or 
                TransactionType.Penalty
                ).Sum(x => x.Amount);
            
            var balanceForMonthExcludingInterest = monthlyIn - monthlyOut;
            var interestForMonth = (totalBalance + balanceForMonthExcludingInterest) * ((_annualEquivalentRateAsPercentage / 100) / 12);
            totalInterestForYear += interestForMonth;
            totalBalance += balanceForMonthExcludingInterest;
            Transactions.Add(new Transaction
            {
                Type = TransactionType.Interest,
                Date = new DateOnly(currentDate.Year, currentDate.Month, 28),
                Amount = interestForMonth
            });

            var newDate = currentDate.AddMonths(1);
            if (newDate.Year != currentDate.Year) // If going in to a new year, add accumulated interest to balance so that this can be compounded
            {
                totalBalance += totalInterestForYear;
                totalInterestForYear = 0;
            }
            currentDate = newDate;
            
            // If next month is in a new tax year, add the tax bonus for the past tax year
            var newTaxYear = GetTaxYearForDate(currentDate);
            if (newTaxYear.StartOfTaxYear != currentTaxYear.StartOfTaxYear)
            {
                var taxYearIn = Transactions // This does NOT include previous benefits, or interest gained
                    .Where(x => x.Type is 
                        TransactionType.Deposit &&
                        x.Date >= currentTaxYear.StartOfTaxYear &&
                        x.Date <= currentTaxYear.EndOfTaxYear
                    )
                    .Sum(x => x.Amount);
                
                var taxYearOut = Transactions
                    .Where(x => x.Type is 
                        TransactionType.Withdraw or 
                        TransactionType.Penalty &&
                        x.Date >= currentTaxYear.StartOfTaxYear &&
                        x.Date <= currentTaxYear.EndOfTaxYear
                    ).Sum(x => x.Amount);

                var balanceForTaxYearExcludingPrevBenefits = taxYearIn - taxYearOut;
                var benefit = balanceForTaxYearExcludingPrevBenefits * 0.25;
                totalBalance += benefit;
                Transactions.Add(new Transaction
                {
                    Type = TransactionType.GovernmentISABenefit,
                    Date = currentTaxYear.EndOfTaxYear,
                    Amount = benefit
                });
            }

            currentTaxYear = newTaxYear;
        }
        
        Transactions = Transactions.OrderBy(x => x.Date).ToList();
    } 
}