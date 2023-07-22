using SavingsCalculator.Reports;

namespace SavingsCalculator.SavingsAccount;

public class BaseSavingsAccount
{
    protected string AccountName;
    protected List<Transaction> Transactions;

    protected BaseSavingsAccount(string accountName, DateOnly openingDate, double openingBalance)
    {
        AccountName = accountName;
        Transactions = new List<Transaction>
        {
            new Transaction
            {
                Amount = openingBalance,
                Date = openingDate,
                Type = TransactionType.DEPOSIT
            }
        };
    }

    protected virtual void CalculateFinance()
    {
        throw new NotImplementedException();
    }

    public virtual AccountSummary GetAccountSummary()
    {
        CalculateFinance();

        var deposits = Transactions.Where(x => x.Type != TransactionType.WITHDRAW).Sum(x => x.Amount);
        var withdrawals = Transactions.Where(x => x.Type == TransactionType.WITHDRAW).Sum(x => x.Amount);
        var finalBalance = deposits - withdrawals;

        var totalInterestAndBenefits = Transactions
            .Where(x => x.Type is TransactionType.INTEREST or TransactionType.OTHER_BENEFIT).Sum(x => x.Amount);

        return new AccountSummary
        {
            Account = AccountName,
            DateFrom = Transactions.First().Date,
            DateTo = Transactions.Last().Date,
            Transactions = Transactions,
            FinalBalance = finalBalance,
            TotalCumulativeInterestAndBenefits = totalInterestAndBenefits
        };
    }
    
    public virtual BaseSavingsAccount Deposit(DateOnly date, double amount)
    {
        if (amount < 0)
            throw new Exception("Cannot deposit a negative amount");
        
        Transactions.Add(new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.DEPOSIT
        });

        return this;
    }
    
    public virtual BaseSavingsAccount Withdraw(DateOnly date, double amount)
    {
        if (amount < 0)
            throw new Exception("Cannot withdraw a negative amount");
        
        Transactions.Add(new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.DEPOSIT
        });
        
        return this;
    }

    public virtual BaseSavingsAccount SetupMonthlyDeposit(DateOnly dateFrom, DateOnly dateTo, int dayOfMonth, double amount)
    {
        if (amount < 0)
            throw new Exception("Cannot deposit a negative amount");
        
        if (dayOfMonth is < 1 or > 31)
            throw new Exception("Invalid day of the month provided");
        
        DateOnly currentDate = dateFrom;
        
        while (currentDate < dateTo)
        {
            Transactions.Add(new Transaction
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