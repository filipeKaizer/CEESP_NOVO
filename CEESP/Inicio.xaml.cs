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
using System.Windows.Input;
using System.Windows.Media;
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
        
        public Inicio(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.Width = main.getWidth();
            this.Height = main.getHeigth();
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
            return XsValue;
        }
    }    
}
