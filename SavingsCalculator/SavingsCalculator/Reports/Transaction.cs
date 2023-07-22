namespace SavingsCalculator.Reports;

public enum TransactionType
{
    Withdraw = 0,
    Deposit = 1,
    Interest = 2,
    GovernmentISABenefit = 3,
    Penalty = 4
}

public class Transaction
{
    public DateOnly Date;
    public TransactionType Type;
    public double Amount;
}