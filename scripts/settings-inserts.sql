-- Sample inserts for Bank (Rwanda)
INSERT INTO Banks (
    Name, Abbreviation, Status, CreatedAt, UpdatedAt, CreatedById, UpdatedById
) VALUES
('Cash','CSH', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Mobile Money','MOMO', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Bank of Kigali','BK', 'Active', GETDATE(), GETDATE(), '1', '1'),
('I&M Bank Rwanda', 'I&M', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Equity Bank Rwanda', 'EQ', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Cogebanque', 'CBK', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Access Bank Rwanda', 'ABR', 'Active', GETDATE(), GETDATE(), '1', '1');

-- Sample inserts for TargetTypes
INSERT INTO TargetTypes (
    Name, Status, CreatedAt, UpdatedAt, CreatedById, UpdatedById
) VALUES
('Members', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Savings', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Loans', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Social Funds', 'Active', GETDATE(), GETDATE(), '1', '1');

-- Sample inserts for InterestTypes
INSERT INTO InterestTypes (
    Name, Description, Status, CreatedAt, UpdatedAt, CreatedById, UpdatedById
) VALUES
('Savings Interest', 'Savings Interest', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Loan Interest', 'Loan Interest', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Late Payment', 'Late Payment', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Penalty', 'Penalty', 'Active', GETDATE(), GETDATE(), '1', '1');


-- Sample inserts for InterestPeriods
INSERT INTO InterestPeriods (
    Name, Status, CreatedAt, UpdatedAt, CreatedById, UpdatedById
) VALUES
('Daily', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Weekly', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Monthly', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Quarterly', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Per Cycle', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Yearly', 'Active', GETDATE(), GETDATE(), '1', '1');


-- Sample inserts for FineTypes
INSERT INTO FineTypes (
    Name, Status, CreatedAt, UpdatedAt, CreatedById, UpdatedById
) VALUES
('Late meeting', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Non contribution sanction', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Delayed savings sanction', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Delayed loan repayment sanction', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Late savings collection', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Late loan repayment', 'Active', GETDATE(), GETDATE(), '1', '1');

-- Sample inserts for TransactionTypes
INSERT INTO TransactionTypes (
    Name, Effect, Status, CreatedAt, UpdatedAt, CreatedById, UpdatedById
) VALUES
('Opening Balance', 'Credit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Deposit', 'Credit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Withdrawal', 'Debit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Loan Disbursement', 'Debit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Loan Repayment', 'Credit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Interest Income', 'Credit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Fine Collection', 'Credit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Social Fund Contribution', 'Credit', 'Active', GETDATE(), GETDATE(), '1', '1'),
('Social Fund Withdrawal', 'Debit', 'Active', GETDATE(), GETDATE(), '1', '1');