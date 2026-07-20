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

        public ExportService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
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
                            !p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
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

        public byte[] ExportToPdf<T>(IEnumerable<T> data, string title)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && 
                            !p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(FormatDisplayName(title)).SemiBold().FontSize(22).FontColor(Colors.Blue.Darken3);
                            col.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "guriza_logo.png");
                        if (File.Exists(logoPath))
                        {
                            row.ConstantItem(120).Image(logoPath);
                        }
                    });

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

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] ExportToWordHtml<T>(IEnumerable<T> data, string title)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && 
                            !p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
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

            sb.AppendLine("</table></body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
