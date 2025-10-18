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
            var foundTotalRow = false; // Flag to stop parsing items after finding "TỔNG CỘNG"

            for (int row = startRow; row <= endRow; row++)
            {
                // Stop parsing cost items if we found "TỔNG CỘNG"
                if (foundTotalRow && row > startRow + 5) break;
                
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

                    // Parse cost items (rows that look like cost data) - dynamically detect all items
                    if (row >= 9 && col == 1 && !foundTotalRow && IsCostItemRow(cellValue, worksheet, row, col))
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
                        foundTotalRow = true; // Set flag to stop parsing more cost items
                        
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
            Console.WriteLine($"=== EXCEL PARSING SUMMARY ===");
            Console.WriteLine($"Total cost items found: {costItems.Count}");
            Console.WriteLine($"Project title: {result.ProjectTitle}");
            Console.WriteLine($"Total cost: {result.TotalCost:N0} VNĐ");
            Console.WriteLine($"Total duration: {result.TotalDurationDays} days");
            Console.WriteLine("Cost items (sorted by order):");
            foreach (var item in result.CostItems)
            {
                Console.WriteLine($"  {item.Order}. {item.Name} - {item.TotalAmount:N0} VNĐ ({item.Notes})");
            }
            Console.WriteLine($"=== END PARSING SUMMARY ===");
        }

        private bool IsCostItemRow(string cellValue, ExcelWorksheet worksheet, int row, int col)
        {
            // Skip if cell is empty
            if (string.IsNullOrEmpty(cellValue)) return false;
            
            // Skip if starts with summary keywords (only check exact matches at the beginning)
            var summaryKeywords = new[] { 
                "TỔNG CỘNG", "TONG CONG", "Tổng cộng", "Tong cong",
                "TỔNG HỢP", "TONG HOP", "Tổng hợp", "Tong hop", 
                "Hạng mục", "Chi phí", "Tỷ lệ",
                "Thời gian", "Thoi gian", "Dự án", "Project"
            };
            
            // Only check if the cell starts with these exact keywords
            foreach (var keyword in summaryKeywords)
            {
                if (cellValue.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // Check if this looks like a cost item (starts with number like "1. Phần móng", "2. Phần thô", etc.)
            // Use regex to match any number followed by a dot
            var itemPattern = @"^\d+\.\s*.+";
            if (Regex.IsMatch(cellValue, itemPattern))
            {
                // Allow items even if cost amount is missing (will be handled in ParseCostItemRow)
                // Just check if it looks like a valid item row
                return true;
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
                // If cost amount is missing, set to 0 but still create the item
                item.TotalAmount = 0;
                Console.WriteLine($"Row {row}: Cost amount missing or invalid, setting to 0");
            }

            // Parse percentage (column C) - this is the percentage
            var percentageValue = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
            Console.WriteLine($"Row {row}: Percentage value from column C = '{percentageValue}'");
            if (!string.IsNullOrEmpty(percentageValue))
            {
                // Convert percentage to proper format
                if (decimal.TryParse(percentageValue, out var percentageDecimal))
                {
                    // If it's a decimal like 0.09, convert to percentage like "9%"
                    if (percentageDecimal < 1)
                    {
                        item.Notes = $"{percentageDecimal * 100:F1}%";
                    }
                    else
                    {
                        // If it's already a percentage like 9, add % symbol
                        item.Notes = $"{percentageDecimal:F1}%";
                    }
                }
                else
                {
                    // If it's already a string like "9%", keep as is
                    item.Notes = percentageValue;
                }
                Console.WriteLine($"Stored percentage in Notes: {item.Notes}");
            }
            else
            {
                // If percentage is missing, set empty string
                item.Notes = "";
                Console.WriteLine($"Row {row}: Percentage missing, setting empty string");
            }

            // Return item if we have name (cost amount can be 0)
            return (!string.IsNullOrEmpty(item.Name)) ? item : null;
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