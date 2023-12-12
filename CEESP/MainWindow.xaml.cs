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

        private void rtGraficos_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtGraficos.Fill = enter_color;
        }

        private void rtGraficos_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtGraficos.Fill = exit_color;
        }

        private void rtDados_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtDados.Fill = enter_color;
        }

        private void rtDados_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtDados.Fill = exit_color;
        }

        private void rtRelatorios_MouseEnter(object sender, RoutedEventArgs e)
        {
            rtRelatorios.Fill = enter_color;
        }

        private void rtRelatorios_MouseLeave(object sender, RoutedEventArgs e)
        {
            rtRelatorios.Fill = exit_color;
        }

        private void menu_button_Click(object sender, RoutedEventArgs e)
        {
            if(menu_isVisible)
            {
                mostra_menu.Stop();
                esconde_menu.Begin();
                menu_isVisible = false;
            } else
            {
                esconde_menu.Stop();
                mostra_menu.Begin();
                menu_isVisible = true;
            }
        }
    }
}
