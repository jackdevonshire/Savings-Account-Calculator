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

        var interestAndBenefitsTransactions = new List<Transaction>();
        // Now loop through months and years etc
        var dateFrom = Transactions.First().Date;
        dateTo ??= Transactions.Last().Date;
        
        var currentDate = dateFrom;
        double totalBalance = 0;
        while (currentDate < dateTo)
        {
            // Here savings account specific calculation logic goes
            var transactionsForMonth = Transactions
                .Where(x => x.Date.Month == currentDate.Month && x.Date.Year == currentDate.Year)
                .OrderBy(x => x.Date)
                .ToList();

            var depositsForMonth = transactionsForMonth.Where(x => x.Type != TransactionType.Withdraw).Sum(x => x.Amount);
            var withdrawalsForMonth = transactionsForMonth.Where(x => x.Type == TransactionType.Withdraw).Sum(x => x.Amount);
            
            var balanceForMonth = depositsForMonth - withdrawalsForMonth;
            var interestForMonth = (totalBalance + balanceForMonth) * ((_annualEquivalentRateAsPercentage / 100) / 12);

            totalBalance += balanceForMonth + interestForMonth;
            Transactions.Add(new Transaction
            {
                Type = TransactionType.Interest,
                Date = new DateOnly(currentDate.Year, currentDate.Month, 28),
                Amount = interestForMonth
            });
            
            currentDate = currentDate.AddMonths(1);
        }

        // Finally add new interest and benefits transactions
        Transactions = Transactions.OrderBy(x => x.Date).ToList();
    }
}