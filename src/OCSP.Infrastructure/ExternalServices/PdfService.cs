using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Image;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.ExternalServices.Interfaces;

namespace OCSP.Infrastructure.ExternalServices
{
    public class PdfService : IPdfService
    {
        public Task<byte[]> GenerateContractPdfAsync(
            Contract contract, 
            Profile homeownerProfile, 
            Profile contractorProfile,
            Contractor? contractorCompany,
            Proposal proposal,
            string? homeownerSignatureBase64 = null,
            string? contractorSignatureBase64 = null)
        {
            if (homeownerProfile == null)
    throw new InvalidOperationException("Homeowner profile not found for contract PDF.");

if (contractorProfile == null)
    throw new InvalidOperationException("Contractor profile not found for contract PDF.");

if (proposal == null)
    throw new InvalidOperationException("Proposal not found for contract PDF.");

            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Load Vietnamese font - try different approaches for better Unicode support
            PdfFont font;
            try
            {
                // Try to use a font that supports Vietnamese characters better
                // First try Calibri font (better Vietnamese support than Arial)
                var calibriPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "calibri.ttf");
                Console.WriteLine($"Checking Calibri font at: {calibriPath}");
                Console.WriteLine($"Calibri font exists: {File.Exists(calibriPath)}");
                
                if (File.Exists(calibriPath))
                {
                    font = PdfFontFactory.CreateFont(calibriPath);
                    Console.WriteLine("✅ Using Calibri font (excellent Vietnamese support)");
                }
                else
                {
                    // Fallback to Arial
                    var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf");
                    Console.WriteLine($"Checking Arial font at: {fontPath}");
                    Console.WriteLine($"Arial font exists: {File.Exists(fontPath)}");
                    
                    if (File.Exists(fontPath))
                    {
                        font = PdfFontFactory.CreateFont(fontPath);
                        Console.WriteLine("✅ Using Arial font (good Vietnamese support)");
                    }
                    else
                    {
                        throw new FileNotFoundException("No custom fonts found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating custom Arial font: {ex.Message}");
                try
                {
                    // Try Times-Roman with explicit Unicode handling
                    font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.TIMES_ROMAN);
                    Console.WriteLine("Using Times-Roman font (explicit Unicode)");
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Error creating Times-Roman font: {ex2.Message}");
                    // Final fallback - use Courier (monospace, better character support)
                    font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.COURIER);
                    Console.WriteLine("Using Courier font as final fallback");
                }
            }

            document.SetFont(font);

            // Header
            var header1 = new Paragraph("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(14);
            document.Add(header1);

            var header2 = new Paragraph("Độc lập - Tự do – Hạnh phúc")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetItalic()
                .SetFontSize(12);
            document.Add(header2);

            var separator = new Paragraph("-----------***----------")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12);
            document.Add(separator);

            // Title
            var title = new Paragraph("HỢP ĐỒNG THI CÔNG XÂY DỰNG NHÀ Ở")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(16)
                .SetMarginTop(20);
            document.Add(title);

