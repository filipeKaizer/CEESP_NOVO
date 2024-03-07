using iTextSharp.text.pdf;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows;

namespace CEESP
{
    internal class Pdf
    {
        String Url;
        String autor = "CTISM - UFSM";
        String discplina = "";

        bool isSingleSample = false;

        int inicioAmostra = 0;
        int finalAmostra = 0;

        DateTime data;
        ColectedData dadoSelecionado;


        public Pdf(String Url)
        {
            this.Url = Url;

            this.data = DateTime.Now;

            if (this.autor.Length <= 0)
                this.autor = "CTISM - UFSM";
        }

        public Task<bool> createFile()
        {
            return Task.Run(() =>
            {
                // Documento
                Document doc = new Document(PageSize.A4);
                doc.SetMargins(40, 40, 40, 40);
                doc.AddCreationDate();

                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(this.Url, FileMode.Create));

                doc.Open();

                // Adiciona titulo
                doc.Add(getTitle());

                // Adiciona informações
                doc.Add(getInfo());

                // Adiciona enunciado
                if (dadoSelecionado != null)
                    doc.Add(getEnunciado());

                doc.Close();
                return true;
            });
        } 

        private Paragraph getEnunciado()
        {
            Paragraph enun = new Paragraph();

            enun.Font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 11);

            enun.Alignment = Element.ALIGN_JUSTIFIED;

            if (isSingleSample)
            {
                // é uma unica amostra
                enun.Add("O presente documento contem a listagem de uma única amostra obtida, sendo uma carga do tipo '" + dadoSelecionado.getFPType(0) + "' medida aos " + dadoSelecionado.getTempo() + " segundos, como demonstrado abaixo.\n\n");
            } else
            {
                enun.Add($"O presente documento contém a listagem das {this.finalAmostra - this.inicioAmostra} leituras selecionadas de um total de {ListData1.colectedData.Count} valores lidos, com ênfase no valor de tipo '{dadoSelecionado.getFPType(0)}' e medido aos {dadoSelecionado.getTempo()} segundos. ");
            }

            return enun;
        }

        private PdfPTable getInfo()
        {
            PdfPTable info = new PdfPTable(3);
            info.WidthPercentage = 100;

            // Nome do autor
            PdfPCell autor = new PdfPCell(new Phrase("Autor: " + this.autor));
            autor.Border = PdfPCell.NO_BORDER;
            info.AddCell(autor);


            PdfPCell disciplina = new PdfPCell(new Phrase(this.discplina.Length != 0 ? "Disc.: " + this.discplina : ""));
            disciplina.Border = PdfPCell.NO_BORDER;
            disciplina.HorizontalAlignment = Element.ALIGN_CENTER;
            info.AddCell(disciplina);

            PdfPCell data = new PdfPCell(new Phrase("Data: " + this.data.ToString("dd/MM/yyyy\n\n\n")));
            data.Border = PdfPCell.NO_BORDER;
            data.HorizontalAlignment = Element.ALIGN_RIGHT;
            info.AddCell(data);

            return info;
        }

        private Paragraph getTitle()
        {
            Paragraph titulo = new Paragraph();

            titulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 18);

            titulo.Alignment = Element.ALIGN_CENTER;

            titulo.Add("Relatório\n\n");

            return titulo;
        }


        public void setDadoSelecionado(ColectedData dadoSelecionado)
        {
            this.dadoSelecionado = dadoSelecionado;
        }

        public void setIsSingleSample(bool status)
        {
            this.isSingleSample = status;
        }

        public void setIsSingleSample(int inicio, int final)
        {
            this.inicioAmostra = inicio;
            this.finalAmostra = final;
        }
    }
}
