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
    /// Interação lógica para Relatorios.xam
    /// </summary>
    public partial class Relatorios : Page
    {
        private MainWindow main;
        public Relatorios(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }
    }
}
