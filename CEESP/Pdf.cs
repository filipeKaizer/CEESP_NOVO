using iTextSharp.text.pdf;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using static System.Resources.ResXFileRef;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Documents;

namespace CEESP
{
    internal class Pdf
    {
        String Url;
        String autor = "CTISM - UFSM";
        String discplina = "";

        private plot plotagem;

        private Canvas canvas;
        List<Line> oldlines;

        private bool isSingleSample = false;
        private bool addValueTable = false;
        private bool addTitleValue = false;
        private bool addAutorData = false;

        private int index = 0;

        private int inicioAmostra = 0;
        private int finalAmostra = 0;
        private int maxAmostra = 0;

        private DateTime data;
        private ColectedData dadoSelecionado;

        public Pdf()
        {
            this.data = DateTime.Now;

            if (this.autor.Length <= 0)
                this.autor = "CTISM - UFSM";

            this.canvas = new Canvas();
            this.canvas.Width = 700;
            this.canvas.Height = 400;

            this.plotagem = new plot((float)this.canvas.Width * 0.05f, (float)200 / 2, ListData1.configData.getXs());
        }

        public bool createFile()
        {
                // Documento
                Document doc = new Document(PageSize.A4);
                doc.SetMargins(25, 25, 25, 25);
                
                doc.AddCreationDate();

                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(this.Url, FileMode.Create));

                doc.Open();

                // Adiciona titulo
                doc.Add(getTitle());

                // Adiciona informações
                if (this.addAutorData)
                    doc.Add(getInfo());

                // Adiciona enunciado
                if (dadoSelecionado != null)
                    doc.Add(getEnunciado());

            // Adiciona a leitura selecionada
            if (dadoSelecionado != null)
            {
                doc.Add(getTituloDadoSelecionado());
                doc.Add(getGraficoFasorial(this.dadoSelecionado, calcularZoom(this.dadoSelecionado), this.index));
                if (this.addValueTable)
                    doc.Add(getValuesTable(this.dadoSelecionado));
            }

            if (!isSingleSample) {
                int value = 0;

                foreach (ColectedData i in ListData1.colectedData)
                {
                    if (value >= inicioAmostra && value <= finalAmostra)
                    {
                        if (this.addTitleValue)
                        {
                            doc.Add(getSubtitulo(i, value));
                        }
                        
                        doc.Add(getGraficoFasorial(i, calcularZoom(i), this.index));
                        if (this.addValueTable)
                            doc.Add(getValuesTable(i));
                    }

                    value++;
                }
            }

            doc.Close();

            return true;
        } 
        private iTextSharp.text.Paragraph getTituloDadoSelecionado()
        {
            iTextSharp.text.Paragraph titulo = new iTextSharp.text.Paragraph();
            titulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 11);
            titulo.Alignment = Element.ALIGN_CENTER;

            titulo.Add("\n\nDado selecionado");

