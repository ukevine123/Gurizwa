using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Application.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class ExportService : IExportService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IQRCodeService _qrCodeService;

        public ExportService(IWebHostEnvironment webHostEnvironment, IQRCodeService qrCodeService)
        {
            _webHostEnvironment = webHostEnvironment;
            _qrCodeService = qrCodeService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private static string FormatDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // Handle specific run-together cases
            name = name.Replace("paymentschedule", "Payment Schedule", StringComparison.OrdinalIgnoreCase);
            name = name.Replace("paymentschadule", "Payment Schedule", StringComparison.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1]))
                {
                    if (name[i - 1] != ' ' && name[i - 1] != '_')
                    {
                        sb.Append(' ');
                    }
                }
                sb.Append(c);
            }

            string result = sb.ToString().Replace("_", " ");
            
            // Professional Title Case formatting
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToLower());
        }

        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            
            string formattedSheetName = FormatDisplayName(sheetName);
            if (formattedSheetName.Length > 30)
                formattedSheetName = formattedSheetName.Substring(0, 30);

            var worksheet = workbook.Worksheets.Add(formattedSheetName);
            
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && 
                            !p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                            !(p.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)))
                .ToArray();
            
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = FormatDisplayName(properties[i].Name);
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var val = properties[i].GetValue(item);
                    if (val != null && (val.GetType() == typeof(decimal) || val.GetType() == typeof(int) || val.GetType() == typeof(double)))
                    {
                        worksheet.Cell(row, i + 1).Value = Convert.ToDouble(val);
                    }
                    else if (val is DateTime dateVal)
                    {
                        worksheet.Cell(row, i + 1).Value = dateVal;
                    }
                    else
                    {
                        worksheet.Cell(row, i + 1).Value = val?.ToString() ?? "";
                    }
                }
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private void ComposeHeader(IContainer container, string title, string webRootPath)
        {
            string displayTitle = FormatDisplayName(title);
            string appCode = "";
            if (title.Contains("_")) 
            {
                var parts = title.Split('_', 2);
                displayTitle = FormatDisplayName(parts[0]);
                appCode = parts[1];
            }

            container.Column(headerCol => 
            {
                headerCol.Item().Row(row =>
                {
                    var logoPath = Path.Combine(webRootPath, "Images", "guriza_logo.png");
                    if (File.Exists(logoPath))
                    {
                        row.ConstantItem(40).Image(logoPath);
                    }

                    row.RelativeItem().PaddingLeft(10).AlignMiddle().Column(col =>
                    {
                        col.Item().Text("Guriza").SemiBold().FontSize(22).FontColor("#1e1b4b");
                        col.Item().Text($"OFFICIAL {displayTitle.ToUpper()}").SemiBold().FontSize(10).FontColor(Colors.BlueGrey.Medium);
                    });

                    row.RelativeItem().AlignRight().AlignMiddle().Column(col =>
                    {
                        if (!string.IsNullOrEmpty(appCode))
                        {
                            col.Item().AlignRight().Text($"App Code: {appCode}").SemiBold().FontSize(11).FontColor("#1e1b4b");
                        }
                        col.Item().AlignRight().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(10).FontColor(Colors.BlueGrey.Medium);
                    });
                });
                
                headerCol.Item().PaddingTop(8).PaddingBottom(12).LineHorizontal(1.5f).LineColor("#1e1b4b");
            });
        }

        public byte[] ExportToPdf<T>(IEnumerable<T> data, string title, string qrCodeUrl = null)
        {
            if (typeof(T) == typeof(Application.DTO.IncomeStatementReportDTO))
            {
                return GenerateIncomeStatementPdf(data.Cast<Application.DTO.IncomeStatementReportDTO>(), title, qrCodeUrl);
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && 
                            !p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                            !(p.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)))
                .ToArray();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Element(c => ComposeHeader(c, title, _webHostEnvironment.WebRootPath));

                    page.Content().Element(compose => 
                    {
                        compose.PaddingVertical(1, Unit.Centimetre).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < properties.Length; i++)
                                    columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                foreach (var prop in properties)
                                {
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(6)
                                          .Text(FormatDisplayName(prop.Name)).SemiBold().FontColor(Colors.White).FontSize(9);
                                }
                            });

                            int rowIndex = 0;
                            foreach (var item in data)
                            {
                                var rowBackground = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                foreach (var prop in properties)
                                {
                                    var val = prop.GetValue(item);
                                    string textVal = val is DateTime d ? d.ToString("yyyy-MM-dd") : 
                                                     val is decimal dec ? dec.ToString("N2") :
                                                     val is double db ? db.ToString("N2") :
                                                     val is float fl ? fl.ToString("N2") :
                                                     val?.ToString() ?? "";

                                    table.Cell().Background(rowBackground).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                         .Padding(6).Text(textVal).FontSize(8);
                                }
                                rowIndex++;
                            }
                        });
                    });

                    page.Footer().Column(footer =>
                    {
                        if (!string.IsNullOrEmpty(qrCodeUrl))
                        {
                            try
                            {
                                var qrBase64 = _qrCodeService.GenerateQRCodeBase64(qrCodeUrl);
                                if (!string.IsNullOrEmpty(qrBase64))
                                {
                                    var qrBytes = Convert.FromBase64String(qrBase64);
                                    footer.Item().PaddingBottom(10).AlignLeft().Width(60).Height(60).Image(qrBytes);
                                }
                            }
                            catch { }
                        }
                        
                        footer.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateIncomeStatementPdf(IEnumerable<Application.DTO.IncomeStatementReportDTO> data, string title, string qrCodeUrl = null)
        {
            var statement = data.FirstOrDefault();
            if (statement == null) return new byte[0];

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => ComposeHeader(c, title, _webHostEnvironment.WebRootPath));

                    page.Content().Element(compose =>
                    {
                        compose.PaddingVertical(1, Unit.Centimetre).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(7);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("");
                                header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text($"Selected Period\n{statement.StartDate:yyyy-MM-dd} to {statement.EndDate:yyyy-MM-dd}").SemiBold().FontColor(Colors.White).FontSize(10).AlignRight();
                            });

                            // Revenue
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text("Revenue").SemiBold().FontSize(12).FontColor(Colors.Black);
                            table.Cell().Background(Colors.Grey.Lighten4);
                            
                            table.Cell().Padding(4).PaddingLeft(12).Text("Interest Income");
                            table.Cell().Padding(4).Text(statement.TotalInterestIncome.ToString("N2")).AlignRight();
                            
                            table.Cell().Padding(4).PaddingLeft(12).Text("Processing Fee Income");
                            table.Cell().Padding(4).Text(statement.TotalProcessingFeeIncome.ToString("N2")).AlignRight();

                            table.Cell().Padding(4).PaddingLeft(12).Text("Penalty Income");
                            table.Cell().Padding(4).Text(statement.TotalPenaltyIncome.ToString("N2")).AlignRight();

                            table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Total Revenue & Gains").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(statement.GrossIncome.ToString("N2")).SemiBold().AlignRight();

                            // Expenses
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text("Expenses").SemiBold().FontSize(12).FontColor(Colors.Black);
                            table.Cell().Background(Colors.Grey.Lighten4);
                            
                            table.Cell().Padding(4).PaddingLeft(12).Text("Operating Expenses");
                            table.Cell().Padding(4).Text(statement.TotalOperatingExpenses.ToString("N2")).AlignRight();

                            table.Cell().Padding(4).PaddingLeft(12).Text("Waivers and Write-Offs");
                            table.Cell().Padding(4).Text(statement.TotalWaiversAndWriteOffs.ToString("N2")).AlignRight();

                            table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Total Expenses").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(statement.TotalDeductions.ToString("N2")).SemiBold().AlignRight();

                            // Net Income
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text("Net Profit (Loss)").SemiBold().FontSize(12).FontColor(Colors.Black);
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(statement.NetIncome.ToString("N2")).SemiBold().FontSize(12).FontColor(Colors.Black).AlignRight();
                        });
                    });

                    page.Footer().Column(footer =>
                    {
                        if (!string.IsNullOrEmpty(qrCodeUrl))
                        {
                            try
                            {
                                var qrBase64 = _qrCodeService.GenerateQRCodeBase64(qrCodeUrl);
                                if (!string.IsNullOrEmpty(qrBase64))
                                {
                                    var qrBytes = Convert.FromBase64String(qrBase64);
                                    footer.Item().PaddingBottom(10).AlignLeft().Width(60).Height(60).Image(qrBytes);
                                }
                            }
                            catch { }
                        }
                        
                        footer.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] ExportToWordHtml<T>(IEnumerable<T> data, string title, string qrCodeUrl = null)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && 
                            !p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                            !(p.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)))
                .ToArray();
            
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'/><style>table { border-collapse: collapse; width: 100%; } th, td { border: 1px solid black; padding: 8px; text-align: left; } th { background-color: #f2f2f2; }</style></head><body>");
            sb.AppendLine($"<h2>{FormatDisplayName(title)}</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            foreach (var prop in properties)
            {
                sb.AppendLine($"<th>{FormatDisplayName(prop.Name)}</th>");
            }
            sb.AppendLine("</tr>");

            foreach (var item in data)
            {
                sb.AppendLine("<tr>");
                foreach (var prop in properties)
                {
                    var val = prop.GetValue(item);
                    string textVal = val is DateTime d ? d.ToString("yyyy-MM-dd") : 
                                     val is decimal dec ? dec.ToString("N2") :
                                     val is double db ? db.ToString("N2") :
                                     val is float fl ? fl.ToString("N2") :
                                     val?.ToString() ?? "";
                    sb.AppendLine($"<td>{textVal}</td>");
                }
                sb.AppendLine("</tr>");
            }

            if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                try
                {
                    var qrBase64 = _qrCodeService.GenerateQRCodeBase64(qrCodeUrl);
                    if (!string.IsNullOrEmpty(qrBase64))
                    {
                        sb.AppendLine($"<div style='text-align: left; margin-top: 30px;'><img src='data:image/png;base64,{qrBase64}' width='100' height='100' /><br/><small>Scan to Verify</small></div>");
                    }
                }
                catch { }
            }

            sb.AppendLine("</table></body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
