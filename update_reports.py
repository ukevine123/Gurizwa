
import sys

file_path = r"C:\Users\HP\Documents\DigitalLoanPlatform2\Web\Components\Pages\Reports.razor"
with open(file_path, "r", encoding="utf-8") as f:
    lines = f.readlines()

start_idx = -1
end_idx = -1
for i, line in enumerate(lines):
    if "<DialogContent>" in line:
        start_idx = i
    elif "</DialogContent>" in line:
        end_idx = i
        break

new_content = """        <DialogContent>
    @if (_activeReport == "ActiveLoans")
    {
        <ActiveLoansReport 
            ActiveLoans="_activeLoans" 
            OnViewDetails="OpenActiveLoanDetails" />
    }
    else if (_activeReport == "Disbursements")
    {
        <DisbursementsReport 
            Disbursements="_disbursements"
            @bind-DisbursementRange="_disbursementRange"
            @bind-LoanTypeFilter="_loanTypeFilter"
            OnGenerateReport="LoadDisbursementReport"
            OnViewDetails="@((args) => OpenLoanDetails(args.LoanId, args.BorrowerName, args.AmountDisbursed, args.ApplicationCode))" />
    }
    else if (_activeReport == "MaturityTracker")
    {
        <MaturityTrackerReport 
            Maturities="_maturities"
            @bind-MaturityRange="_maturityRange"
            OnGenerateReport="LoadMaturityReport"
            OnViewDetails="@((args) => OpenLoanDetails(args.LoanId, args.BorrowerName, args.RemainingBalance, args.ApplicationCode))" />
    }
    else if (_activeReport == "UserActivities")
    {
        <UserActivityReport 
            TargetActivities="_activities.Where(a => string.IsNullOrEmpty(_activityUserSearch) || string.Equals(a.UserName, _activityUserSearch, StringComparison.OrdinalIgnoreCase)).ToList()"
            SubUsers="_subUsers"
            @bind-ActivityRange="_activityRange"
            @bind-ActivityUserSearch="_activityUserSearch"
            OnGenerateReport="LoadActivityReport"
            AppsCount="@(_activities.Where(a => (string.IsNullOrEmpty(_activityUserSearch) || string.Equals(a.UserName, _activityUserSearch, StringComparison.OrdinalIgnoreCase)) && a.EntityName != null && a.EntityName.Contains(\"Application\", StringComparison.OrdinalIgnoreCase)).Select(a => a.EntityId).Distinct().Count())"
            DisbCount="@(_activities.Count(a => (string.IsNullOrEmpty(_activityUserSearch) || string.Equals(a.UserName, _activityUserSearch, StringComparison.OrdinalIgnoreCase)) && ((a.EntityName != null && a.EntityName.Contains(\"Disbursement\", StringComparison.OrdinalIgnoreCase)) || (a.Action != null && a.Action.Contains(\"Disbursed\", StringComparison.OrdinalIgnoreCase)))))"
            PayCount="@(_activities.Count(a => (string.IsNullOrEmpty(_activityUserSearch) || string.Equals(a.UserName, _activityUserSearch, StringComparison.OrdinalIgnoreCase)) && ((a.EntityName != null && a.EntityName.Contains(\"Payment\", StringComparison.OrdinalIgnoreCase)) || (a.Action != null && (a.Action.Contains(\"Paid\", StringComparison.OrdinalIgnoreCase) || a.Action.Contains(\"Deposit\", StringComparison.OrdinalIgnoreCase))))))"
            />
    }
    else if (_activeReport == "RepaymentSchedules")
    {
        <RepaymentScheduleReport 
            RepaymentSchedules="_repaymentSchedules"
            @bind-SelectedApplicationCode="_selectedApplicationCode"
            OnExportFiltered="ExportFilteredRepayments"
            OnViewDetails="@((args) => OpenLoanDetails(args.LoanId, args.BorrowerName, args.Balance, args.ApplicationCode))" />
    }
    else if (_activeReport == "UpcomingDues")
    {
        <UpcomingDuesReport 
            RepaymentSchedules="_repaymentSchedules"
            @bind-ActiveUpcomingDuesTab="_activeUpcomingDuesTab"
            OnViewDetails="@((args) => OpenLoanDetails(args.LoanId, args.BorrowerName, args.Balance, args.ApplicationCode))" />
    }
    else if (_activeReport == "OverdueLoans")
    {
        <OverdueLoansReport 
            Overdues="_overdues"
            OnViewDetails="@((args) => OpenLoanDetails(args.LoanId, args.BorrowerName, args.OutstandingBalance, args.ApplicationCode))" />
    }
    else if (_activeReport == "CustomerPortfolios")
    {
        <CustomerPortfolioReport 
            CustomerPortfolios="_customerPortfolios"
            OnViewDetails="OpenCustomerPortfolioDetails" />
    }
    else if (_activeReport == "ApplicationStatuses")
    {
        <ApplicationStatusReport 
            ApplicationStatuses="_applicationStatuses"
            OnViewDetails="NavigateToApplication" />
    }
    else if (_activeReport == "IncomeStatement")
    {
        <IncomeStatementReport 
            IncomeStatement="_incomeStatement"
            @bind-IncomeStatementRange="_incomeStatementRange"
            OnGenerateReport="LoadIncomeStatement"
            OnViewDetails="OpenStatementDetailsDialog" />
    }
    else if (_activeReport == "LoanProductTracker")
    {
        <LoanProductTrackerReport 
            LoanProductTrackers="_loanProductTrackers"
            OnViewDetails="@((args) => OpenLoanDetails(args.Item1, args.Item2, args.Item3, args.Item4))" />
    }
    else if (_activeReport == "AccountHistory")
    {
        <AccountHistoryReport 
            AccountHistories="_accountHistories"
            @bind-AccountHistoryRange="_accountHistoryRange"
            OnGenerateReport="LoadAccountHistoryReport" />
    }
        </DialogContent>\n"""

if start_idx != -1 and end_idx != -1:
    lines[start_idx:end_idx+1] = [new_content]

lines.insert(4, "@using DigitalLoanPlatform2.Web.Components.Pages.ReportsFolder\n")

with open(file_path, "w", encoding="utf-8") as f:
    f.writelines(lines)

print("Replaced successfully")

