using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
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
        private ColectedData dado;

        private Storyboard show_salvar;
        private Storyboard hide_salvar;
        private Storyboard show_information;

        private float zoomScale = 1;

        private bool autosizeEnable = true;
        private bool spliterEnable = true;

        private int index;


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

            this.show_salvar = (Storyboard)FindResource("show_salvar");
            this.hide_salvar = (Storyboard)FindResource("hide_salvar");
            this.show_information = (Storyboard)FindResource("show_information");

            AutosizeButton.Content = autosizeEnable ? "A" : (object)"M";

            InitializeTime(30, 3);
            Phase.SelectedIndex = 0;

            saveMode.Content = "Autosave";

            this.index = 0;
        }

        public void drawLines()
        {
            this.clearGraph();

            int index = Phase.SelectedIndex;


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

            // Atualiza demonstração de falhas
            problems();

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

            times.Add(2 + "s");

            CBTimes.ItemsSource = times;
        }

        public void AutoSizeValue(ColectedData valores, int index)
        {
            this.spliterEnable = false; // Garante que o Slider não seja alterado
            double zoom = 0;
            float X = 0;

            float angle = (float)Math.Acos(valores.getFP(index));
            X = valores.getFP(index) != 0
                ? valores.getVa(index) + ((valores.getFPType(index) == 'i') ? valores.getIa(index) * ListData1.configData.getXs() * (float)Math.Cos(1.5708 - angle) : 0)
                : valores.getVa(index);

            if (X != 0)
                zoom = (this.Width - (2 * ListData1.configData.getCenterX())) / X;
            this.zoomScale = (float)zoom;

            Slider.Value = (zoom != 0) ? zoom : 1;
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
                if (this.dado != null)
                {
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
            if (this.dado != null)
            {
                drawLines();
            }
        }

        private void AutosizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.autosizeEnable = !autosizeEnable;
            AutosizeButton.Content = this.autosizeEnable ? "A" : (object)"M";

            if (this.dado != null)
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
            }
            else
            {
                CBTimes.Visibility = Visibility.Visible;
                Itens.Visibility = Visibility.Hidden;
                TimeSelected.Visibility = Visibility.Visible;
                saveMode.Visibility = Visibility.Visible;
                rtSaveMode.Visibility = Visibility.Visible;
            }

            ListData1.configData.setModuloAtivo(isModuleOption);

        }

        private void plusItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.index < ListData1.colectedData.Count - 1)
            {
                this.index++;
                ItemText.Text = "Item: " + (index + 1).ToString() + "/" + ListData1.colectedData.Count.ToString();

                this.setDado(ListData1.colectedData[this.index]);
                drawLines();
            }
        }

        private void minusItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.index - 1 != -1)
            {
                this.index--;
                ItemText.Text = "Item: " + (index + 1).ToString() + "/" + ListData1.colectedData.Count.ToString();

                this.setDado(ListData1.colectedData[this.index]);
                drawLines();
            }
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            TBInformation.Text = ListData1.colectedData.Count == 0 ? "Erro" : "Salvo";

            this.show_information.Stop();
            this.show_information.Begin();

            ListData1.colectedData.Add(this.dado);
            this.main.getDados().atualizaDados();
            this.main.getGraficos().getTemporal().atualizaGraph();
        }

        private void SaveMode_Click(object sender, RoutedEventArgs e)
        {
            if (saveMode.Content.ToString() == "Autosave")
            {
                this.hide_salvar.Stop();
                this.show_salvar.Begin();
                saveMode.Content = "Manual";
            }
            else
            {
                this.show_salvar.Stop();
                this.hide_salvar.Begin();
                saveMode.Content = "Autosave";
            }
        }

        public void setDado(ColectedData dado)
        {
            this.dado = dado;

        }

        public void clearGraph()
        {
            if (oldLines != null)
            {
                foreach (Line i in oldLines)
                {
                    Graph.Children.Remove(i);
                }
            }
        }

        private void problems()
        {
            FaseA_Fault.Visibility = Visibility.Hidden;
            FaseB_Fault.Visibility = Visibility.Hidden;
            FaseC_Fault.Visibility = Visibility.Hidden;

            // Falta de fases
            for (int i = 1; i < 4; i++)
            {
                bool problem = this.dado.getPhaseFail(i);
                if (problem && i == 1)
                    FaseA_Fault.Visibility = Visibility.Visible;

                if (problem && i == 2)
                    FaseB_Fault.Visibility = Visibility.Visible;

                if (problem && i == 3)
                    FaseC_Fault.Visibility = Visibility.Visible;
            }


        }
    }
}
