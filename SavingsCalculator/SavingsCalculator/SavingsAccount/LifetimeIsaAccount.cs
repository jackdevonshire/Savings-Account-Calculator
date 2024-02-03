using SavingsCalculator.Helpers;
using SavingsCalculator.Reports;
using SavingsCalculator.Types;

namespace SavingsCalculator.SavingsAccount;

public class LifetimeIsaAccount : BaseSavingsAccount
{
    private readonly InterestPaidType _interestPaidType;
    private readonly CompoundType _compoundType;
    private readonly double _actualInterestRate;

    public LifetimeIsaAccount(
        string accountName, 
        DateOnly openingDate,
        double openingBalance, 
        double annualEquivalentRateAsPercentage,
        InterestPaidType interestPaidType = InterestPaidType.Monthly,
        CompoundType compoundType = CompoundType.Annually
    ) : base(accountName, openingDate, openingBalance)
    {
        _interestPaidType = interestPaidType;
        _compoundType = compoundType;
        _actualInterestRate = InterestAndTaxHelper.GetActualInterestRate(_interestPaidType, annualEquivalentRateAsPercentage);
    }
    
    public BonusPeriod GetBonusPeriodForDate(DateOnly currentDate)
    {
        /*
         * Bonuses are calculated based on deposits within a certain bonus period,
         * and then paid at the end of the current month. This method will get the
         * bonus period for the current date.
         */
        
        DateOnly startOfBonusPeriod = new DateOnly(currentDate.Year, currentDate.Month, 6);
        DateOnly endOfBonusPeriod = new DateOnly(currentDate.Year, currentDate.Month, 5).AddMonths(1);

        if (currentDate < startOfBonusPeriod)
        {
            startOfBonusPeriod = new DateOnly(currentDate.Year, currentDate.Month, 5).AddMonths(-1);
            endOfBonusPeriod = new DateOnly(currentDate.Year, currentDate.Month, 5);
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
        Transactions = Transactions
            .Where(x => x.Type is TransactionType.Deposit or TransactionType.Withdraw or TransactionType.Penalty)
            .OrderBy(x => x.Date)
            .ToList();
        
        // Exit if no transactions at all
        if (Transactions.Count == 0)
            return;
        
        // Now get transactions we want to loop through
        dateTo ??= Transactions.Last().Date;
        var transactionsToCalculate = Transactions;
        transactionsToCalculate = transactionsToCalculate.Where(x => x.Date <= dateTo.Value).ToList();
        
        // Exit if no transactions for selected period
        if (transactionsToCalculate.Count == 0)
            return;
        
        // Now initialise essential variables
        var openingDate = Transactions.First().Date;
        var currentDate = openingDate;
        var dateLastInterestPaid = openingDate;
        var dateInterestLastCompounded = openingDate;
        
        // Variables representing accounts balance
        double balanceToCalculateInterestOn = 0;
        double interestWaitingToCompound = 0;
        double amountToCalculateBonusOn = 0;

        while (currentDate <= dateTo)
        {
            // Get transactions for the current date
            var transactionsForDate = Transactions
                .Where(x => x.Date == currentDate)
                .ToList();
            
            // Get balance for the current date, excluding interest
            var depositsForPeriod = transactionsForDate
                .Where(x => x.Type is TransactionType.Deposit)
                .Sum(x => x.Amount);
            
            var bonusForPeriod = transactionsForDate
                .Where(x => x.Type is TransactionType.GovernmentISABenefit)
                .Sum(x => x.Amount);
            
            var withdrawalsForPeriod  = transactionsForDate
                .Where(x => x.Type is TransactionType.Withdraw or TransactionType.Penalty)
                .Sum(x => x.Amount);

            amountToCalculateBonusOn += depositsForPeriod;
            
            var balanceForPeriod = depositsForPeriod + bonusForPeriod - withdrawalsForPeriod;
            balanceToCalculateInterestOn += balanceForPeriod;
            
            // Find out whether or not interest should be paid today
            var payInterestToday = InterestAndTaxHelper.PayInterestToday(_interestPaidType, dateLastInterestPaid, currentDate);
            if (payInterestToday)
            {
                // Calculate interest to be paid and add it to the account
                dateLastInterestPaid = currentDate;
                var interestToAdd = balanceToCalculateInterestOn * _actualInterestRate;
                interestWaitingToCompound += interestToAdd;
                
                Transactions.Add(new Transaction
                {
                    Date = currentDate,
                    Amount = interestToAdd,
                    Type = TransactionType.Interest
                });
            }
            
            // Find out whether or not to compound interest today
            if (InterestAndTaxHelper.CompoundInterestToday(_compoundType, dateInterestLastCompounded, currentDate))
            {
                balanceToCalculateInterestOn += interestWaitingToCompound;
                interestWaitingToCompound = 0;
                dateInterestLastCompounded = currentDate;
            }
            
            // Now we do government bonus calculations
            var bonusPeriod = GetBonusPeriodForDate(currentDate);
            
            if (currentDate == bonusPeriod.EndOfBonusPeriod)
            {
                var calculatedBonus = amountToCalculateBonusOn * 0.25;
                Transactions.Add(new Transaction
                {
                    Type = TransactionType.GovernmentISABenefit,
                    Amount = calculatedBonus,
                    Date = new DateOnly(bonusPeriod.EndOfBonusPeriod.Year, bonusPeriod.EndOfBonusPeriod.Month, 28)
                });
                
                amountToCalculateBonusOn = 0;
            }
            
            currentDate = currentDate.AddDays(1);
        }
        
        // Finally add new interest and benefits transactions
        Transactions = Transactions
            .OrderBy(x => x.Date)
            .ToList();
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