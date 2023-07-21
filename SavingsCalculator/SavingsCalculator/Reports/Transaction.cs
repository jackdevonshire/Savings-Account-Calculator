namespace SavingsCalculator.Reports;

public enum TransactionType
{
    WITHDRAW = 0,
    DEPOSIT = 1,
    INTEREST = 2,
    OTHER_BENEFIT = 3
}

public class Transaction
{
    public DateOnly Date;
    public TransactionType Type;
    public double Amount;
}