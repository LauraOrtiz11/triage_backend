using System;
using System.IO;
using triage_backend.Dtos;
using triage_backend.Repositories;
using triage_backend.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Borders;
using IOPath = System.IO.Path;

namespace triage_backend.Services
{
    public class ReportService : IReportService
    {
        private readonly ReportRepository _reportRepository;

        public ReportService(ReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public byte[] GenerateTriageReport(string userName, DateTime startDate, DateTime endDate)
        {
            var stats = _reportRepository.GetTriageStats(startDate, endDate);
            var dateRange = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";

            return PdfHelper.CreatePdf((doc, pdf) =>
            {
                // =================== FUENTES ===================
                PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont fontItalic = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                // =================== COLORES ===================
                var softGreen = new DeviceRgb(220, 245, 221);  
                var headerGreen = new DeviceRgb(184, 232, 187);
                var darkGreen = new DeviceRgb(0, 100, 0);
                var grayText = new DeviceRgb(60, 60, 60);
                var pastelBlue = new DeviceRgb(173, 216, 230);
                // ====== FONDO DE TODA LA PÁGINA ======
                if (pdf.GetNumberOfPages() == 0)
                {
                    pdf.AddNewPage();
                }

                var page = pdf.GetPage(1);
                var pageSize = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                canvas.SaveState()
                      .SetFillColor(softGreen)
                      .Rectangle(pageSize.GetLeft(), pageSize.GetBottom(), pageSize.GetWidth(), pageSize.GetHeight())
                      .Fill()
                      .RestoreState();



                // ====== ENCABEZADO (logo izquierda, título derecha) ======
                var headerTable = new Table(new float[] { 80, 1 }).UseAllAvailableWidth();
                var logoCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(10);
                string imagePath = IOPath.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                if (File.Exists(imagePath))
                {
                    var img = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(imagePath))
                        .ScaleToFit(60, 60)
                        .SetAutoScale(true);
                    logoCell.Add(img);
                }
                headerTable.AddCell(logoCell);

                var titleCell = new Cell().SetBorder(Border.NO_BORDER);
                var titleBlock = new Paragraph("REPORTE DE TIEMPOS PROMEDIO DE ATENCIÓN")
                    .SetFont(fontBold)
                    .SetFontSize(16)
                    .SetFontColor(darkGreen)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMargin(0)
                    .SetPadding(8)
                    .SetBackgroundColor(headerGreen);
                titleCell.Add(titleBlock);
                headerTable.AddCell(titleCell);
                doc.Add(headerTable);

                doc.Add(new Paragraph("").SetMarginBottom(12));

                // ====== DATOS GENERALES ======
                doc.Add(new Paragraph($"Generado por: {userName}")
                    .SetFont(fontRegular).SetFontColor(grayText).SetFontSize(11).SetMarginBottom(2));
                doc.Add(new Paragraph($"Fecha y hora de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFont(fontRegular).SetFontColor(grayText).SetFontSize(11).SetMarginBottom(2));
                doc.Add(new Paragraph($"Rango de fechas analizado: {dateRange}")
                    .SetFont(fontRegular).SetFontColor(grayText).SetFontSize(11).SetMarginBottom(10));

                // ====== MENSAJE SIN DATOS ======
                if (stats.AvgWaitTime == 0 && stats.AvgAttentionTime == 0 && stats.TotalTriageTime == 0)
                {
                    doc.Add(new Paragraph("No existen datos disponibles para el rango seleccionado.")
                        .SetFont(fontBold).SetFontColor(new DeviceRgb(150, 0, 0))
                        .SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));
                    DrawFooter(doc, pdf, darkGreen, fontItalic);
                    return;
                }

                // ====== TABLA ======
                var table = new Table(new float[] { 3, 1 })
                    .UseAllAvailableWidth()
                    .SetMarginTop(10)
                    .SetMarginBottom(16);

                table.AddHeaderCell(HeaderCell("Indicador", pastelBlue, fontBold));
                table.AddHeaderCell(HeaderCell("Valor (min)", pastelBlue, fontBold));

                table.AddCell(MetricCell("Tiempo promedio de espera antes de ser atendido", fontRegular));
                table.AddCell(ValueCell(stats.AvgWaitTime, fontRegular));

                table.AddCell(MetricCell("Tiempo promedio de duración de la atención", fontRegular));
                table.AddCell(ValueCell(stats.AvgAttentionTime, fontRegular));

                table.AddCell(MetricCell("Tiempo total promedio del proceso de triage", fontRegular));
                table.AddCell(ValueCell(stats.TotalTriageTime, fontRegular));

                doc.Add(table);

                // ====== GRÁFICO DE BARRAS ======
                doc.Add(new Paragraph("Visualización comparativa de tiempos")
                    .SetFont(fontBold).SetFontSize(12).SetMarginBottom(8).SetTextAlignment(TextAlignment.CENTER));

                var chart = new Table(new float[] { 3, 1, 5 }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
                double max = Math.Max(stats.AvgWaitTime, Math.Max(stats.AvgAttentionTime, stats.TotalTriageTime));
                if (max <= 0) max = 1;

                void AddBar(string label, double value, DeviceRgb color)
                {
                    chart.AddCell(new Cell().Add(new Paragraph(label).SetFont(fontRegular).SetFontSize(10))
                        .SetBorder(Border.NO_BORDER).SetPadding(2));
                    chart.AddCell(new Cell().Add(new Paragraph($"{value:F2}"))
                        .SetTextAlignment(TextAlignment.RIGHT).SetFont(fontRegular)
                        .SetFontSize(10).SetBorder(Border.NO_BORDER).SetPadding(6));

                    // barra más grande
                    float percent = (float)(value / max * 100.0);
                    var barContainer = new Div()
                        .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                        .SetHeight(25) // <-- altura aumentada
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBorder(new SolidBorder(ColorConstants.WHITE, 0.5f));

                    var bar = new Div()
                        .SetBackgroundColor(color)
                        .SetHeight(25) // <-- altura aumentada
                        .SetWidth(UnitValue.CreatePercentValue(percent));

                    barContainer.Add(bar);
                    chart.AddCell(new Cell().Add(barContainer).SetBorder(Border.NO_BORDER).SetPaddingTop(5).SetPaddingBottom(5));
                }

                AddBar("Espera promedio", stats.AvgWaitTime, new DeviceRgb(122, 186, 120));
                AddBar("Duración promedio", stats.AvgAttentionTime, new DeviceRgb(83, 160, 81));
                AddBar("Total triage", stats.TotalTriageTime, new DeviceRgb(50, 120, 50));

                doc.Add(chart);
                doc.Add(new Paragraph("").SetMarginTop(20));

                // ====== PIE DE PÁGINA ======
                DrawFooter(doc, pdf, darkGreen, fontItalic);
            });
        }

        // --- Helpers de tabla y footer ---
        private static Cell HeaderCell(string text, DeviceRgb bg, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font))
                .SetBackgroundColor(bg)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6);
        }

