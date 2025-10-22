using System;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using triage_backend.Utils;

namespace triage_backend.Services
{
    public class ReportService : IReportService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public byte[] GenerateTriageReport(string generatedBy)
        {
            return PdfHelper.CreatePdf((doc, pdf) =>
            {
                
                // CONFIGURACIÓN DE FUENTES Y COLORES
                
                var titleFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                Color primaryColor = new DeviceRgb(80, 139, 199); 
                Color secondaryColor = new DeviceRgb(230, 242, 255);  

                
                // ASEGURAR QUE EXISTA UNA PÁGINA
                
                if (pdf.GetNumberOfPages() == 0)
                {
                    pdf.AddNewPage(PageSize.A4);
                }

                var pageSize = pdf.GetDefaultPageSize();
                var canvas = new PdfCanvas(pdf.GetFirstPage());

               

                
                // ENCABEZADO CON LOGO Y TÍTULO
                var logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "logo_triage.jpg");
                if (!File.Exists(logoPath))
                    throw new FileNotFoundException($"No se encontró la imagen del logo en: {logoPath}");

                Image logo = new Image(ImageDataFactory.Create(logoPath))
                    .SetWidth(120)
                    .SetHeight(70)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);

                Paragraph title = new Paragraph("Reporte de Triage")
                    .SetFont(titleFont)
                    .SetFontSize(20)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetMargin(0)
                    .SetPaddingLeft(15);

                // Crear tabla de 2 columnas (logo y título)
                Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 3 }))
                    .UseAllAvailableWidth()
                    .SetMarginTop(10)
                    .SetMarginBottom(15)
                    .SetBackgroundColor(primaryColor);

                // Celda del logo
                Cell logoCell = new Cell().Add(logo)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(TextAlignment.LEFT);

                // Celda del título
                Cell titleCell = new Cell().Add(title)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetTextAlignment(TextAlignment.LEFT);

                // Agregar las celdas a la tabla
                headerTable.AddCell(logoCell);
                headerTable.AddCell(titleCell);

                doc.Add(headerTable);




                // INFORMACIÓN DE GENERACIÓN

                Paragraph info = new Paragraph(
                    $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                    $"Generado por: {generatedBy}")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.DARK_GRAY)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginTop(20)
                    .SetMarginBottom(15);

                doc.Add(info);

                
                // TABLA DE ACTIVIDAD
                
                doc.Add(new Paragraph("Resumen de Actividad del Personal de Enfermería")
                    .SetFont(titleFont)
                    .SetFontSize(13)
                    .SetFontColor(primaryColor)
                    .SetMarginBottom(8));

                string[] headers = {
                    "Enfermero",
                    "Duración Promedio (min)",
                    "Pacientes Atendidos",
                    "Duración Total (min)"
                };

                string[,] mockData =
                {
                    { "Enf. Camila Ruiz", "8.5", "18", (8.5 * 18).ToString("0.0") },
                    { "Enf. Carlos Mejía", "9.2", "22", (9.2 * 22).ToString("0.0") },
                    { "Enf. Diana López", "7.8", "25", (7.8 * 25).ToString("0.0") },
                    { "Enf. Felipe Castro", "10.0", "16", (10.0 * 16).ToString("0.0") },
                    { "Enf. Andrea Gómez", "8.9", "20", (8.9 * 20).ToString("0.0") }
                };

                Table table = new Table(UnitValue.CreatePercentArray(headers.Length)).UseAllAvailableWidth();

                // Encabezados
                foreach (var h in headers)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(h)
                        .SetFont(titleFont)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(10))
                        .SetBackgroundColor(primaryColor)
                        .SetPadding(5));
                }

                // Filas
                for (int i = 0; i < mockData.GetLength(0); i++)
                {
                    Color rowColor = (i % 2 == 0) ? secondaryColor : ColorConstants.WHITE;

                    for (int j = 0; j < mockData.GetLength(1); j++)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(mockData[i, j])
                            .SetFont(normalFont)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(rowColor)
                            .SetPadding(5));
                    }
                }

                doc.Add(table);

                
                // GRÁFICA DE BARRAS SIMULADA

                doc.Add(new Paragraph("\nDuración Promedio por Enfermero (minutos)")
                    .SetFont(titleFont)
                    .SetFontSize(12)
                    .SetFontColor(primaryColor)
                    .SetMarginTop(20)
                    .SetTextAlignment(TextAlignment.LEFT));

                float startX = 60;
                float startY = 300;
                float barHeight = 12;
                float maxBarWidth = 300;

                string[] enfermeros = { "C. Ruiz", "C. Mejía", "D. López", "F. Castro", "A. Gómez" };
                double[] valores = { 8.5, 9.2, 7.8, 10.0, 8.9 };
                double maxValue = 10.0;

                for (int i = 0; i < valores.Length; i++)
                {
                    float barWidth = (float)(valores[i] / maxValue * maxBarWidth);

                    canvas.SaveState()
                          .SetFillColor(primaryColor)
                          .Rectangle(startX, startY - i * 25, barWidth, barHeight)
                          .Fill()
                          .RestoreState();

                    doc.Add(new Paragraph($"{enfermeros[i]}: {valores[i]} min")
                        .SetFont(normalFont)
                        .SetFontSize(9)
                        .SetFixedPosition(startX + barWidth + 10, startY - i * 25, 200)
                        .SetFontColor(ColorConstants.DARK_GRAY));
                }


                // PIE DE PÁGINA 
                float pageBottom = 30; // distancia desde el borde inferior
                float pageCenter = pdf.GetDefaultPageSize().GetWidth() / 2;

                Paragraph footer = new Paragraph("Reporte generado automáticamente por Intelligent Triage © 2025")
                    .SetFont(normalFont)
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.CENTER);

                // Agregar el texto en posición absoluta
                new iText.Layout.Canvas(
                    new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.GetFirstPage()),
                    pdf.GetDefaultPageSize())
                    .ShowTextAligned(footer, pageCenter, pageBottom, iText.Layout.Properties.TextAlignment.CENTER);

            });
        }

        /// <summary>
        /// Retorna el nombre del archivo PDF con el nombre del usuario.
        /// </summary>
        public string GetReportFileName(string generatedBy)
        {
            string safeName = generatedBy.Replace(" ", "_");
            return $"Reporte_Triage_{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        }
    }
}
