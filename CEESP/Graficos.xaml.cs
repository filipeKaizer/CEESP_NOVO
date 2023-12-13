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
    /// Interação lógica para Graficos.xam
    /// </summary>
    public partial class Graficos : Page
    {
        private MainWindow main;
        private int tempo = 2;
        public Graficos(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }





        public int getTimeRefresh()
        {
            return this.tempo;
        }
    }
}
