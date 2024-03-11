using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Events;
using PdfSharp.Pdf.Advanced;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;

namespace CEESP
{
    /// <summary>
    /// Interação lógica para Relatorios.xam
    /// </summary>
    public partial class Relatorios : Page
    {
        private MainWindow main;
        private System.Windows.Media.Brush defaultColor;
        private List<ColectedData> dadosSelecionados;
        private ColectedData valorSelecionado;
        private List<Line> oldLines;
        private plot plot;
        private int index;
        private Pdf pdf;

        public Relatorios(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.Width = ListData1.configData.getWidth();
            this.Height = ListData1.configData.getHeigth();

            Page.Width = ListData1.configData.getWidth();
            Page.Height = ListData1.configData.getHeigth();

            Grid.Width = ListData1.configData.getWidth();
            Grid.Height = ListData1.configData.getHeigth();

            this.defaultColor = BorderSeguir.Background;

            this.dadosSelecionados = new List<ColectedData>();

            this.plot = new plot((float)this.Graph.Width * 0.1f, (float)this.Graph.Height / 2, ListData1.configData.getXs()); 

            this.index = 0;

            this.pdf = new Pdf();

            this.atualizaLista();
            this.showCargaSelecionada();
            drawLines();
        }

        private async void Seguir_Click(object sender, RoutedEventArgs e)
        {
            if (SelecionarLeitura.Visibility == Visibility.Visible)
            {
                if (this.valorSelecionado != null)
                {
                    this.pdf.setDadoSelecionado(this.valorSelecionado);

                    SelecionarLeitura.Visibility = Visibility.Hidden;

                    rangeSlider.Minimum = 0;
                    rangeSlider.Maximum = ListData1.colectedData.Count;
                    rangeSlider.UpperValue = rangeSlider.Maximum;

                    slider.Minimum = 0;
                    slider.Maximum = rangeSlider.Maximum;

                    SelecionarRange.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Selecione um valor para prosseguir.");
                }

            }
            else if (SelecionarRange.Visibility == Visibility.Visible)
            {
                if (checkNoValues.IsChecked == false && checkRange.IsChecked == false && checkMaxLimit.IsChecked == false)
                {
                    MessageBox.Show("Marque uma opção para prosseguir.");
                }
                else
                {
                    if (checkNoValues.IsChecked == true)
                        this.pdf.setIsSingleSample(true);

                    SelecionarRange.Visibility = Visibility.Hidden;
                    SelecionarDados.Visibility = Visibility.Visible;
                }

            } else {
                // Salvar as ultimas informações informações
                this.pdf.setAutor(Autor.Text.Length != 0 ? Autor.Text.ToString() : "CTISM - UFSM");

                // Abrir salvamento
                SaveFileDialog SaveWindow = new SaveFileDialog();
                SaveWindow.Filter = "Arquivo PDF (*.pdf)|*.pdf";
                SaveWindow.Title = "Escolher caminho do arquivo de dados";

                if (SaveWindow.ShowDialog() == true)
                {
                    // Documento
                    this.pdf.setUrl(SaveWindow.FileName);

                    bool status = this.pdf.createFile();

                    if (status) {
                        SelecionarLeitura.Visibility = Visibility.Visible;
                        SelecionarRange.Visibility = Visibility.Hidden;
                        SelecionarDados.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void Seguir_MouseEnter(object sender, RoutedEventArgs e)
        {
            BorderSeguir.Background = System.Windows.Media.Brushes.DarkCyan;
        }

        private void Seguir_MouseLeave(object sender, RoutedEventArgs e)
        {
            BorderSeguir.Background = defaultColor;
        }

        private void Carga_Click(object sender, RoutedEventArgs e)
        {
            // Atualiza o TBCarga
            TBCarga.Text = TBCarga.Text == "Indutivo" ? "Resistivo" : TBCarga.Text == "Resistivo" ? "Capacitivo" : "Indutivo";

            atualizaLista();

            // Seleciona o primeiro
            this.index = this.dadosSelecionados.Count > 0 ? 0 : -1;

            showCargaSelecionada();
            drawLines();
        }

        private void Proximo_Click(object sender, RoutedEventArgs e)
        {
            if (this.index + 1 <= dadosSelecionados.Count - 1)
            {
                this.index++;
                showCargaSelecionada();
                drawLines();
            }
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            if (this.index - 1 >= 0)
            {
                this.index--;
                showCargaSelecionada();
                drawLines();
            }

        }

        public void atualizaLista()
        {
            char type = 'i';
            // Carrega apenas os compativeis
            if (TBCarga.Text == "Resistivo")
               type = 'r';
            else
                type = (TBCarga.Text == "Indutivo") ? 'i' : 'c';


            dadosSelecionados.Clear();

            foreach (ColectedData dado in ListData1.colectedData)
            {
                if (dado.getFPType(0) == type)
                {
                    dadosSelecionados.Add(dado);
                }
            }

            this.index = 0;
            showCargaSelecionada();
        }

        private void showCargaSelecionada()
        {
            if (this.index == -1 || dadosSelecionados.Count == 0)
            {
                TBCargaSelecionada.Text = "Nenhuma carga encontrada";
            }
            else
            {
                ColectedData dado = dadosSelecionados[this.index];
                float Va = (float)Math.Round(dado.getVa(0), 0);
                float Ia = (float)Math.Round(dado.getIa(0), 0);
                float FP = (float)Math.Round(dado.getFP(0), 2);

                TBCargaSelecionada.Text = "Amostra " + (this.index + 1).ToString() + "/" + dadosSelecionados.Count.ToString() +
                                          " (Va: " + Va.ToString() + "V, Ia: "
                                          + Ia.ToString() + "A, FP: " + FP.ToString() + ")";
                valorSelecionado = dado;
            }
        }

        private void drawLines()
        {
            /*Remove linhas antigas*/
            if (oldLines != null)
            {
                foreach (Line i in oldLines)
                {
                    Graph.Children.Remove(i);
                }
            }

            if (valorSelecionado != null)
            {
                float zoom = (float)calcularZoom();

                List<Line> objects = new List<Line>
                {
                    plot.createVa(valorSelecionado.getVa(0) * zoom), //Adiciona Va
                    plot.createIa(valorSelecionado.getIa(0) * zoom, valorSelecionado.getFP(0), valorSelecionado.getFPType(0)), //Adiciona Ia
                    plot.createXs(valorSelecionado.getIa(0) * zoom,valorSelecionado.getFP(0), valorSelecionado.getFPType(0)), //Adiciona Xs
                };

                if (valorSelecionado.getFP(0) != 0)
                {
                    objects.Add(plot.createEa());
                }

                // Adiciona as linhas
                foreach (Line i in objects)
                {
                    Graph.Children.Add(i);
                }

                oldLines = objects;
            }
        }

        private double calcularZoom()
        {
            double zoom = 0;
            float X = 0;

            if (valorSelecionado == null)
                return 0;

            float angle = (float)Math.Acos(valorSelecionado.getFP(0));
            X = valorSelecionado.getFP(0) != 0
                ? valorSelecionado.getVa(0) + ((valorSelecionado.getFPType(0) == 'i') ? valorSelecionado.getIa(0) * ListData1.configData.getXs() * (float)Math.Cos(1.5708 - angle) : 0)
                : valorSelecionado.getVa(0);
            if (X != 0)
                zoom = (Graph.Width * 0.8f) / X;

            return zoom;
        }

        private void ProximoButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void checkRange_Checked(object sender, RoutedEventArgs e)
        {
            checkMaxLimit.IsChecked = false;
            checkNoValues.IsChecked = false;

            this.pdf.setIsSingleSample(false);

            TBMode.Visibility = Visibility.Visible;
            TBMode.Text = "Selecione os limites maximo e minimo abaixo";

            TBMaxValue.Visibility = Visibility.Visible;
            TBMinValue.Visibility = Visibility.Visible;
            rangeSlider.Visibility = Visibility.Visible;
            slider.Visibility = Visibility.Hidden;
        }

        private void checkNoCheck_Checked(object sender, RoutedEventArgs e)
        {
            checkMaxLimit.IsChecked = false;
            checkRange.IsChecked = false;

            this.pdf.setIsSingleSample(true);

            TBMode.Visibility = Visibility.Visible;
            TBMode.Text = "Clique em seguir";

            TBMaxValue.Visibility = Visibility.Hidden;
            TBMinValue.Visibility = Visibility.Hidden;
            rangeSlider.Visibility = Visibility.Hidden;
            slider.Visibility = Visibility.Hidden;

        }

        private void checkMaxLimit_Checked(object sender, RoutedEventArgs e)
        {
            checkRange.IsChecked = false;
            checkNoValues.IsChecked = false;

            this.pdf.setIsSingleSample(false);

            TBMode.Visibility = Visibility.Visible;
            TBMode.Text = "Selecione o mumero de leituras abaixo";

            TBMaxValue.Visibility = Visibility.Visible;
            TBMinValue.Visibility = Visibility.Hidden;
            rangeSlider.Visibility = Visibility.Hidden;
            slider.Visibility = Visibility.Visible;
        }

        private void rangeSlider_RangeSelectionChanged(object sender, MahApps.Metro.Controls.RangeSelectionChangedEventArgs<double> e)
        {
            TBMinValue.Text = rangeSlider.LowerValue.ToString();
            TBMaxValue.Text = rangeSlider.UpperValue.ToString();

            this.pdf.setRangeValues((int)rangeSlider.LowerValue, (int)rangeSlider.UpperValue);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TBMaxValue.Text = slider.Value.ToString();

            this.pdf.setRangeValues(0, (int)slider.Value);
        }

        private void Phase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (pdf != null)
                this.pdf.setIndex(Fase.SelectedIndex);
        }

        private void RBaddValues_Checked(object sender, RoutedEventArgs e)
        {
            this.pdf.setAddValueTable(true);
        }

        private void RBaddValues_Unchecked(object sender, RoutedEventArgs e)
        {
            this.pdf.setAddValueTable(false);
        }

        private void RBaddTitle_Checked(object sender, RoutedEventArgs e)
        {
            this.pdf.setAddTitleValue(true);
        }

        private void RBaddTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            this.pdf.setAddTitleValue(false);
        }

        private void RBaddAutorData_Unchecked(object sender, RoutedEventArgs e)
        {
            this.pdf.setAddAutorData(false);
        }

        private void RBaddAutorData_Checked(object sender, RoutedEventArgs e)
        {
            this.pdf.setAddAutorData(true);
        }
    }
}
