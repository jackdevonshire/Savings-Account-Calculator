using SavingsCalculator.Reports;

namespace SavingsCalculator.SavingsAccount;

public class InstantAccessAccount : BaseSavingsAccount
{
    private readonly double _annualEquivalentRateAsPercentage;

    public InstantAccessAccount(
        string accountName, 
        DateOnly openingDate,
        double openingBalance, 
        double annualEquivalentRateAsPercentage
) : base(accountName, openingDate, openingBalance)
    {
        _annualEquivalentRateAsPercentage = annualEquivalentRateAsPercentage;
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
        while (currentDate < dateTo)
        {
            // Here savings account specific calculation logic goes
            var transactionsForMonth = Transactions
                .Where(x => x.Date.Month == currentDate.Month && x.Date.Year == currentDate.Year)
                .OrderBy(x => x.Date)
                .ToList();

            var depositsForMonth = transactionsForMonth.Where(x => x.Type is 
                TransactionType.Deposit
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