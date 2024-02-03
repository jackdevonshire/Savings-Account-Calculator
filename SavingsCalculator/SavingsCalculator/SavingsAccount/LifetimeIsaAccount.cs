using SavingsCalculator.Helpers;
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
        var currentTaxYear = InterestAndTaxHelper.GetTaxYearForDate(transactionDate);
        var depositsForTaxYear = Transactions
            .Where(x =>
                x.Date >= currentTaxYear.StartOfTaxYear &&
                x.Date <= currentTaxYear.EndOfTaxYear &&
                x.Type == TransactionType.Deposit
            )
            .Sum(x => x.Amount);

        var totalDeposits = depositsForTaxYear + depositAmount;
        
        if (totalDeposits <= 4000)
            return true;

        return false;
    }
    
    protected override void CalculateFinance(DateOnly? dateTo)
    {
        // First clear current transaction log of interest and benefit payments, and order by date ascending
        Transactions = Transactions.Where(x => x.Type is 
            TransactionType.Deposit or 
            TransactionType.Withdraw or 
            TransactionType.Penalty
        ).ToList();
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
            
            var withdrawalsForMonth = transactionsForMonth.Where(x => x.Type is 
                TransactionType.Withdraw or 
                TransactionType.Penalty
            ).Sum(x => x.Amount);

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
    
    public override BaseSavingsAccount Deposit(DateOnly date, double amount)
    {
        if (amount < 0)
            throw new Exception("Cannot deposit a negative amount");

        if (!CanDeposit(date, amount))
            throw new Exception("Cannot deposit over £4000 within the same tax year");
        
        Transactions.Add(new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.Deposit
        });

        return this;
    }
    
    public override BaseSavingsAccount Withdraw(DateOnly date, double amount)
    {
        if (amount < 0)
            throw new Exception("Cannot withdraw a negative amount");
        
        // Calculate ISA specific withdrawal fee
        var withdrawalFee = amount * 0.25;
        CalculateFinance(date);
        var balance = GetAccountSummary(date).FinalBalance;
        if ((balance - amount - withdrawalFee) < 0)
            throw new Exception(
                "Withdrawing this amount incurs a 25% fee, which would put your account balance below zero");
        
        Transactions.Add(new Transaction
        {
            Date = date,
            Amount = withdrawalFee,
            Type = TransactionType.Penalty
        });
        
        Transactions.Add(new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.Withdraw
        });
        
        return this;
    }

    public override BaseSavingsAccount SetupMonthlyDeposit(DateOnly dateFrom, DateOnly dateTo, int dayOfMonth, double amount)
    {
        if (amount < 0)
            throw new Exception("Cannot deposit a negative amount");
        
        if (dayOfMonth is < 1 or > 31)
            throw new Exception("Invalid day of the month provided");
        
        DateOnly currentDate = dateFrom;

        List<Transaction> deposits = new List<Transaction>();
        while (currentDate < dateTo)
        {
            if (!CanDeposit(currentDate, amount))
                throw new Exception("Cannot deposit over £4000 within the same tax year");
            
            deposits.Add(new Transaction
            {
                Date = new DateOnly(currentDate.Year, currentDate.Month, dayOfMonth),
                Amount = amount,
                Type = TransactionType.Deposit
            });
            
            currentDate = currentDate.AddMonths(1);
        }
        
        Transactions.AddRange(deposits);
        
        return this;
    }
}