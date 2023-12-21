using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        private Storyboard hide_options;
        private Arquivo arquivo;

        private Brush defaultColor;

        bool portSelected = false;

        public Inicio(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.Width = ListData1.configData.getWidth();
            this.Height = ListData1.configData.getHeigth();

            this.Page.Width = ListData1.configData.getWidth();
            this.Page.Height = ListData1.configData.getHeigth();

            this.Grid.Width = ListData1.configData.getWidth();
            this.Grid.Height = ListData1.configData.getHeigth();

            this.show_Xs = (Storyboard)FindResource("show_Xs");
            this.show_Ports = (Storyboard)FindResource("show_ports");
            this.hide_options = (Storyboard)FindResource("hide_Options");

            this.defaultColor = BorderButton.Background;
        }

        public void setProgress(string texto, bool ativo)
        {
            if (ativo)
            {
                progress.IsActive = true;
                verbose.Visibility = Visibility.Visible;

                //Adiciona valores
                verbose.Content = texto;
            }
            else
            {
                progress.IsActive = false;
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
                TBBuscarModulo.Visibility = Visibility.Collapsed;
                TBBuscarModuloText.Visibility = Visibility.Collapsed;

                LPorts.Visibility = Visibility.Visible;
                show_Ports.Begin();
                setProgress("Iniciando busca", true);

                List<string> compatiblePorts = await main.getSerial().SearchPorts(); //Busca portas de forma assincrona

                setProgress("Adicionando porta", true);

                foreach (string port in compatiblePorts)
                {
                    LPorts.Items.Add(port);
                }

                if (compatiblePorts.Count >= 1)
                    LPorts.Visibility = System.Windows.Visibility.Visible;

                setProgress("", false);
                verbose.Visibility = Visibility.Visible;

                verbose.Content = compatiblePorts.Count >= 1 ? "Portas compativeis: " + compatiblePorts.Count : (object)"Nenhuma porta válida encontrada";
                TBSelecioneUmaPorta.Visibility = Visibility.Visible;
            }
            else
            {
                if (Xs.Value != 0 && Xs.Value.HasValue)
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

                this.main.getGraficos().getFasorial().changeMode(true);


                main.SetPage(1, false);
            }
        }

        private void Buscar_MouseEnter(object sender, RoutedEventArgs e)
        {
            BorderButton.Background = Brushes.DarkCyan;
        }

        private void Buscar_MouseLeave(object sender, RoutedEventArgs e)
        {
            BorderButton.Background = defaultColor;
        }

        private void BuscarArquivo_MouseEnter(object sender, RoutedEventArgs e)
        {
            BorderBuscarArquivo.Background = Brushes.DarkCyan;
        }

        private void BuscarArquivo_MouseLeave(object sender, RoutedEventArgs e)
        {
            BorderBuscarArquivo.Background = defaultColor;
        }

        private void BuscarArquivo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos Excel|*.xlsx;*.xls",
                Title = "Selecione o arquivo de máquinas elétricas"
            };

            arquivo = new Arquivo((openFileDialog.ShowDialog() == true) ? openFileDialog.FileName : "");

            if (arquivo.isCompatible())
            {
                Arquivo.Visibility = Visibility.Hidden;
                ArquivoSelecionado.Visibility = Visibility.Visible;

                TBNome.Text = "Nome: " + arquivo.getNome();
                TBLeituras.Text = "Leituras: " + arquivo.getNumberItems().ToString();
                TBXs.Text = arquivo.getXs().ToString() + "Ω";
                TBVa.Text = arquivo.getVaMax().ToString() + "V";
                TBIa.Text = arquivo.getIaMax().ToString() + "A";
                TBIndutivo.Text = arquivo.getIndutivo().ToString();
                TBResistivo.Text = arquivo.getResistivo().ToString();
                TBCapacitivo.Text = arquivo.getCapacitivo().ToString();
                verbose.Content = "";
            }
            else
            {

                verbose.Content = "Arquivo não compatível.";
            }

            progress.IsActive = false;
        }

        private void btModulo_MouseEnter(object sender, RoutedEventArgs e)
        {
            BorderButtonModulo.BorderBrush = Brushes.DarkViolet;
            rtModulo.StrokeThickness = 3;
            rtModulo.Stroke = Brushes.DarkViolet;
        }

        private void btModulo_MouseLeave(object sender, RoutedEventArgs e)
        {
            BorderButtonModulo.Background = defaultColor;
            rtModulo.StrokeThickness = 1;
            rtModulo.Stroke = null;
        }

        private void btModulo_Click(object sender, RoutedEventArgs e)
        {
            hide_options.Begin();
            Modulo.Visibility = Visibility.Visible;
            Back.Visibility = Visibility.Visible;
        }

        private void btArquivo_MouseEnter(object sender, RoutedEventArgs e)
        {

            BorderButtonArquivo.BorderBrush = Brushes.DarkViolet;
            rtArquivo.StrokeThickness = 3;
            rtArquivo.Stroke = Brushes.DarkViolet;
        }

        private void btArquivo_MouseLeave(object sender, RoutedEventArgs e)
        {
            BorderButtonArquivo.Background = defaultColor;
            rtArquivo.StrokeThickness = 1;
            rtArquivo.Stroke = null;
        }

        private void btArquivo_Click(object sender, RoutedEventArgs e)
        {
            hide_options.Begin();
            Arquivo.Visibility = Visibility.Visible;
            Back.Visibility = Visibility.Visible;
        }

        private void Seguir_Click(object sender, RoutedEventArgs e)
        {
            ListData1.configData.setModuloAtivo(false);
            ListData1.colectedData = arquivo.getDados();

            if (arquivo != null)
                ListData1.configData.setXs(arquivo.getXs());

            this.main.getGraficos().getFasorial().changeMode(false);

            if (ListData1.colectedData.Count > 0)
            {
                this.main.getGraficos().getFasorial().setDado(ListData1.colectedData[ListData1.colectedData.Count - 1]);
                this.main.getGraficos().getFasorial().drawLines();
            }

            // Atualiza dados
            this.main.getDados().atualizaDados();

            // Atualiza grafico temporal
            this.main.getGraficos().getTemporal().atualizaGraph();

            this.main.SetPage(1, false);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Modulo.Visibility = Visibility.Hidden;
            Arquivo.Visibility = Visibility.Hidden;
            ArquivoSelecionado.Visibility = Visibility.Hidden;
            Back.Visibility = Visibility.Hidden;
            Options.Visibility = Visibility.Visible;

            this.XsValue = 5;
            this.main.getSerial().setPort("");

            show_Xs.Stop();
            show_Ports.Stop();
            hide_options.Stop();


        }

        private void LPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TBBuscar.Text = "Iniciar";

            show_Ports.Stop();
            show_Xs.Begin();

            LPorts.Visibility = Visibility.Hidden;
            portSelected = true;
            main.getSerial().setPort(LPorts.SelectedItem.ToString());
            TBSelecioneUmaPorta.Text = "Informe um valor para a reatância sincrona (Xs)";
        }
    }
}
