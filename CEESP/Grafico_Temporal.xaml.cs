﻿using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CEESP
{
    /// <summary>
    /// Interação lógica para Grafico_Temporal.xam
    /// </summary>
    public partial class Grafico_Temporal : Page
    {
        private MainWindow main;
        private Storyboard show_Checks;
        private Storyboard hide_Ckechs;

        bool isVisibleChecks = false;

        public Grafico_Temporal(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.Width = ListData1.configData.getWidth();
            this.Height = ListData1.configData.getHeigth();

            PlotGraph.Width = ListData1.configData.getWidth();
            PlotGraph.Height = ListData1.configData.getHeigth() - 40;

            Page.Width = ListData1.configData.getWidth();
            Page.Height = ListData1.configData.getHeigth();

            Grid.Width = ListData1.configData.getWidth();
            Grid.Height = ListData1.configData.getHeigth();

            this.show_Checks = (Storyboard)FindResource("show_Checks");
            this.hide_Ckechs = (Storyboard)FindResource("esconde_Checks");

            changeCheck();
        }


        private void changeCheck()
        {
            if (isVisibleChecks)
            {
                show_Checks.Stop();
                hide_Ckechs.Begin();
                isVisibleChecks = false;
            }
            else
            {
                hide_Ckechs.Stop();
                show_Checks.Begin();
                isVisibleChecks = true;
            }
        }


        public void atualizaGraph()
        {
            // Define o modelo do gráfico
            var plotModel = new PlotModel { Title = "" };

            //Muda coloração
            plotModel.PlotAreaBorderColor = OxyColor.FromRgb(160, 160, 160);
            plotModel.TitleColor = OxyColor.FromRgb(160, 160, 160);
            plotModel.TextColor = OxyColor.FromRgb(160, 160, 160);
            plotModel.SubtitleColor = OxyColor.FromRgb(160, 160, 160);
            plotModel.SelectionColor = OxyColor.FromRgb(160, 160, 160);

            /* CRIA OS TIPOS DE LINHAS */
            var VaLineSeries = new LineSeries
            {
                Color = OxyColors.Green,  // Cor da linha
                StrokeThickness = ListData1.configData.getLarguraLinha(),     // Espessura da linha
                MarkerType = MarkerType.Circle
            };

            var IaLineSeries = new LineSeries
            {
                Color = OxyColors.Red,  // Cor da linha
                StrokeThickness = ListData1.configData.getLarguraLinha(),    // Espessura da linha
                MarkerType = MarkerType.Circle
            };

            var EaLineSeries = new LineSeries
            {
                Color = OxyColors.Blue,  // Cor da linha
                StrokeThickness = ListData1.configData.getLarguraLinha(),    // Espessura da linha
                MarkerType = MarkerType.Circle
            };

            var RPMLineSeries = new LineSeries
            {
                Color = OxyColors.Yellow,  // Cor da linha
                StrokeThickness = ListData1.configData.getLarguraLinha(),   // Espessura da linha
                MarkerType = MarkerType.Circle
            };
            /*-------------------------*/

            if (ListData1.temporalData.Count == 0 && ListData1.colectedData.Count > 0)
                ListData1.temporalData = ListData1.colectedData;

            foreach (ColectedData valores in ListData1.temporalData)
            {
                if (valores != null)
                {
                    int t = valores.getTempo();

                    VaLineSeries.Points.Add(new DataPoint(t, Math.Round(valores.getVa(0), 2)));
                    IaLineSeries.Points.Add(new DataPoint(t, Math.Round(valores.getIa(0), 2)));
                    RPMLineSeries.Points.Add(new DataPoint(t, Math.Round(valores.getRPM(), 2)));
                    EaLineSeries.Points.Add(new DataPoint(t, Math.Round(valores.getEa(0), 2)));
                }
            }
            /*--------------------*/


            // Adicione a série ao PlotModel conforme CheckBox
            if (ListData1.colectedData.Count != 0)
            {
                if (VaCheckBox.IsChecked == true)
                    plotModel.Series.Add(VaLineSeries);

                if (IaCheck.IsChecked == true)
                    plotModel.Series.Add(IaLineSeries);

                if (RPM.IsChecked == true)
                    plotModel.Series.Add(RPMLineSeries);

                if (EaCheck.IsChecked == true)
                    plotModel.Series.Add(EaLineSeries);
            }
            // Associe o PlotModel ao PlotView
            PlotGraph.Model = plotModel;
        }

        private void VaCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void VaCheckBox_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void VaCheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            atualizaGraph();
        }

        private void btLegenda_Click(object sender, RoutedEventArgs e)
        {
            changeCheck();
        }
    }
}