            // Date
            var date = new Paragraph($"Hôm nay, ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12)
                .SetMarginBottom(20);
            document.Add(date);

            // Bên A - Homeowner
            document.Add(new Paragraph("BÊN THUÊ THI CÔNG XÂY DỰNG NHÀ Ở (gọi tắt là Bên A)")
                .SetBold()
                .SetFontSize(12));
            
            var homeownerName = $"{homeownerProfile.FirstName ?? ""} {homeownerProfile.LastName ?? ""}".Trim();
            if (string.IsNullOrEmpty(homeownerName)) homeownerName = "[Chưa cập nhật]";
            
            document.Add(new Paragraph($"Ông/bà: {homeownerName}")
                .SetFontSize(11));
            document.Add(new Paragraph($"Địa chỉ: {homeownerProfile.Address ?? "[Chưa cập nhật]"}, {homeownerProfile.City ?? ""}")
                .SetFontSize(11));
            document.Add(new Paragraph($"Điện thoại: {homeownerProfile.PhoneNumber ?? "[Chưa cập nhật]"}")
                .SetFontSize(11)
                .SetMarginBottom(15));

            // Bên B - Contractor
            document.Add(new Paragraph("BÊN NHẬN THI CÔNG XÂY DỰNG NHÀ Ở (gọi tắt là Bên B)")
                .SetBold()
                .SetFontSize(12));
            
            var contractorName = contractorCompany?.CompanyName ?? $"{contractorProfile.FirstName ?? ""} {contractorProfile.LastName ?? ""}".Trim();
            if (string.IsNullOrEmpty(contractorName)) contractorName = "[Chưa cập nhật]";
            
            document.Add(new Paragraph($"Ông/Bà/Công ty: {contractorName}")
                .SetFontSize(11));
            
            var contractorAddress = contractorCompany?.Address ?? contractorProfile.Address ?? "[Chưa cập nhật]";
            var contractorCity = contractorCompany?.City ?? contractorProfile.City ?? "";
            
            document.Add(new Paragraph($"Địa chỉ: {contractorAddress}, {contractorCity}")
                .SetFontSize(11));
            
            var contractorPhone = contractorCompany?.ContactPhone ?? contractorProfile.PhoneNumber ?? "[Chưa cập nhật]";
            document.Add(new Paragraph($"Điện thoại: {contractorPhone}")
                .SetFontSize(11)
                .SetMarginBottom(15));

            // Introduction
            document.Add(new Paragraph("Hai bên thỏa thuận ký hợp đồng này, trong đó, bên A đồng ý thuê bên B đảm nhận phần nhân công thi công xây dựng công trình nhà ở với các điều khoản như sau:")
                .SetFontSize(11)
                .SetMarginBottom(15));

            // Điều 1
            document.Add(new Paragraph("Điều 1: Nội dung công việc, Đơn giá, Tiến độ thi công, Trị giá hợp đồng")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(10));

            document.Add(new Paragraph("1. Nội dung công việc và đơn giá xây dựng")
                .SetBold()
                .SetFontSize(11));
            
            document.Add(new Paragraph("Bên A giao cho Bên B thi công trọn gói toàn bộ công trình nhà ở 2 tầng theo hồ sơ thiết kế được phê duyệt.")
                .SetFontSize(11));
            
            document.Add(new Paragraph("Hợp đồng trọn gói này bao gồm toàn bộ các hạng mục sau:")
                .SetFontSize(11));

            // Add contract items from proposal
            if (contract.Items != null && contract.Items.Count > 0)
            {
                foreach (var item in contract.Items)
                {
                    document.Add(new Paragraph($"- {item.Name}")
                        .SetFontSize(10)
                        .SetMarginLeft(20));
                }
            }
            else
            {
                document.Add(new Paragraph("- Thi công phần móng, phần thô, mái, hoàn thiện toàn bộ công trình;")
                    .SetFontSize(10)
                    .SetMarginLeft(20));
                document.Add(new Paragraph("- Cung cấp đầy đủ vật liệu, nhân công, máy móc thiết bị, vận chuyển và tổ chức thi công hoàn chỉnh;")
                    .SetFontSize(10)
                    .SetMarginLeft(20));
                document.Add(new Paragraph("- Lắp đặt hệ thống điện, nước, chống thấm, sơn bả, lát nền, ốp tường, cầu thang, ban công, bể phốt, bể nước ngầm, và các hạng mục hoàn thiện khác theo thiết kế.")
                    .SetFontSize(10)
                    .SetMarginLeft(20));
            }

            // Duration
            var durationMonths = (int)Math.Ceiling(contract.DurationDays / 30.0);
            document.Add(new Paragraph("4. Tiến độ thi công")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            document.Add(new Paragraph($"- Thời gian hoàn thành: Dự kiến trong {durationMonths} tháng ({contract.DurationDays} ngày) kể từ ngày khởi công.")
                .SetFontSize(11));
            document.Add(new Paragraph("- Nếu Bên B chậm tiến độ do lỗi chủ quan, sẽ bị phạt 05% giá trị hợp đồng tính trên phần khối lượng bị chậm.")
                .SetFontSize(11));

            // Price
            document.Add(new Paragraph("5. Trị giá hợp đồng")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            document.Add(new Paragraph($"- Tổng giá trị hợp đồng trọn gói là: {contract.TotalPrice:N0} VND")
                .SetFontSize(11)
                .SetBold());
            
            // Add contract items breakdown as table (using Proposal format)
            document.Add(new Paragraph("Giá trị này bao gồm:")
                .SetFontSize(11)
                .SetMarginTop(5));
            
            // Create table for items (Proposal format: Hạng mục, Chi phí, Tỷ lệ)
            var sortedItems = proposal.Items?.OrderBy(item => GetItemOrder(item.Name)).ToList() ?? new List<ProposalItem>();
            if (sortedItems.Any())
            {
                var table = new Table(3).UseAllAvailableWidth();
                
                // Add headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Hạng mục").SetBold().SetFontSize(10)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Chi phí (VNĐ)").SetBold().SetFontSize(10)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tỷ lệ (%)").SetBold().SetFontSize(10)));
                
                // Add items
                foreach (var item in sortedItems)
                {
                    var percentage = proposal.PriceTotal > 0 ? (item.Price / proposal.PriceTotal) * 100 : 0;
                    
                    table.AddCell(new Cell().Add(new Paragraph(item.Name).SetFontSize(9)));
                    table.AddCell(new Cell().Add(new Paragraph($"{item.Price:N0} VNĐ").SetFontSize(9)));
                    table.AddCell(new Cell().Add(new Paragraph($"{percentage:F1}%").SetFontSize(9)));
                }
                
                // Add total row
                table.AddCell(new Cell().Add(new Paragraph("TỔNG CỘNG").SetBold().SetFontSize(10)));
                table.AddCell(new Cell().Add(new Paragraph($"{proposal.PriceTotal:N0} VNĐ").SetBold().SetFontSize(10)));
                table.AddCell(new Cell().Add(new Paragraph("100.0%").SetBold().SetFontSize(10)));
                
                document.Add(table);
            }

            // Điều 2 - Trách nhiệm
            document.Add(new Paragraph("Điều 2: Trách nhiệm của các bên")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));

            document.Add(new Paragraph("1. Trách nhiệm của Bên A:")
                .SetBold()
                .SetFontSize(11));
            document.Add(new Paragraph("- Cung cấp bản vẽ kỹ thuật công trình;")
                .SetFontSize(10)
                .SetMarginLeft(15));
            document.Add(new Paragraph("- Cử người trực tiếp giám sát thi công về tiến độ, biện pháp kỹ thuật thi công về khối lượng và chất lượng;")
                .SetFontSize(10)
                .SetMarginLeft(15));

            document.Add(new Paragraph("2. Trách nhiệm của Bên B:")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            document.Add(new Paragraph("- Cung cấp đầy đủ vật tư, nguyên liệu, thiết bị thi công, máy móc cần thiết và đảm bảo chất lượng;")
                .SetFontSize(10)
                .SetMarginLeft(15));
            document.Add(new Paragraph("- Đảm bảo thi công an toàn tuyệt đối cho người và công trình;")
                .SetFontSize(10)
                .SetMarginLeft(15));
            document.Add(new Paragraph("- Chịu trách nhiệm bảo hành công trình trong thời hạn 6 tháng kể từ ngày nghiệm thu;")
                .SetFontSize(10)
                .SetMarginLeft(15));

            // Điều 3 - Thanh toán
            document.Add(new Paragraph("Điều 3: Thanh toán")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("- Xong phần xây thô và đổ mái được ứng 40%;")
                .SetFontSize(10)
                .SetMarginLeft(15));
            document.Add(new Paragraph("- Sau khi lát nền, sơn xong được thanh toán 50%;")
                .SetFontSize(10)
                .SetMarginLeft(15));
            document.Add(new Paragraph("- Khi công trình hoàn thành đưa vào sử dụng thanh toán 7% còn lại (sau khi trừ tiền bảo hành 3%);")
                .SetFontSize(10)
                .SetMarginLeft(15));

            // Điều 4 - Cam kết
            document.Add(new Paragraph("Điều 4: Cam kết")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("- Hợp đồng có giá trị từ ngày ký đến ngày thanh lý hợp đồng. Hai bên cam kết thực hiện đúng các điều khoản của hợp đồng;")
                .SetFontSize(11));
            document.Add(new Paragraph("- Hợp đồng được lập thành hai (02) bản có giá trị pháp lý như nhau. Mỗi bên giữ 01 bản để thực hiện.")
                .SetFontSize(11)
                .SetMarginBottom(30));

            // Signature section
            var signatureTable = new Table(2).UseAllAvailableWidth();
            signatureTable.AddCell(new Cell().Add(new Paragraph("ĐẠI DIỆN BÊN A")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(12))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            signatureTable.AddCell(new Cell().Add(new Paragraph("ĐẠI DIỆN BÊN B")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(12))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            // Add signature cells with images if available, or empty space
            // Homeowner signature cell (left) - aligned to RIGHT within left column
            var homeownerCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetHeight(100)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPaddingRight(90);
            
            if (!string.IsNullOrEmpty(homeownerSignatureBase64))
            {
                try
                {
                    var homeownerSigBytes = Convert.FromBase64String(homeownerSignatureBase64.Replace("data:image/png;base64,", ""));
                    var homeownerImg = ImageDataFactory.Create(homeownerSigBytes);
                    var homeownerImage = new Image(homeownerImg).ScaleToFit(120, 60);
                    homeownerImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT);
                    homeownerCell.Add(homeownerImage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding homeowner signature to table: {ex.Message}");
                    homeownerCell.Add(new Paragraph("\n\n\n\n"));
                }
            }
            else
            {
                homeownerCell.Add(new Paragraph("\n\n\n\n"));
            }
            signatureTable.AddCell(homeownerCell);

            // Contractor signature cell (right) - aligned to RIGHT within right column
            var contractorCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetHeight(100)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPaddingRight(20);
            
            if (!string.IsNullOrEmpty(contractorSignatureBase64))
            {
                try
                {
                    var contractorSigBytes = Convert.FromBase64String(contractorSignatureBase64.Replace("data:image/png;base64,", ""));
                    var contractorImg = ImageDataFactory.Create(contractorSigBytes);
                    var contractorImage = new Image(contractorImg).ScaleToFit(120, 60);
                    contractorImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT);
                    contractorCell.Add(contractorImage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding contractor signature to table: {ex.Message}");
                    contractorCell.Add(new Paragraph("\n\n\n\n"));
                }
            }
            else
            {
                contractorCell.Add(new Paragraph("\n\n\n\n"));
            }
            signatureTable.AddCell(contractorCell);

            document.Add(signatureTable);

            document.Close();
            return Task.FromResult(ms.ToArray());
        }

        public Task<byte[]> AddSignaturesToPdfAsync(
            byte[] pdfBytes, 
            string? homeownerSignatureBase64, 
            string? contractorSignatureBase64)
        {
            using var inputMs = new MemoryStream(pdfBytes);
            using var outputMs = new MemoryStream();
            
            using var reader = new PdfReader(inputMs);
            using var writer = new PdfWriter(outputMs);
            using var pdf = new PdfDocument(reader, writer);

            // ALWAYS add signatures to the LAST PAGE at FIXED POSITION FROM BOTTOM
            // This ensures signatures are placed correctly regardless of PDF length
            var totalPages = pdf.GetNumberOfPages();
            var page = pdf.GetLastPage();
            var pageSize = page.GetPageSize();
            
            Console.WriteLine($"📄 PDF has {totalPages} pages, adding signatures to last page");
            Console.WriteLine($"📏 Page size: {pageSize.GetWidth()} x {pageSize.GetHeight()}");

            // Use PdfCanvas to draw signatures
            var canvas = new PdfCanvas(page);

            // Calculate signature positions from BOTTOM of page (always fixed)
            // Signature table is at ~120px from bottom, signatures go ~20px below "ĐẠI DIỆN BÊN A/B"
            float signatureYFromBottom = 140; // Fixed distance from bottom
            float signatureHeight = 60;
            float signatureWidth = 120;

            // Add homeowner signature (left side)
            if (!string.IsNullOrEmpty(homeownerSignatureBase64))
            {
                try
                {
                    var homeownerSigBytes = Convert.FromBase64String(homeownerSignatureBase64.Replace("data:image/png;base64,", ""));
                    var homeownerImg = ImageDataFactory.Create(homeownerSigBytes);
                    
                    // Left side: 1/4 of page width, centered in left half
                    float homeownerX = (pageSize.GetWidth() / 4) - (signatureWidth / 2);
                    float homeownerY = signatureYFromBottom;
                    
                    canvas.AddImageFittedIntoRectangle(homeownerImg, 
                        new iText.Kernel.Geom.Rectangle(homeownerX, homeownerY, signatureWidth, signatureHeight), 
                        false);
                    
                    Console.WriteLine($"✅ Homeowner signature added at ({homeownerX:F1}, {homeownerY:F1}) on page {totalPages}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error adding homeowner signature: {ex.Message}");
                }
            }

            // Add contractor signature (right side)
            if (!string.IsNullOrEmpty(contractorSignatureBase64))
            {
                try
                {
                    var contractorSigBytes = Convert.FromBase64String(contractorSignatureBase64.Replace("data:image/png;base64,", ""));
                    var contractorImg = ImageDataFactory.Create(contractorSigBytes);
                    
                    // Right side: 3/4 of page width, centered in right half
                    float contractorX = (pageSize.GetWidth() * 3 / 4) - (signatureWidth / 2);
                    float contractorY = signatureYFromBottom;
                    
                    canvas.AddImageFittedIntoRectangle(contractorImg, 
                        new iText.Kernel.Geom.Rectangle(contractorX, contractorY, signatureWidth, signatureHeight), 
                        false);
                    
                    Console.WriteLine($"✅ Contractor signature added at ({contractorX:F1}, {contractorY:F1}) on page {totalPages}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error adding contractor signature: {ex.Message}");
                }
            }

            pdf.Close();
            return Task.FromResult(outputMs.ToArray());
        }

        private int GetItemOrder(string itemName)
        {
            var name = itemName.ToLower();
            if (name.Contains("móng") || name.Contains("foundation")) return 1;
            if (name.Contains("thô tầng 1") || name.Contains("rough floor 1")) return 2;
            if (name.Contains("thô tầng 2") || name.Contains("rough floor 2")) return 3;
            if (name.Contains("mái") || name.Contains("roof")) return 4;
            if (name.Contains("hoàn thiện") || name.Contains("finishing")) return 5;
            if (name.Contains("máy móc") || name.Contains("thiết bị") || name.Contains("equipment")) return 6;
            if (name.Contains("nhân công") || name.Contains("labor")) return 7;
            if (name.Contains("dự phòng") || name.Contains("reserve")) return 8;
            return 99;
        }
    }
}


