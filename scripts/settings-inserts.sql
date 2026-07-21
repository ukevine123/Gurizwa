-- Initial data for PaymentTypes
INSERT INTO [PaymentTypes] ([PaymentTypeName], [CreatedAt], [IsActive]) VALUES
('Cash', GETDATE(), 1),
('Bank Transfer', GETDATE(), 1),
('Mobile Money', GETDATE(), 1),
('Cheque', GETDATE(), 1);

-- Initial data for Reasons
INSERT INTO [Reasons] ([Name], [IsActive]) VALUES
('Late Payment Penalty', 1),
('Default Penalty', 1),
('Returned Cheque Penalty', 1),
('Breach of Contract Penalty', 1);

-- Initial data for AccountTypes
INSERT INTO [AccountTypes] ([AccountTypeName]) VALUES
('Bank'),
('Cash'),
('Mobile Money'),
('Airtel Money');

-- Initial data for BorrowerTypes
INSERT INTO [BorrowerTypes] ([Type]) VALUES
('Individual'),
('Business'),
('Group');

-- Initial data for GuarantorTypes
INSERT INTO [GuarantorTypes] ([Name]) VALUES
('Individual'),
('Business'),
('Group');

-- Initial data for PaymentModalities
INSERT INTO [PaymentModalities] ([Mode]) VALUES
('Daily'),
('Weekly'),
('Bi-Weekly'),
('Monthly'),
('Quarterly'),
('Annually');