using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CEESP
{
    /// <summary>
    /// Interação lógica para Inicio.xam
    /// </summary>
    public partial class Inicio : Page
    {
        private MainWindow main;
        private float XsValue;

        private Storyboard show_Xs;
        private Storyboard show_Ports;

        bool portSelected = false;

        public Inicio(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.Width = main.getWidth();
            this.Height = main.getHeigth();

            this.show_Xs = (Storyboard)FindResource("show_Xs");
            this.show_Ports = (Storyboard)FindResource("show_ports");
        }



        public void setProgress(string texto, float progresso, bool ativo)
        {
            if (ativo)
            {
                progress.Visibility = Visibility.Visible;
                verbose.Visibility = Visibility.Visible;

                //Adiciona valores
                progress.Value = progresso;
                verbose.Content = texto;
            }
            else
            {
                progress.Visibility = Visibility.Hidden;
                verbose.Visibility = Visibility.Hidden;
            }
        }


        public float getXs()
        {
            return this.XsValue;
        }

        private async void Buscar_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (!portSelected)
            {
                LPorts.Visibility = Visibility.Visible;
                show_Ports.Begin();
                setProgress("Iniciando busca", 2, true);

                List<string> compatiblePorts = await main.getSerial().SearchPorts(); //Busca portas de forma assincrona

                foreach (string port in compatiblePorts)
                {
                    LPorts.Items.Add(port);
                }

                if (compatiblePorts.Count >= 1)
                    LPorts.Visibility = System.Windows.Visibility.Visible;

                setProgress("", 0, false);
                verbose.Visibility = Visibility.Visible;

                if (compatiblePorts.Count >= 1)
                {
                    verbose.Content = "Portas compativeis: " + compatiblePorts.Count;
                }
                else
                {
                    verbose.Content = "Nenhuma porta válida encontrada";
                }
            }
            else
            {
                if (Xs.Value != 0 || Xs.Value.HasValue)
                {
                    verbose.Visibility = Visibility.Hidden;
                    ListData1.configData.setXs((float)Xs.Value);
                    this.XsValue = (float)Xs.Value;                   
                }
                else
                {
                    MessageBox.Show("O valor informado não é válido.\nAdotando Xs = 5.");
                    // Mantem o Xs = 5
                    ListData1.configData.setXs(5);
                }

                if (!main.getGraficos().timerExists())
                    main.getGraficos().AutoRefreshInit();

                this.main.getSerial().actualizeSerialPort();
                main.SetPage(1, false);
            }
        }

        private void LPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TBBuscar.Text = "Iniciar";

            show_Ports.Stop();
            show_Xs.Begin();

            LPorts.Visibility = Visibility.Hidden;
            portSelected = true;
            main.getSerial().setPort(LPorts.SelectedItem.ToString());
        }
    }    
}
