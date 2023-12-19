﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CEESP
{
    /// <summary>
    /// Interação lógica para Grafico_Fasorial.xam
    /// </summary>
    public partial class Grafico_Fasorial : Page
    {
        private MainWindow main;

        private List<String> times;
        private plot plot;
        private List<Line> oldLines;
        private bool autosizeEnable = true;
        private float zoomScale = 1;
        private bool spliterEnable = true;
        private bool deleteLastDado = false;
        private int index;
        private ColectedData dado;

        public Grafico_Fasorial(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.Width = ListData1.configData.getWidth();
            this.Height = ListData1.configData.getHeigth();

            Graph.Width = ListData1.configData.getWidth();
            Graph.Height = ListData1.configData.getHeigth();

            Page.Width = ListData1.configData.getWidth();
            Page.Height = ListData1.configData.getHeigth();

            Grid.Width = ListData1.configData.getWidth();
            Grid.Height = ListData1.configData.getHeigth();

            this.plot = new plot(ListData1.configData.getCenterX(), ListData1.configData.getCenterY(), ListData1.configData.getXs());

            this.times = new List<String>();

            if (autosizeEnable)
                AutosizeButton.Content = "A";
            else 
                AutosizeButton.Content = "M";

            InitializeTime(30, 6);
            Phase.SelectedIndex = 0;

            Salvar.Visibility = Visibility.Hidden;
            rtSalvar.Visibility = Visibility.Hidden;
            saveMode.Content = "Autosave";

            this.index = 0;
        }

        public void drawLines()
        {
            /*Remove linhas antigas*/
            if (oldLines != null)
            {
                foreach (Line i in oldLines)
                {
                    Graph.Children.Remove(i);
                }
            }

            int index = Phase.SelectedIndex;
            List<ColectedData> data = ListData1.colectedData;

            if (ListData1.configData.getModuloAtivo())
            {
                this.dado = data[data.Count - 1]; //Pega o ultimo dado coletado

                if (saveMode.Content.ToString() != "Autosave")
                {
                    // Remove o valor corrente
                    if (this.deleteLastDado)
                        ListData1.colectedData.Remove(dado);
                    this.deleteLastDado = true;
                }

            }
            else
            {
                this.dado = data[this.index];
            }

            if (autosizeEnable)
                AutoSizeValue(this.dado, index);

            List<Line> objects = new List<Line>

            {
                plot.createVa(this.dado.getVa(index) * this.zoomScale), //Adiciona Va
                plot.createIa(this.dado.getIa(index) * this.zoomScale, this.dado.getFP(index), this.dado.getFPType(index)), //Adiciona Ia
                plot.createXs(this.dado.getIa(index) * this.zoomScale,this.dado.getFP(index), this.dado.getFPType(index)), //Adiciona Xs
            };

            if (this.dado.getFP(index) != 0)
            {
                objects.Add(plot.createEa());
            }

            double FPv = this.dado.getFP(index);
            double angle = Math.Acos(FPv) * 180 / Math.PI;

            // Atuliza tabela de valores
            VaValue.Content = "Va: " + Math.Round(this.dado.getVa(index), ListData1.configData.getDecimals()).ToString() + "V";
            IaValue.Content = "Ia: " + Math.Round(this.dado.getIa(index), ListData1.configData.getDecimals()).ToString() + "∠" + Math.Round(angle, 1) + "º A";
            EaValue.Content = "Ea: " + Math.Round(this.dado.getEa(index), ListData1.configData.getDecimals()).ToString() + "V";
            XsIa.Content = "XsIa: " + Math.Abs(Math.Round((this.dado.getIa(index) * ListData1.configData.getXs()), ListData1.configData.getDecimals())).ToString() + "∠" + Math.Round(90 - angle, 1) + "º V";

            FPValue.Content = "Cos(φ): " + Math.Round(FPv, 2);

            type.Content = FPv.ToString() != "1" ? (this.dado.getFPType(index) == 'i') ? "Indutivo" : "Capacitivo" : (object)"Resistivo";

            // Adiciona as linhas
            foreach (Line i in objects)
            {
                Graph.Children.Add(i);
            }
            oldLines = objects;

            this.spliterEnable = true;
        }

        public void InitializeTime(int max, int diference)
        {
            times.Add("Pause");

            for (int i = 1; i < max; i += diference)
            {
                times.Add(i + "s");
            }

            CBTimes.ItemsSource = times;
        }

        public void AutoSizeValue(ColectedData valores, int index)
        {
            this.spliterEnable = false; // Garante que o Slider não seja alterado
            double zoom = 0;
            float X = 0;

            float angle = (float)Math.Acos(valores.getFP(index));
            if (valores.getFP(index) != 0)
            {
                X = valores.getVa(index) + ((valores.getFPType(index) == 'i') ? valores.getIa(index) * ListData1.configData.getXs() * (float)Math.Cos(1.5708 - angle) : 0);
            } else
            {
                X = valores.getVa(index);
            }

            if (X != 0)
                zoom = (this.Width - (2 * ListData1.configData.getCenterX())) / X;
            this.zoomScale = (float)zoom;

            Slider.Value = (zoom != null) ? zoom : 1;
            LabelZoom.Content = Math.Round(this.zoomScale, 1) + "x";
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.spliterEnable)
            {
                if (autosizeEnable)
                {
                    autosizeEnable = false;
                    AutosizeButton.Content = "M";
                }

                this.zoomScale = (float)Slider.Value;
                LabelZoom.Content = Math.Round(Slider.Value, 1) + "x";
                if (ListData1.colectedData.Count > 0)
                {
                    this.deleteLastDado = false;
                    drawLines();
                }
            }
        }

        public int getSelectedTime()
        {
            if (CBTimes.SelectedItem.ToString() == "Pause")
            {
                return 0;
            }
            else
            {
                try
                {
                    string v = CBTimes.SelectedValue.ToString();

                    int tempo;

                    return int.TryParse(v.Substring(0, v.Length - 1), out tempo) ? tempo : 2;
                }
                catch
                {
                    return 2;
                }
            }
        }

        private void CBTimes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimeSelected.Content = CBTimes.SelectedItem.ToString() == "Pause" ? "P" : (object?)CBTimes.SelectedItem.ToString();

            main.getGraficos().ActualizeTimeRefresh();
        }

        private void Phase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListData1.colectedData.Count != 0)
            {
                this.deleteLastDado = false;
                drawLines();
            }
        }

        private void AutosizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.autosizeEnable = !autosizeEnable;
            if (this.autosizeEnable)
                AutosizeButton.Content = "A";
            else
                AutosizeButton.Content = "M";

            this.deleteLastDado = false;
            drawLines();
        }

        public void changeMode(bool isModuleOption)
        {
            if (!isModuleOption)
            {
                CBTimes.Visibility = Visibility.Collapsed;
                Itens.Visibility = Visibility.Visible;
                TimeSelected.Visibility = Visibility.Collapsed;
                saveMode.Visibility = Visibility.Hidden;
                rtSaveMode.Visibility = Visibility.Hidden;

                this.deleteLastDado = false;
                drawLines();
            } else
            {
                CBTimes.Visibility = Visibility.Visible;
                Itens.Visibility = Visibility.Hidden;
                TimeSelected.Visibility = Visibility.Visible;
                saveMode.Visibility = Visibility.Visible;
                rtSaveMode.Visibility = Visibility.Visible;
            }

        }

        private void plusItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.index < ListData1.colectedData.Count - 1)
            {
                this.index++;
                ItemText.Text = "Item: " + (index + 1).ToString();
                drawLines();
            }
        }

        private void minusItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.index - 1 != -1)
            {
                this.index--;
                ItemText.Text = "Item: " + (index + 1).ToString();
                drawLines();
            }
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            ListData1.colectedData.Add(this.dado);
        }

        private void SaveMode_Click(object sender, RoutedEventArgs e)
        {
           if (saveMode.Content.ToString() == "Autosave")
           {
                Salvar.Visibility = Visibility.Visible;
                rtSalvar.Visibility = Visibility.Visible;
                saveMode.Content = "Manual";
                this.deleteLastDado = false;
           } else
            {
                saveMode.Content = "Autosave";
                Salvar.Visibility = Visibility.Hidden;
                rtSalvar.Visibility = Visibility.Hidden;
                this.deleteLastDado = true;
            }
        }

        public void setDeleteLastDado (bool type)
        {
            this.deleteLastDado = type;
        }
    }
}
