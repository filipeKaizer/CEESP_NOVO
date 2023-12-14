using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
        private float zoomScale = 1;

        public Grafico_Fasorial(MainWindow main)
        {
            InitializeComponent();
            this.main = main;


            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;

            Graph.Width = SystemParameters.WorkArea.Width;
            Graph.Height = SystemParameters.WorkArea.Height;

            Page.Width = SystemParameters.WorkArea.Width;
            Page.Height = SystemParameters.WorkArea.Height;

            Grid.Width = SystemParameters.WorkArea.Width;
            Grid.Height = SystemParameters.WorkArea.Height;

            this.plot = new plot(ListData1.configData.getCenterX(), ListData1.configData.getCenterY() / 2, ListData1.configData.getXs());

            this.times = new List<String>();

            InitializeTime(20, 4);
            Phase.SelectedIndex = 0;
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

            ColectedData valores = data[data.Count - 1]; //Pega o ultimo dado coletado

            List<Line> objects = new List<Line>

            {
                plot.createVa(valores.getVa(index) * (float)this.zoomScale), //Adiciona Va
                plot.createIa(valores.getIa(index) * (float)this.zoomScale, valores.getFP(index), valores.getFPType(index)), //Adiciona Ia
                plot.createXs(valores.getIa(index) * (float)this.zoomScale,valores.getFP(index), valores.getFPType(index)), //Adiciona Xs
                plot.createEa() //Adiciona Ea
            };

            // Atuliza tabela de valores
            VaValue.Content = "Va: " + Math.Round(valores.getVa(index), ListData1.configData.getDecimals()).ToString() + "V";
            IaValue.Content = "Ia: " + Math.Round(valores.getIa(index), ListData1.configData.getDecimals()).ToString() + "A";
            XsIa.Content = "XsIa: " + Math.Round((valores.getIa(index) * ListData1.configData.getXs()), ListData1.configData.getDecimals()).ToString() + "V";

            string FPv = Math.Round(valores.getFP(index), ListData1.configData.getDecimals()).ToString();
            FPValue.Content = "FP: " + FPv + ((FPv != "1") ? valores.getFPType(index) : 'r');

            // Adiciona as linhas
            foreach (Line i in objects)
            {
                Graph.Children.Add(i);
            }
            oldLines = objects;

        }

        public void InitializeTime(int max, int diference)
        {
            times.Add("Pause");

            for (int i = 2; i < max; i += diference)
            {
                times.Add(i + "s");
            }

            CBTimes.ItemsSource = times;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.zoomScale = (float)Slider.Value;
            LabelZoom.Content = Math.Round(Slider.Value, 1) + "x";
            if (ListData1.colectedData.Count > 0)
                drawLines();
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

                    if (int.TryParse(v.Substring(0, v.Length - 1), out tempo))
                        return tempo;
                    else
                        return 2;
                }
                catch
                {
                    return 2;
                }
            }
        }

        private void CBTimes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBTimes.SelectedItem.ToString() == "Pause")
                TimeSelected.Content = "P";
            else
                TimeSelected.Content = CBTimes.SelectedItem.ToString();

            main.getGraficos().ActualizeTimeRefresh();
        }

        private void Phase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListData1.colectedData.Count != 0)
                drawLines();
        }
    }
}
