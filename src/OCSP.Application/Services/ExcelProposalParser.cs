using System.Text.Json;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace OCSP.Application.Services
{
    /// <summary>
    /// Service to parse Excel proposal files and extract data from "Tổng hợp" tab only
    /// </summary>
    public class ExcelProposalParser
    {
        public class ParsedProposalData
        {
            public decimal TotalCost { get; set; }
            public int TotalDurationDays { get; set; }
            public string ProjectTitle { get; set; } = string.Empty;
            public Dictionary<string, object> GeneralInfo { get; set; } = new();
            public List<ProposalCostItemData> CostItems { get; set; } = new();
        }

        public class ProposalCostItemData
        {
            public string Category { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public decimal Quantity { get; set; } = 0;
            public decimal UnitPrice { get; set; } = 0;
            public decimal TotalAmount { get; set; } = 0;
            public int Order { get; set; } = 0;
            public string? Notes { get; set; }
        }

        public async Task<ParsedProposalData> ParseExcelAsync(Stream excelStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage(excelStream);
            var result = new ParsedProposalData();

            // Only parse "Tổng hợp" tab
            var tongHopWorksheet = package.Workbook.Worksheets.FirstOrDefault(w => 
                w.Name.Contains("Tổng hợp", StringComparison.OrdinalIgnoreCase) ||
                w.Name.Contains("Tong hop", StringComparison.OrdinalIgnoreCase));

            if (tongHopWorksheet == null)
            {
                throw new InvalidOperationException("Không tìm thấy tab 'Tổng hợp' trong file Excel");
            }

            // Parse general info and cost items from "Tổng hợp" tab
            await ParseGeneralInfo(tongHopWorksheet, result);

            return result;
        }

        private async Task ParseGeneralInfo(ExcelWorksheet worksheet, ParsedProposalData result)
        {
            var startRow = 1;
            var endRow = worksheet.Dimension?.End.Row ?? 100;
            var endCol = worksheet.Dimension?.End.Column ?? 10;

            var costItems = new List<ProposalCostItemData>();

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = 1; col <= endCol; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(cellValue)) continue;

                    // Parse project title (usually at the top)
                    if (row <= 5 && cellValue.Length > 10 && !cellValue.Contains(":"))
                    {
                        if (string.IsNullOrEmpty(result.ProjectTitle))
                        {
                            result.ProjectTitle = cellValue;
                        }
                    }

                    // Parse project information
                    if (cellValue.Contains("Diện tích xây dựng", StringComparison.OrdinalIgnoreCase))
                    {
                        var areaValue = worksheet.Cells[row, col + 1].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(areaValue))
                        {
                            result.GeneralInfo["ConstructionArea"] = areaValue;
                        }
                    }
                    else if (cellValue.Contains("Thời gian thi công", StringComparison.OrdinalIgnoreCase))
                    {
                        var durationValue = worksheet.Cells[row, col + 1].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(durationValue))
                        {
                            result.GeneralInfo["ConstructionTime"] = durationValue;
                            var cleanDuration = durationValue.Replace("tháng", "").Replace("ngày", "").Trim();
                            if (int.TryParse(cleanDuration, out var duration))
                            {
                                // Convert months to days (1 month = 30 days)
                                if (durationValue.Contains("tháng"))
                                {
                                    result.TotalDurationDays = duration * 30;
                                }
                                else
                                {
                                    result.TotalDurationDays = duration;
                                }
                            }
                        }
                    }
                    else if (cellValue.Contains("Số công nhân", StringComparison.OrdinalIgnoreCase))
                    {
                        var workersValue = worksheet.Cells[row, col + 1].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(workersValue))
                        {
                            result.GeneralInfo["NumberOfWorkers"] = workersValue;
                        }
                    }
                    else if (cellValue.Contains("Lương trung bình", StringComparison.OrdinalIgnoreCase))
                    {
                        var salaryValue = worksheet.Cells[row, col + 1].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(salaryValue))
                        {
                            result.GeneralInfo["AverageSalary"] = salaryValue;
                        }
                    }

                    // Parse cost items (rows that look like cost data)
                    if (row >= 9 && row <= 16 && col == 1 && IsCostItemRow(cellValue, worksheet, row, col))
                    {
                        Console.WriteLine($"Found cost item at row {row}: '{cellValue}'");
                        var costItem = ParseCostItemRow(worksheet, row, endCol);
                        if (costItem != null)
                        {
                            // Order is already set in ParseCostItemRow method
                            costItems.Add(costItem);
                            Console.WriteLine($"Added cost item: Order={costItem.Order}, Name='{costItem.Name}'");
                        }
                    }

                    // Parse total cost (TỔNG CỘNG)
                    if (cellValue.Contains("TỔNG CỘNG", StringComparison.OrdinalIgnoreCase) ||
                        cellValue.Contains("TONG CONG", StringComparison.OrdinalIgnoreCase))
                    {
                        // Look for total cost in adjacent cells
                        for (int i = col + 1; i <= Math.Min(col + 3, endCol); i++)
                        {
                            var totalValue = worksheet.Cells[row, i].Value?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(totalValue) && IsNumericValue(totalValue))
                            {
                                result.TotalCost = ParseNumericValue(totalValue);
                                break;
                            }
                        }
                    }
                }
            }

            result.CostItems = costItems.OrderBy(x => x.Order).ToList();
            
            // Debug logging
            Console.WriteLine($"Parsed {costItems.Count} cost items from Excel");
            Console.WriteLine("Before sorting:");
            foreach (var item in costItems)
            {
                Console.WriteLine($"Order {item.Order}: {item.Name} - {item.TotalAmount} VNĐ");
            }
            Console.WriteLine("After sorting:");
            foreach (var item in costItems.OrderBy(x => x.Order))
            {
                Console.WriteLine($"Order {item.Order}: {item.Name} - {item.TotalAmount} VNĐ");
            }
        }

        private bool IsCostItemRow(string cellValue, ExcelWorksheet worksheet, int row, int col)
        {
            // Skip if cell is empty
            if (string.IsNullOrEmpty(cellValue)) return false;
            
            // Skip if contains summary keywords
            var summaryKeywords = new[] { "TỔNG", "CỘNG", "Tổng", "Cộng", "TOTAL", "SUM", "Thời gian", "Thoi gian", "Dự án", "Project", "DỰ TOÁN", "CHI PHÍ" };
            if (summaryKeywords.Any(keyword => cellValue.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Check if this looks like a cost item (starts with number like "1. Phần móng", "2. Phần thô", etc.)
            if (cellValue.Length > 2 && (cellValue.StartsWith("1.") || cellValue.StartsWith("2.") || cellValue.StartsWith("3.") || 
                cellValue.StartsWith("4.") || cellValue.StartsWith("5.") || cellValue.StartsWith("6.") || 
                cellValue.StartsWith("7.") || cellValue.StartsWith("8.")))
            {
                // Check if column B (index 2) has a numeric value (cost amount)
                var costValue = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                return !string.IsNullOrEmpty(costValue) && IsNumericValue(costValue);
            }

            return false;
        }

        private ProposalCostItemData? ParseCostItemRow(ExcelWorksheet worksheet, int row, int endCol)
        {
            var item = new ProposalCostItemData();

            // Debug: Log all cell values in this row
            Console.WriteLine($"Row {row}: A='{worksheet.Cells[row, 1].Value}', B='{worksheet.Cells[row, 2].Value}', C='{worksheet.Cells[row, 3].Value}'");

            // Parse category/name (column A)
            var categoryValue = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(categoryValue)) return null;
            
            item.Category = categoryValue;
            item.Name = categoryValue;

            // Extract order from item name (e.g., "1. Phần móng" -> 1)
            var orderMatch = Regex.Match(item.Name, @"^(\d+)\.\s*(.+)$");
            Console.WriteLine($"Row {row}: Item name = '{item.Name}', Order match = {orderMatch.Success}");
            if (orderMatch.Success)
            {
                var originalName = item.Name;
                // Keep the original name with number prefix for ProposalService sorting
                // item.Name = orderMatch.Groups[2].Value.Trim(); // Don't remove number prefix
                if (int.TryParse(orderMatch.Groups[1].Value, out int order))
                {
                    item.Order = order;
                    Console.WriteLine($"Row {row}: Extracted order {order} from '{originalName}' -> keeping original name");
                }
                else
                {
                    item.Order = 999; // Default for items without valid order
                    Console.WriteLine($"Row {row}: Failed to parse order from '{originalName}', using default 999");
                }
            }
            else
            {
                item.Order = 999; // Default for items without order pattern
                Console.WriteLine($"Row {row}: No order pattern found in '{item.Name}', using default 999");
            }

            // Parse cost amount (column B) - this is the main cost value
            var costValue = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(costValue) && IsNumericValue(costValue))
            {
                item.TotalAmount = ParseNumericValue(costValue);
            }
            else
            {
                return null; // Must have cost amount
            }

            // Parse percentage (column C) - this is the percentage
            var percentageValue = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
            Console.WriteLine($"Row {row}: Percentage value from column C = '{percentageValue}'");
            if (!string.IsNullOrEmpty(percentageValue))
            {
                // Store percentage in Notes field
                item.Notes = percentageValue;
                Console.WriteLine($"Stored percentage in Notes: {item.Notes}");
            }

            // Return item if we have name and total amount
            return (!string.IsNullOrEmpty(item.Name) && item.TotalAmount > 0) ? item : null;
        }

        private bool IsNumericValue(string? value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            
            var cleanValue = CleanNumericString(value);
            return decimal.TryParse(cleanValue, out _);
        }

        private decimal ParseNumericValue(string value)
        {
            var cleanValue = CleanNumericString(value);
            return decimal.TryParse(cleanValue, out var result) ? result : 0;
        }

        private string CleanNumericString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            
            // Remove currency symbols, commas, and other non-numeric characters except dots
            var clean = value.Replace("VNĐ", "")
                           .Replace("VND", "")
                           .Replace("đ", "")
                           .Replace(",", "")
                           .Replace(" ", "")
                           .Replace("$", "")
                           .Replace("USD", "");
            
            return clean;
        }
    }
}