using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TriageBackend.DTOs;

namespace TriageBackend.Utilities
{
    public class PdfGeneratorHistoryReport : IPdfGeneratorHistoryReport
    {
        public byte[] GenerateConsultationsPdf(IEnumerable<ConsultationReportDto> consultations)
        {
            var list = consultations?
                .OrderByDescending(c => c.FechaInicioConsulta)
                .ToList()
                ?? new List<ConsultationReportDto>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    page.Header()
                        .Text("Historial Clínico del Paciente")
                        .FontSize(18)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().PaddingVertical(8).Column(col =>
                    {
                        if (!list.Any())
                        {
                            col.Item().Text("No hay consultas para mostrar.").Italic();
                            return;
                        }

                        foreach (var c in list)
                        {
                            col.Item().Padding(8).Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.White)
                                
                                .Column(card =>
                                {
                                    card.Item().Text($"Consulta #{c.ConsultationId}")
                                        .FontSize(14).SemiBold();

                                    card.Item().Text($"Fecha: {c.FechaInicioConsulta:yyyy-MM-dd HH:mm}");

                                    if (c.FechaFinConsulta.HasValue)
                                        card.Item().Text($"Terminó: {c.FechaFinConsulta:yyyy-MM-dd HH:mm}");

                                    
                                    card.Item().Text($"Médico: {c.DoctorFullName ?? "Desconocido"}");

                                    card.Item().Text($"Estado: {c.EstadoId}  |  Triage: {c.TriageId}");

                                    card.Item().PaddingTop(5).Text(txt =>
                                    {
                                        txt.Span("Diagnóstico: ").SemiBold();

                                        if (c.DiagnosisName is null)
                                            txt.Span("Sin diagnóstico registrado");
                                        else
                                            txt.Span($"{c.DiagnosisName} ({c.DiagnosisObservation})");
                                    });

                                    if (c.TreatmentDescription is not null)
                                    {
                                        card.Item().PaddingTop(5).Text(txt =>
                                        {
                                            txt.Span("Tratamiento: ").SemiBold();
                                            txt.Span(c.TreatmentDescription);
                                        });
                                    }
                                    else
                                    {
                                        card.Item().PaddingTop(5).Text("Sin tratamiento registrado").Light();
                                    }
                                });

                            col.Item().PaddingBottom(6);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado: ").SemiBold();
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
                });
            });

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}