        private static Cell MetricCell(string text, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetPadding(6);
        }

        private static Cell ValueCell(double value, PdfFont font)
        {
            return new Cell().Add(new Paragraph(value.ToString("F2")).SetFont(font))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6);
        }

        private static void DrawFooter(Document doc, PdfDocument pdf, DeviceRgb lineColor, PdfFont italicFont)
        {
            var pageNum = pdf.GetNumberOfPages();
            var page = pdf.GetPage(pageNum);
            Rectangle ps = page.GetPageSize();
            var canvas = new PdfCanvas(page);
            canvas.SetStrokeColor(lineColor)
                  .SetLineWidth(1)
                  .MoveTo(ps.GetLeft() + 36, ps.GetBottom() + 36)
                  .LineTo(ps.GetRight() - 36, ps.GetBottom() + 36)
                  .Stroke();

            var footer = new Paragraph("Reporte generado automáticamente por el sistema Intelligent Triage")
                .SetFont(italicFont)
                .SetFontSize(9)
                .SetFontColor(lineColor);

            doc.ShowTextAligned(footer, ps.GetWidth() / 2, ps.GetBottom() + 26, pageNum,
                TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
        }

        public string GetReportFileName(string userName)
        {
            return $"Reporte_Triage_{userName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        }
    }
}
