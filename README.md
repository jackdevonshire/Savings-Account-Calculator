# Savings-Account-Calculator
A basic C# application for calculating the potential returns of various savings accounts.

## Why Make This?
After starting my graduate job, I realised I needed a plan for effectively saving my money. A lot of research and google sheets formulas later,
I figured out the perfect allocation of my funds on a monthly basis, to maximise my payoff over various accounts. As with all savings accounts, the rates will change over time, and when it comes time
to research potential returns on accounts in the future, I want a faster way to determine the exact benefits of different types of savings accounts. Hence
this project being born.

## Limitations
I'm no financial advisor, so naturally as I develop this application there will be things and rules that I discover. Below is a list of rules / information for various accounts that this project does NOT model.

- Lifetime ISA / Regular ISA Rules (https://www.gov.uk/lifetime-isa)
  - Deposits
    - A person may not have over £20,000 combined in ISA accounts. This is known as the annual ISA limit
    - Maximum age limit of 50 years old, after this age you cannot deposit into an ISA
    - Maximum starting age limit of 40 years old, you must make your first payment into an ISA before this age
  - Withdrawals
    - This program will model ALL withdrawals with a 25% withdrawal charge, this is because Lifetime ISA's are meant for long term saving. In reality, the withdrawal charge will be applied, unless you are:
        - Buying your first house that costs under 450k
        - Aged 60 or over
        - Terminally ill and have less than 12 months to live
     
- Tax
  - I may aswell switch career if I become an expert on how savings are taxed. Tax is not modelled by this program.
   
## Disclaimer
This is just a personal portfolio project. It may contain issues, and it's models of accounts may be incorrect or outdated, so please don't use it to actually plan your savings...It would probably be a bad idea without verifying the numbers yourself.
