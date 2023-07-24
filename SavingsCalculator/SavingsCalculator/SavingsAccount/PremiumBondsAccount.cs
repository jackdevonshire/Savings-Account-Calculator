using SavingsCalculator.Reports;
using SavingsCalculator.Types;

namespace SavingsCalculator.SavingsAccount;

public class PremiumBondsAccount : BaseSavingsAccount
{
    public PremiumBondsAccount(
        string accountName,
        DateOnly openingDate,
        double openingBalance
    ) : base(accountName, openingDate, openingBalance)
    { }

    public override BaseSavingsAccount Deposit(DateOnly date, double amount)
    {
        throw new Exception("Don't waste your money! https://www.moneysavingexpert.com/savings/premium-bonds/");
    }
}