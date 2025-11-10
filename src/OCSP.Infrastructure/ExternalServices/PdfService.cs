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
            
            var homeownerName = $"{homeownerProfile.LastName ?? ""} {homeownerProfile.FirstName ?? ""} ".Trim();
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
            
            var contractorName = contractorCompany?.CompanyName ?? $"{contractorProfile.LastName ?? ""} {contractorProfile.FirstName ?? ""} ".Trim();
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

        public Task<byte[]> GenerateSupervisorContractPdfAsync(
            SupervisorContract contract,
            Profile homeownerProfile,
            Profile supervisorProfile,
            Project project,
            string? homeownerSignatureBase64 = null,
            string? supervisorSignatureBase64 = null)
        {
            if (homeownerProfile == null)
                throw new InvalidOperationException("Homeowner profile not found for supervisor contract PDF.");
            if (supervisorProfile == null)
                throw new InvalidOperationException("Supervisor profile not found for supervisor contract PDF.");
            if (project == null)
                throw new InvalidOperationException("Project not found for supervisor contract PDF.");

            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Load Vietnamese font
            PdfFont font;
            try
            {
                var calibriPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "calibri.ttf");
                if (File.Exists(calibriPath))
                {
                    font = PdfFontFactory.CreateFont(calibriPath);
                }
                else
                {
                    var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf");
                    if (File.Exists(fontPath))
                    {
                        font = PdfFontFactory.CreateFont(fontPath);
                    }
                    else
                    {
                        font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.TIMES_ROMAN);
                    }
                }
            }
            catch
            {
                font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.TIMES_ROMAN);
            }

            document.SetFont(font);

            // Header
            document.Add(new Paragraph("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(14));
            document.Add(new Paragraph("Độc lập – Tự do – Hạnh phúc")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetItalic()
                .SetFontSize(12));
            document.Add(new Paragraph("-----------***----------")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12));

            // Date
            var now = DateTime.Now;
            document.Add(new Paragraph($"{project.Address}, ngày {now.Day} tháng {now.Month} năm {now.Year}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(11)
                .SetMarginBottom(10));

            // Title
            document.Add(new Paragraph("HỢP ĐỒNG TƯ VẤN GIÁM SÁT")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(16));
            document.Add(new Paragraph("THI CÔNG XÂY DỰNG CÔNG TRÌNH")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(16)
                .SetMarginBottom(15));

            // Parties section
            document.Add(new Paragraph("Căn cứ kết quả lựa chọn Nhà thầu tại văn bản số (Quyết định số 4).")
                .SetFontSize(11)
                .SetMarginBottom(10));
            document.Add(new Paragraph($"Hôm nay, ngày {now.Day} tháng {now.Month} năm {now.Year} tại {project.Address}, chúng tôi gồm các bên dưới đây:")
                .SetFontSize(11)
                .SetMarginBottom(10));

            // 1. Chủ đầu tư
            document.Add(new Paragraph("1. Chủ đầu tư (viết tắt là CĐT):")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            var homeownerName = $"{homeownerProfile.LastName ?? ""} {homeownerProfile.FirstName ?? ""} ".Trim();
            if (string.IsNullOrEmpty(homeownerName)) homeownerName = "[Chưa cập nhật]";
            document.Add(new Paragraph($"Đại diện (hoặc người được ủy quyền) là: {homeownerName}")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph($"Địa chỉ: {homeownerProfile.Address ?? "[Chưa cập nhật]"}, {homeownerProfile.City ?? ""}")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph($"Điện thoại: {homeownerProfile.PhoneNumber ?? "[Chưa cập nhật]"}")
                .SetFontSize(11)
                .SetMarginLeft(15)
                .SetMarginBottom(10));

            // 2. Tư vấn giám sát
            document.Add(new Paragraph("và một bên")
                .SetFontSize(11)
                .SetMarginTop(5));
            document.Add(new Paragraph("2. Tư vấn giám sát thi công xây dựng công trình (viết tắt là TVGS):")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            var supervisorName = $"{supervisorProfile.LastName ?? ""} {supervisorProfile.FirstName ?? ""} ".Trim();
            if (string.IsNullOrEmpty(supervisorName)) supervisorName = "[Chưa cập nhật]";
            document.Add(new Paragraph($"Đại diện (hoặc người được ủy quyền) là: {supervisorName}")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph($"Địa chỉ: {supervisorProfile.Address ?? "[Chưa cập nhật]"}, {supervisorProfile.City ?? ""}")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph($"Điện thoại: {supervisorProfile.PhoneNumber ?? "[Chưa cập nhật]"}")
                .SetFontSize(11)
                .SetMarginLeft(15)
                .SetMarginBottom(10));

            document.Add(new Paragraph("là bên còn lại")
                .SetFontSize(11));
            document.Add(new Paragraph("Chủ đầu tư và TVGS được gọi riêng là Bên và gọi chung là Các Bên.")
                .SetFontSize(11)
                .SetMarginBottom(15));
            document.Add(new Paragraph("Các Bên tại đây thống nhất thỏa thuận như sau:")
                .SetFontSize(11)
                .SetMarginBottom(15));

            // ĐIỀU 1. MÔ TẢ PHẠM VI CÔNG VIỆC
            document.Add(new Paragraph("ĐIỀU 1. MÔ TẢ PHẠM VI CÔNG VIỆC")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph($"Chủ đầu tư đồng ý thuê và TVGS đồng ý nhận thực hiện các công việc giám sát thi công xây dựng cho công trình, hạng mục công trình {project.Name} (tên công trình, hạng mục công trình) hoặc cho gói thầu (tên, số gói thầu) thuộc dự án như sau:")
                .SetFontSize(11)
                .SetMarginBottom(10));

            // Giám sát chất lượng thi công
            document.Add(new Paragraph("- Giám sát chất lượng thi công xây dựng công trình:")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            AddBulletPoint(document, "+ Kiểm tra các điều kiện khởi công công trình xây dựng theo qui định của pháp luật;");
            AddBulletPoint(document, "+ Kiểm tra sự phù hợp năng lực của nhà thầu thi công xây dựng công trình với hồ sơ dự thầu và hợp đồng xây dựng, bao gồm:");
            AddBulletPoint(document, "+ Kiểm tra về nhân lực, thiết bị thi công của nhà thầu thi công xây dựng công trình đưa vào công trường.", 30);
            AddBulletPoint(document, "+ Kiểm tra hệ thống quản lý chất lượng của nhà thầu thi công xây dựng công trình.", 30);
            AddBulletPoint(document, "+ Kiểm tra giấy phép sử dụng các máy móc, thiết bị, vật tư có yêu cầu an toàn phục vụ thi công xây dựng công trình.", 30);
            AddBulletPoint(document, "+ Kiểm tra phòng thí nghiệm và các cơ sở sản xuất vật liệu, cấu kiện, sản phẩm xây dựng phục vụ thi công xây dựng của nhà thầu thi công xây dựng công trình.", 30);
            AddBulletPoint(document, "+ Kiểm tra và giám sát chất lượng vật tư, vật liệu và thiết bị lắp đặt vào công trình do nhà thầu thi công xây dựng công trình, nhà thầu cung cấp thiết bị thực hiện theo yêu cầu của thiết kế, bao gồm:");
            AddBulletPoint(document, "+ Kiểm tra giấy chứng nhận chất lượng của nhà sản xuất, kết quả thí nghiệm của các phòng thí nghiệm hợp chuẩn và kết quả kiểm định chất lượng thiết bị của các tổ chức được cơ quan nhà nước có thẩm quyền công nhận đối với vật liệu, cấu kiện, sản phẩm xây dựng, thiết bị lắp đặt cho công trình trước khi đưa vào công trình.", 30);
            AddBulletPoint(document, "+ Trường hợp nghi ngờ các kết quả kiểm tra chất lượng vật liệu, thiết bị lắp đặt vào công trình do nhà thầu thi công xây dựng, nhà thầu cung cấp thiết bị thực hiện thì TVGS báo cáo chủ đầu tư để tiến hành thực hiện kiểm tra trực tiếp vật tư, vật liệu và thiết bị lắp đặt vào công trình xây dựng.", 30);
            AddBulletPoint(document, "+ Kiểm tra và giám sát trong quá trình thi công xây dựng công trình, bao gồm:");
            AddBulletPoint(document, "+ Kiểm tra biện pháp thi công của nhà thầu thi công xây dựng công trình.", 30);
            AddBulletPoint(document, "+ Kiểm tra và giám sát thường xuyên có hệ thống quá trình nhà thầu thi công xây dựng công trình triển khai các công việc tại hiện trường. Kết quả kiểm tra đều phải ghi nhật ký giám sát của chủ đầu tư hoặc biên bản kiểm tra theo quy định.", 30);
            AddBulletPoint(document, "+ Xác nhận bản vẽ hoàn công.", 30);
            AddBulletPoint(document, "+ Nghiệm thu công trình xây dựng theo quy định của pháp luật về quản lý chất lượng công trình xây dựng (Nghị định số 03 NĐ-CP ngày 15 của Chính phủ về quản lý chất lượng công trình).", 30);
            AddBulletPoint(document, "+ Tập hợp, kiểm tra tài liệu phục vụ nghiệm thu công việc xây dựng, bộ phận công trình, giai đoạn thi công xây dựng, nghiệm thu thiết bị, nghiệm thu hoàn thành từng hạng mục công trình xây dựng và hoàn thành công trình xây dựng.", 30);
            AddBulletPoint(document, "+ Phát hiện sai sót, bất hợp lý về thiết kế để đề nghị chủ đầu tư điều chỉnh hoặc yêu cầu nhà thầu thiết kế điều chỉnh.", 30);
            AddBulletPoint(document, "+ Phối hợp với chủ đầu tư tổ chức kiểm định lại chất lượng bộ phận công trình, hạng mục công trình và công trình xây dựng khi có nghi ngờ về chất lượng.", 30);
            AddBulletPoint(document, "+ Phối hợp với chủ đầu tư và các bên liên quan giải quyết những vướng mắc, phát sinh trong thi công xây dựng công trình.", 30);

            document.Add(new Paragraph($"TVGS đảm bảo giám sát thi công công trình, hạng mục công trình, gói thầu {project.Name} (tên công trình, hạng mục công trình, gói thầu) thuộc dự án đúng thiết kế, đúng quy chuẩn, tiêu chuẩn xây dựng được áp dụng, bảo đảm công trình đạt chất lượng cao, khối lượng đầy đủ và chính xác, đúng tiến độ đã được duyệt; đảm bảo an toàn, vệ sinh môi trường và phòng chống cháy, nổ.")
                .SetFontSize(11)
                .SetMarginTop(10));

            // ĐIỀU 2. GIÁ HỢP ĐỒNG
            document.Add(new Paragraph("ĐIỀU 2. GIÁ HỢP ĐỒNG, TẠM ỨNG VÀ THANH TOÁN")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("- Giá hợp đồng:")
                .SetBold()
                .SetFontSize(11)
                .SetMarginTop(10));
            var priceText = contract.MonthlyPrice.ToString("N0");
            var priceInWords = ConvertNumberToWords(contract.MonthlyPrice);
            document.Add(new Paragraph($"+ Giá hợp đồng với số tiền là: {priceText} đồng (Bằng chữ: {priceInWords})")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph("+ Trong đó bao gồm chi phí để thực hiện toàn bộ các công việc.")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph("+ Những chi phí phát sinh theo thay đổi và điều chỉnh giá hợp đồng.")
                .SetFontSize(11)
                .SetMarginLeft(15)
                .SetMarginBottom(10));

            // ĐIỀU 3. THAY ĐỔI VÀ ĐIỀU CHỈNH GIÁ HỢP ĐỒNG
            document.Add(new Paragraph("ĐIỀU 3. THAY ĐỔI VÀ ĐIỀU CHỈNH GIÁ HỢP ĐỒNG")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            AddBulletPoint(document, "Chi phí phát sinh chỉ được tính nếu công việc của TVGS gia tăng phạm vi công việc theo yêu cầu của Chủ đầu tư;");
            AddBulletPoint(document, "Kéo dài công việc vì lý do từ phía CĐT hoặc các Nhà thầu xây lắp hoặc các Nhà cung cấp trong quá trình xây dựng Công trình. Thời gian kéo dài chỉ được tính bắt đầu sau 2 tháng kể từ ngày bàn giao công trình, hạng mục công trình theo tiến độ của Dự án đã được phê duyệt (hoặc được điều chỉnh phê duyệt).");
            document.Add(new Paragraph("Nếu những trường hợp trên phát sinh hoặc có xu hướng phát sinh, TVGS sẽ thông báo cho CĐT trước khi thực hiện công việc. Không có chi phí phát sinh nào được thanh toán trừ khi được CĐT chấp thuận bằng văn bản trước khi tiến hành công việc.")
                .SetFontSize(11)
                .SetMarginTop(5));

            // ĐIỀU 4. BẢO HIỂM
            document.Add(new Paragraph("ĐIỀU 4. BẢO HIỂM")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("Để tránh những rủi ro về trách nhiệm nghề nghiệp, TVGS phải mua bảo hiểm trách nhiệm nghề nghiệp theo qui định của pháp luật.")
                .SetFontSize(11));

            // ĐIỀU 5. PHẠT VI PHẠM HỢP ĐỒNG
            document.Add(new Paragraph("ĐIỀU 5. PHẠT VI PHẠM HỢP ĐỒNG")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("– Phạt vi phạm hợp đồng")
                .SetFontSize(11));
            document.Add(new Paragraph("– Đối với TVGS: Nếu do lỗi của TVGS làm chậm tiến độ nhưng tổng số tiền phạt không quá 12% phần giá trị hợp đồng vi phạm.")
                .SetFontSize(11)
                .SetMarginLeft(15));
            document.Add(new Paragraph("– Đối với Chủ đầu tư: Nếu không cung cấp kịp thời những tài liệu và thanh toán theo yêu cầu của tiến độ đã được xác định thì cũng sẽ bị phạt theo hình thức trên.")
                .SetFontSize(11)
                .SetMarginLeft(15));

            // ĐIỀU 6. QUYẾT TOÁN HỢP ĐỒNG
            document.Add(new Paragraph("ĐIỀU 6. QUYẾT TOÁN HỢP ĐỒNG")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("– Quyết toán hợp đồng")
                .SetFontSize(11));
            document.Add(new Paragraph("Trong vòng 10 ngày sau khi nhận được Biên bản xác nhận của Chủ đầu tư rằng TVGS đã hoàn thành tất cả các nghĩa vụ theo qui định của hợp đồng, TVGS sẽ trình cho Chủ đầu tư bộ dự thảo quyết toán hợp đồng với các tài liệu trình bày chi tiết theo mẫu mà Chủ đầu tư đã chấp thuận:")
                .SetFontSize(11)
                .SetMarginLeft(15));
            AddBulletPoint(document, "a) Giá trị của tất cả các công việc được làm theo đúng Hợp đồng", 30);
            AddBulletPoint(document, "b) Số tiền khác mà TVGS coi là đến hạn thanh toán theo Hợp đồng hoặc các thỏa thuận khác.", 30);
            document.Add(new Paragraph("Nếu Chủ đầu tư không đồng ý hoặc cho rằng TVGS chưa cung cấp đủ cơ sở để xác nhận một phần nào đó của dự thảo quyết toán hợp đồng, TVGS sẽ cung cấp thêm thông tin khi Chủ đầu tư có yêu cầu hợp lý và sẽ thay đổi dự thảo theo sự nhất trí của hai bên. TVGS sẽ chuẩn bị và trình cho Chủ đầu tư quyết toán hợp đồng như hai bên đã nhất trí.")
                .SetFontSize(11)
                .SetMarginTop(5));
            document.Add(new Paragraph("Tuy nhiên nếu sau khi có những cuộc thảo luận giữa các bên và bất kỳ thay đổi nào trong dự thảo quyết toán hợp đồng mà hai bên đã nhất trí, Chủ đầu tư sẽ thanh toán toàn bộ giá trị của phần này cho TVGS.")
                .SetFontSize(11)
                .SetMarginTop(5));

            // ĐIỀU 7. ĐIỀU KHOẢN CHUNG
            document.Add(new Paragraph("ĐIỀU 7. ĐIỀU KHOẢN CHUNG")
                .SetBold()
                .SetFontSize(12)
                .SetMarginTop(15));
            document.Add(new Paragraph("– Hai bên cam kết thực hiện đúng những điều đã quy định trong hợp đồng này")
                .SetFontSize(11));
            document.Add(new Paragraph($"– Hợp đồng này có hiệu lực kể từ ngày {now.Day}/{now.Month}/{now.Year}")
                .SetFontSize(11)
                .SetMarginBottom(30));

            // Signature section
            var signatureTable = new Table(2).UseAllAvailableWidth();
            signatureTable.AddCell(new Cell().Add(new Paragraph("ĐẠI DIỆN TVGS")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(12))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            signatureTable.AddCell(new Cell().Add(new Paragraph("ĐẠI DIỆN CHỦ ĐẦU TƯ")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(12))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            // Supervisor signature cell (left)
            var supervisorCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetHeight(100)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPaddingRight(90);
            
            if (!string.IsNullOrEmpty(supervisorSignatureBase64))
            {
                try
                {
                    var supervisorSigBytes = Convert.FromBase64String(supervisorSignatureBase64.Replace("data:image/png;base64,", ""));
                    var supervisorImg = ImageDataFactory.Create(supervisorSigBytes);
                    var supervisorImage = new Image(supervisorImg).ScaleToFit(120, 60);
                    supervisorImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT);
                    supervisorCell.Add(supervisorImage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding supervisor signature: {ex.Message}");
                    supervisorCell.Add(new Paragraph("\n\n\n\n"));
                }
            }
            else
            {
                supervisorCell.Add(new Paragraph("\n\n\n\n"));
            }
            signatureTable.AddCell(supervisorCell);

            // Homeowner signature cell (right)
            var homeownerCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetHeight(100)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPaddingRight(20);
            
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
                    Console.WriteLine($"Error adding homeowner signature: {ex.Message}");
                    homeownerCell.Add(new Paragraph("\n\n\n\n"));
                }
            }
            else
            {
                homeownerCell.Add(new Paragraph("\n\n\n\n"));
            }
            signatureTable.AddCell(homeownerCell);

            document.Add(signatureTable);
            document.Close();
            return Task.FromResult(ms.ToArray());
        }

        private void AddBulletPoint(Document document, string text, float leftMargin = 15)
        {
            document.Add(new Paragraph(text)
                .SetFontSize(11)
                .SetMarginLeft(leftMargin)
                .SetMarginTop(3));
        }

        private string ConvertNumberToWords(decimal number)
        {
            // Simplified number to words converter for Vietnamese
            // This is a basic implementation - you may want to use a more complete library
            if (number == 0) return "không";
            if (number < 1000) return number.ToString("N0");
            if (number < 1000000) return $"{number / 1000:N0} nghìn";
            if (number < 1000000000) return $"{number / 1000000:N0} triệu";
            return $"{number / 1000000000:N0} tỷ";
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