            return titulo;
        }
        private iTextSharp.text.Image getGraficoFasorial(ColectedData dado, float zoomScale, int index)
        {
            if (oldlines != null)
            {
                foreach(Line i in oldlines)
                {
                    this.canvas.Children.Remove(i);
                }
            }


            // Criar grafico
            if (this.plotagem == null)
            {
                MessageBox.Show("Plot é null");
            }

            List<Line> objects = this.plotagem.createLines(dado, zoomScale, index);

            foreach (Line i in objects)
            {
                this.canvas.Children.Add(i);
            }

            this.oldlines = objects;

            this.canvas.Background = System.Windows.Media.Brushes.Transparent;
            // Criar um RenderTargetBitmap com as dimensões do Canvas
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)760, (int)170, 96d, 96d, PixelFormats.Pbgra32);

            // Renderizar o Canvas no RenderTargetBitmap
            canvas.Measure(new System.Windows.Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new Rect(new System.Windows.Size((int)canvas.Width, (int)canvas.Height)));
            renderBitmap.Render(canvas);

            // Criar um codificador PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Converter o PngBitmapEncoder em um iTextSharp.text.Image
            MemoryStream memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(bytes);


            return image;

        }
        private PdfPTable getValuesTable(ColectedData dado)
        {
            PdfPTable tabela = new PdfPTable(6);

            tabela.WidthPercentage = 70;
            tabela.HorizontalAlignment = Element.ALIGN_CENTER;

            // Adicionar cabeçalho
            tabela.AddCell("V");
            tabela.AddCell("Ia");
            tabela.AddCell("F");
            tabela.AddCell("FP");
            tabela.AddCell("Ea");
            tabela.AddCell("XsIa");

            // Adicionar valores
            tabela.AddCell(Math.Round(dado.getVa(this.index), 1).ToString() + "V");
            tabela.AddCell(Math.Round(dado.getIa(this.index), 2).ToString() + "A");
            tabela.AddCell(Math.Round(dado.getFrequency(), 1).ToString() + "Hz");
            tabela.AddCell(Math.Round(dado.getFP(this.index), 2).ToString() + dado.getFPType(this.index));
            tabela.AddCell(Math.Round(dado.getEa(this.index), 2).ToString() + "V");
            tabela.AddCell(Math.Round(dado.getIa(this.index) * ListData1.configData.getXs(), 2).ToString() + "V");

            return tabela;
        }
        private iTextSharp.text.Paragraph getSubtitulo(ColectedData dado, int num)
        {
            iTextSharp.text.Paragraph sub = new iTextSharp.text.Paragraph();
            
            sub.Font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 11);
            sub.Alignment = Element.ALIGN_CENTER;

            sub.Add($"\n\nLeitura {num} ({dado.getTempo()}s):\n");

            return sub;
        }
        private iTextSharp.text.Paragraph getEnunciado()
        {
            iTextSharp.text.Paragraph enun = new iTextSharp.text.Paragraph();

            enun.Font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 11);

            enun.Alignment = Element.ALIGN_JUSTIFIED;

            if (isSingleSample)
            {
                // é uma unica amostra
                enun.Add("O presente documento contem a listagem de uma única amostra obtida, sendo uma carga do tipo '" + dadoSelecionado.getFPType(0) + "' medida aos " + dadoSelecionado.getTempo() + " segundos e ênfase na " + this.getPhaseString());
            } else
            {
                enun.Add($"O presente documento contém a listagem das {this.finalAmostra - this.inicioAmostra} leituras selecionadas de um total de {ListData1.colectedData.Count} valores lidos, sendo detalhado o valor de tipo '{dadoSelecionado.getFPType(0)}', medido aos {dadoSelecionado.getTempo()} segundos e ênfase na " + this.getPhaseString());
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
        private iTextSharp.text.Paragraph getTitle()
        {
            iTextSharp.text.Paragraph titulo = new iTextSharp.text.Paragraph();

            titulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 18);

            titulo.Alignment = Element.ALIGN_CENTER;

            titulo.Add("Relatório\n\n");

            return titulo;
        }
        private float calcularZoom(ColectedData dado)
        {
            float zoom = 0;
            float X = 0;

            if (dado == null)
                return 1;

            float angle = (float)Math.Acos(dado.getFP(0));
            X = dado.getFP(0) != 0
                ? dado.getVa(0) + ((dado.getFPType(0) == 'i') ? dado.getIa(0) * ListData1.configData.getXs() * (float)Math.Cos(1.5708 - angle) : 0)
                : dado.getVa(0);
            if (X != 0)
                zoom = (float)(450) / X;

            return zoom;
        }
        public void setDadoSelecionado(ColectedData dadoSelecionado)
        {
            this.dadoSelecionado = dadoSelecionado;
        }

        public void setIsSingleSample(bool status)
        {
            this.isSingleSample = status;
        }

        public void setRangeValues(int inicio, int final)
        {
            this.inicioAmostra = inicio;
            this.finalAmostra = final;
        }

        public void setUrl(String Url)
        {
            this.Url = Url;
        }

        public void setAutor(String autor)
        {
            this.autor = autor;
        }

        public void setIndex(int index)
        {
            this.index = index;
        }

        public void setAddValueTable(bool status)
        {
            this.addValueTable = status;
        }

        public void setAddTitleValue(bool status)
        {
            this.addTitleValue = status;
        }
        
        public void setAddAutorData(bool status)
        {
            this.addAutorData = status;
        }

        private string getPhaseString()
        {
            if (this.index == 0)
                return "média das fases.";
            else if (this.index == 1)
                return "fase A.";
            else if (this.index == 2)
                return "fase B.";
            else
                return "fase C.";

        }
    }
}
