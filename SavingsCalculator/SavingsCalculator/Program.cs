// See https://aka.ms/new-console-template for more information

using SavingsCalculator.SavingsAccount;

Console.WriteLine("Hello, World!");

var test = new InstantAccessAccount("Test", new DateOnly(2023, 01, 01), 0, 5);
test
    .SetupMonthlyDeposit(new DateOnly(2023, 01, 01), new DateOnly(2023, 12, 01), 28, 100);

Console.WriteLine(test.GetAccountSummary().FinalBalance);
    