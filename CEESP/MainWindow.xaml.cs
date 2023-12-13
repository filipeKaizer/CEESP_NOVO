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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CEESP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolidColorBrush enter_color; 
        private SolidColorBrush exit_color;
        private bool menu_isVisible = false;

        private Storyboard mostra_menu;
        private Storyboard esconde_menu;

        private Inicio inicio;
        private Dados dados;
        private Graficos graficos;
        private Relatorios relatorios;
        private SerialCOM serial;

        public MainWindow()
        {
            InitializeComponent();

            enter_color = new SolidColorBrush(Color.FromArgb(51, 240, 230, 230));
            exit_color = new SolidColorBrush(Color.FromArgb(0, 240, 230, 230));

            // Define o tamanho de tela e tipo
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.SingleBorderWindow;

            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;

            this.Left = SystemParameters.WorkArea.Left;
            this.Top = SystemParameters.WorkArea.Top;

            backgroundPrincipal.Width = SystemParameters.WorkArea.Width;
            backgroundPrincipal.Height = SystemParameters.WorkArea.Height;

            Work.Height = SystemParameters.WorkArea.Height;
            Work.Width = SystemParameters.WorkArea.Width;

            this.esconde_menu = (Storyboard)FindResource("esconde_menu");
            this.mostra_menu = (Storyboard)FindResource("mostra_menu");

            inicio = new Inicio(this);
            graficos = new Graficos(this);
            dados = new Dados(this);
            relatorios = new Relatorios(this);
            serial = new SerialCOM(this);

            FrameInicio.Navigate(this.inicio);
            FrameGraficos.Navigate(this.graficos);
            FrameDados.Navigate(this.dados);
            FrameRelatorios.Navigate(this.relatorios);

            Work.SelectedIndex = 0;
        }

        public void SetPage(int index)
        {
            this.Work.SelectedIndex = index;
            change_menu();

        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void rtInicio_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtInicio.Fill = enter_color;
        }

        private void rtInicio_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtInicio.Fill = exit_color;
        }

        private void Inicio_Click(object sender, RoutedEventArgs e)
        {
            this.SetPage(0);
        }

        private void rtGraficos_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtGraficos.Fill = enter_color;
        }

        private void rtGraficos_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtGraficos.Fill = exit_color;
        }

        private void Grafico_Click(object sender, RoutedEventArgs e)
        {
            this.SetPage(1);
        }

        private void rtDados_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtDados.Fill = enter_color;
        }

        private void rtDados_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtDados.Fill = exit_color;
        }

        private void Dados_Click(object sender, RoutedEventArgs e)
        {
            this.SetPage(2);
        }

        private void rtRelatorios_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtRelatorios.Fill = enter_color;
        }

        private void rtRelatorios_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtRelatorios.Fill = exit_color;
        }

        private void Relatorios_Click(object sender, RoutedEventArgs e)
        {
            this.SetPage(3);
        }

        private void menu_button_Click(object sender, RoutedEventArgs e)
        {
            change_menu();
        }

        private void change_menu()
        {
            if (menu_isVisible)
            {
                mostra_menu.Stop();
                esconde_menu.Begin();
                menu_isVisible = false;
            }
            else
            {
                esconde_menu.Stop();
                mostra_menu.Begin();
                menu_isVisible = true;
            }
        }

        public double getWidth()
        {
            return this.Width;
        }

        public double getHeigth()
        {
            return this.Height;
        }

        public Inicio getInicio()
        {
            return this.inicio;
        }

        public Dados getDados()
        {
            return this.dados;
        }

        public Graficos getGraficos()
        {
            return this.graficos;
        }

        public Relatorios getRelatorios()
        {
            return this.relatorios;
        }

        public SerialCOM getSerial()
        {
            return this.serial;
        }
    }
}
