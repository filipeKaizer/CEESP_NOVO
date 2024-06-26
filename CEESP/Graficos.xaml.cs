﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CEESP
{
    /// <summary>
    /// Interação lógica para Graficos.xam
    /// </summary>
    public partial class Graficos : Page
    {
        private Grafico_Fasorial grafico_Fasorial;
        private Grafico_Temporal grafico_Temporal;

        private MainWindow main;
        private int tempo = 2;

        private DispatcherTimer timer;

        SolidColorBrush selectedColor;

        public Graficos(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            grafico_Fasorial = new Grafico_Fasorial(this.main);
            grafico_Temporal = new Grafico_Temporal(this.main);

            FrameFasorial.Navigate(this.grafico_Fasorial);
            FrameTemporal.Navigate(this.grafico_Temporal);

            this.Width = ListData1.configData.getWidth();
            this.Height = ListData1.configData.getHeigth();

            Page.Width = ListData1.configData.getWidth();
            Page.Height = ListData1.configData.getHeigth();

            Grid.Width = ListData1.configData.getWidth();
            Grid.Height = ListData1.configData.getHeigth();

            Graficos_View.Width = ListData1.configData.getWidth();
            Graficos_View.Height = ListData1.configData.getHeigth();

            FrameFasorial.Height = ListData1.configData.getHeigth();
            FrameFasorial.Width = ListData1.configData.getWidth();

            FrameTemporal.Width = ListData1.configData.getWidth();
            FrameTemporal.Height = ListData1.configData.getHeigth();

            TabFasorial.Width = ListData1.configData.getWidth();
            TabFasorial.Height = ListData1.configData.getHeigth();

            TabTemporal.Width = ListData1.configData.getWidth();
            TabTemporal.Height = ListData1.configData.getHeigth();

            Color baseColor = Color.FromArgb(102, 0, 0, 0);
            selectedColor = new SolidColorBrush(baseColor);

            selectGrafico(0); // Começa com o fasorial
        }



        public void selectGrafico(int index)
        {
            Graficos_View.SelectedIndex = index;

            if (index == 0)
            {
                selectFasores.Background = selectedColor;
                selectTemporal.Background = null;
            }
            else
            {
                selectFasores.Background = null;
                selectTemporal.Background = selectedColor;
            }
        }


        public void AutoRefreshInit()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(this.getTimeRefresh()); // Defina o intervalo inicial
            timer.Tick += async (sender, e) => atualiza();

            // Inicie o timer
            timer.Start();
        }


        public async void atualiza()
        {
            // Verifica o tamanho do ListData1
            this.main.saveCache();

            setProgressRingStatus(true);
            // Realiza leitura no serialCOM e atualiza o colectedData
            ColectedData novo;
            if (ListData1.configData.getModuloAtivo())
            {
                novo = await main.getSerial().readValues();

                if (novo != null)
                {
                    this.grafico_Fasorial.setDado(novo);
                    this.grafico_Fasorial.drawLines();
                }
                if (this.grafico_Fasorial.saveMode.Content.ToString() == "Autosave")
                {
                    ListData1.colectedData.Add(novo);
                    // Atualiza o dataView da classe dados
                    this.main.getDados().atualizaDados();

                    // Atualiza a lista de leitura do Grafico Temporal
                    if (ListData1.temporalData.Count > ListData1.configData.getMaxItems())
                        ListData1.temporalData.Remove(ListData1.temporalData[0]);

                    ListData1.temporalData.Add(novo);

                    // Atualiza o Graph
                    grafico_Temporal.atualizaGraph();

                }
            }

            setProgressRingStatus(false);
        }


        public void ActualizeTimeRefresh()
        {
            if (this.grafico_Fasorial.getSelectedTime() == 0)
            {
                if (timer != null)
                    this.timer.Stop();
            }
            else
            {
                if (timer != null)
                    timer.Interval = TimeSpan.FromSeconds(this.grafico_Fasorial.getSelectedTime());
                if (timer != null && !timer.IsEnabled)
                    timer.Start();

            }

            this.tempo = this.grafico_Fasorial.getSelectedTime();
        }

        private void setProgressRingStatus(bool active)
        {
            progressRing.IsActive = active;
        }

        public int getTimeRefresh()
        {
            return this.tempo;
        }

        public void setTimeRefresh(int time)
        {
            this.tempo = time;
        }

        private void selectFasores_Click(object sender, RoutedEventArgs e)
        {
            selectGrafico(0);
        }

        private void selectTemporal_Click(object sender, RoutedEventArgs e)
        {
            selectGrafico(1);
        }

        public Grafico_Fasorial getFasorial()
        {
            return this.grafico_Fasorial;
        }

        public Grafico_Temporal getTemporal()
        {
            return this.grafico_Temporal;
        }

        public bool timerExists()
        {
            return timer != null;
        }
    }
}
