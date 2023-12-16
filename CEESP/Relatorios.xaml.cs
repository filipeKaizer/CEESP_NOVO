using System.Windows.Controls;

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
