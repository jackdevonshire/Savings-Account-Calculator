using SavingsCalculator.Reports;

namespace SavingsCalculator.SavingsAccount;

public class InstantAccessAccount : ISavingsAccount
{
    private List<Transaction> _transactions;
    private string _accountName;
    private double _annualEquivalentRateAsPercentage;

    public InstantAccessAccount(
        string accountName, 
        DateOnly openingDate,
        double openingBalance, 
        double annualEquivalentRateAsPercentage
        )
    {
        _annualEquivalentRateAsPercentage = annualEquivalentRateAsPercentage;
        _accountName = accountName;
        _transactions = new List<Transaction>
        {
            new Transaction
            {
                Amount = openingBalance,
                Date = openingDate,
                Type = TransactionType.DEPOSIT
            }
        };
    }
    
    private void CalculateFinance()
    {
        // First clear current transaction log of interest and benefit payments, and order by date ascending
        _transactions = _transactions.Where(x => x.Type is TransactionType.DEPOSIT or TransactionType.WITHDRAW).ToList();
        _transactions = _transactions.OrderBy(x => x.Date).ToList();

        var interestAndBenefitsTransactions = new List<Transaction>();
        // Now loop through months and years etc
        var dateFrom = _transactions.First().Date;
        var dateTo = _transactions.Last().Date;
        
        var currentDate = dateFrom;
        double totalBalance = 0;
        while (currentDate <= dateTo)
        {
            // Here savings account specific calculation logic goes
            var transactionsForMonth = _transactions
                .Where(x => x.Date.Month == currentDate.Month && x.Date.Year == currentDate.Year)
                .OrderBy(x => x.Date)
                .ToList();
            
            var depositsForMonth = transactionsForMonth.Where(x => x.Type != TransactionType.WITHDRAW).Sum(x => x.Amount);
            var withdrawalsForMonth = transactionsForMonth.Where(x => x.Type == TransactionType.WITHDRAW).Sum(x => x.Amount);
            
            var balanceForMonth = depositsForMonth - withdrawalsForMonth;
            var interestForMonth = (totalBalance + balanceForMonth) * ((_annualEquivalentRateAsPercentage / 100) / 12);

            totalBalance += balanceForMonth + interestForMonth;
            _transactions.Add(new Transaction
            {
                Type = TransactionType.INTEREST,
                Date = new DateOnly(currentDate.Year, currentDate.Month, 28),
                Amount = interestForMonth
            });
            
            currentDate = currentDate.AddMonths(1);
        }

        // Finally add new interest and benefits transactions
        _transactions = _transactions.OrderBy(x => x.Date).ToList();
    }

    public AccountSummary GetAccountSummary()
    {
        CalculateFinance();

        var deposits = _transactions.Where(x => x.Type != TransactionType.WITHDRAW).Sum(x => x.Amount);
        var withdrawals = _transactions.Where(x => x.Type == TransactionType.WITHDRAW).Sum(x => x.Amount);
        var finalBalance = deposits - withdrawals;

        var totalInterestAndBenefits = _transactions
            .Where(x => x.Type is TransactionType.INTEREST or TransactionType.OTHER_BENEFIT).Sum(x => x.Amount);

        return new AccountSummary
        {
            DateFrom = _transactions.First().Date,
            DateTo = _transactions.Last().Date,
            Transactions = _transactions,
            FinalBalance = finalBalance,
            TotalCumulativeInterestAndBenefits = totalInterestAndBenefits
        };
    }

    public virtual ISavingsAccount Deposit(DateOnly date, double amount)
    {
        _transactions.Add(new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.DEPOSIT
        });

        return this;
    }
    
    public virtual ISavingsAccount Withdraw(DateOnly date, double amount)
    {
        _transactions.Add(new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.DEPOSIT
        });
        
        return this;
    }

    public virtual ISavingsAccount SetupMonthlyDeposit(DateOnly dateFrom, DateOnly dateTo, int dayOfMonth, double amount)
    {
        if (dayOfMonth is < 1 or > 31)
            throw new Exception("Invalid day of the month provided");
        
        DateOnly currentDate = dateFrom;
        
        while (currentDate <= dateTo)
        {
            _transactions.Add(new Transaction
                {
                    Date = new DateOnly(currentDate.Year, currentDate.Month, dayOfMonth),
                    Amount = amount,
                    Type = TransactionType.DEPOSIT
                });
            
            currentDate = currentDate.AddMonths(1);
        }
        
        return this;
    }
}