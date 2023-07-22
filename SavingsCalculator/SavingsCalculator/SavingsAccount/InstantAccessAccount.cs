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

            var depositsForMonth = transactionsForMonth.Where(x => x.Type != TransactionType.Withdraw).Sum(x => x.Amount);
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
            if (newDate.Year != currentDate.Year) // If going in to a new year, add accumulated interest to balance so that this can be compounded
            {
                totalBalance += totalInterestForYear;
                totalInterestForYear = 0;
            }
            currentDate = newDate;
        }

        // Finally add new interest and benefits transactions
        Transactions = Transactions.OrderBy(x => x.Date).ToList();
    }
}