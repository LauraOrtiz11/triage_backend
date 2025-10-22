using System;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;

namespace triage_backend.Utils
{
    public static class PdfHelper
    {
        public static byte[] CreatePdf(Action<Document, PdfDocument> buildAction)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(60, 50, 50, 50);

                buildAction?.Invoke(document, pdf);

                document.Close();
                return ms.ToArray();
            }
        }

        public static Table CreateTable(string[] headers, string[,] data)
        {
            Table table = new Table(headers.Length)
                .UseAllAvailableWidth()
                .SetMarginTop(15)
                .SetMarginBottom(15);

            var headerBg = new DeviceRgb(0, 102, 204);
            var white = ColorConstants.WHITE;
            var black = ColorConstants.BLACK;

            PdfFont fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            PdfFont fontRegular = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            foreach (var header in headers)
            {
                Cell cell = new Cell()
                    .Add(new Paragraph(header).SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(headerBg)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(6);
                table.AddCell(cell);
            }

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    Cell cell = new Cell()
                        .Add(new Paragraph(data[i, j]).SetFont(fontRegular).SetFontColor(black))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(6);
                    table.AddCell(cell);
                }
            }

            return table;
        }

        public static void AddCenteredImage(Document doc, string imagePath, float width, float height)
        {
            if (File.Exists(imagePath))
            {
                Image img = new Image(ImageDataFactory.Create(imagePath))
                    .ScaleToFit(width, height)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                doc.Add(img);
            }
        }
    }
}
