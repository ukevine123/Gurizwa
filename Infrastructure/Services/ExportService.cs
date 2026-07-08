using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Application.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class ExportService : IExportService
    {
        public ExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
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
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(compose => 
                    {
                        compose.Text(title).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
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
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Black)
                                          .PaddingBottom(5).Text(prop.Name).SemiBold();
                                }
                            });

                            foreach (var item in data)
                            {
                                foreach (var prop in properties)
                                {
                                    var val = prop.GetValue(item);
                                    string textVal = val is DateTime d ? d.ToString("yyyy-MM-dd") : val?.ToString() ?? "";

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                         .PaddingVertical(5).Text(textVal);
                                }
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
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'/><style>table { border-collapse: collapse; width: 100%; } th, td { border: 1px solid black; padding: 8px; text-align: left; } th { background-color: #f2f2f2; }</style></head><body>");
            sb.AppendLine($"<h2>{title}</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            foreach (var prop in properties)
            {
                sb.AppendLine($"<th>{prop.Name}</th>");
            }
            sb.AppendLine("</tr>");

            foreach (var item in data)
            {
                sb.AppendLine("<tr>");
                foreach (var prop in properties)
                {
                    var val = prop.GetValue(item);
                    string textVal = val is DateTime d ? d.ToString("yyyy-MM-dd") : val?.ToString() ?? "";
                    sb.AppendLine($"<td>{textVal}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table></body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
