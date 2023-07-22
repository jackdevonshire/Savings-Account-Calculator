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
    
    public TaxYear GetTaxYearForTransaction(DateOnly transactionDate)
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
        var currentTaxYear = GetTaxYearForTransaction(transactionDate);
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
}