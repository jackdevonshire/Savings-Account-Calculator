using SavingsCalculator.Reports;

namespace SavingsCalculator.SavingsAccount;

public class BaseSavingsAccount
{
    private readonly string _accountName;
    protected List<Transaction> Transactions;

    protected BaseSavingsAccount(string accountName, DateOnly openingDate, double openingBalance)
    {
        _accountName = accountName;
        Transactions = new List<Transaction>
        {
            new Transaction
            {
                Amount = openingBalance,
                Date = openingDate,
                Type = TransactionType.Deposit
            }
        };
    }

    protected virtual void CalculateFinance(DateOnly? dateTo)
    {
        throw new NotImplementedException();
    }

    public virtual AccountSummary GetAccountSummary(DateOnly? dateTo)
    {
        CalculateFinance(dateTo);

        var deposits = Transactions.Where(x => x.Type is 
                TransactionType.Deposit or 
                TransactionType.Interest or 
                TransactionType.GovernmentISABenefit
            );
        
        var withdrawals = Transactions.Where(x => x.Type is 
            TransactionType.Withdraw or 
            TransactionType.Penalty
            );

        if (dateTo != null)
        {
            deposits = deposits.Where(x => x.Date <= dateTo);
            withdrawals = withdrawals.Where(x => x.Date <= dateTo);
        }
        
        var finalBalance = deposits.Sum(x => x.Amount) - withdrawals.Sum(x => x.Amount);

        var totalInterestAndBenefits = Transactions.Where(x => x.Type is 
            TransactionType.Interest or 
            TransactionType.GovernmentISABenefit
        ).Sum(x => x.Amount);

        return new AccountSummary
        {
            Account = _accountName,
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
            Type = TransactionType.Deposit
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
            Type = TransactionType.Withdraw
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
                Type = TransactionType.Deposit
            });
            
            currentDate = currentDate.AddMonths(1);
        }
        
        return this;
    }
}