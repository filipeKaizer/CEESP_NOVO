using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CEESP
{
    /// <summary>
    /// Interação lógica para Dados.xam
    /// </summary>
    public partial class Dados : Page
    {
        private MainWindow main;
        private Brush defaultColor;

        bool edit = true;
        public Dados(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.defaultColor = BorderButton.Background;

            changeVisibility();
        }


        public void atualizaDados()
        {
            ListViewItem item;
            int index = Phase.SelectedIndex;

            ListData.Items.Clear();


            foreach (ColectedData i in ListData1.colectedData)
            {
                item = new ListViewItem();
                item.Content = new
                {
                    Tempo = i.getTempo() + "s",
                    Va = Math.Round(i.getVa(index), 2),
                    Ia = Math.Round(i.getIa(index), 2),
                    Ea = Math.Round(i.getEa(index), 2),
                    FP = Math.Round(i.getFP(index), 2).ToString(),
                    RPM = Math.Round(i.getRPM(), 2),
                    F = Math.Round(i.getFrequency(), 2),
                    Tipo = (i.getFP(index) == 1) ? "Resisitivo" : ((i.getFPType(index) == 'i') ? "Indutivo" : "Capacitivo")
                };

                ListData.Items.Add(item);
                TextNItens.Text = "Itens: " + ListData1.colectedData.Count;
            }
        }

        private void ListData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListData.SelectedIndex != -1)
            {
                int p = ListData1.configData.getDecimals();
                int index = Phase.SelectedIndex;

                float fp = ListData1.colectedData[ListData.SelectedIndex].getFP(index);
                TBIa.Value = Math.Round(ListData1.colectedData[ListData.SelectedIndex].getIa(index), p);
                TBVa.Value = Math.Round(ListData1.colectedData[ListData.SelectedIndex].getVa(index), p);
                TBFP.Value = Math.Round(fp, p);
                TBRPM.Value = Math.Round(ListData1.colectedData[ListData.SelectedIndex].getRPM(), p);
                TBF.Value = Math.Round(ListData1.colectedData[ListData.SelectedIndex].getFrequency(), p);

                try
                {
                    TBAngle.Value = fp <= 0
                        ? (double?)90
                        : fp >= 1 ? (double?)0 : Math.Round((float)(Math.Acos((float)fp) * 180) / Math.PI, ListData1.configData.getDecimals());
                }
                catch
                {
                    MessageBox.Show("Falha no calculo de fp.");
                }
            }
        }

        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ListData.SelectedIndex != -1)
                changeVisibility();
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListData.SelectedIndex != -1)
            {
                ListData1.colectedData.Remove(ListData1.colectedData[ListData.SelectedIndex]);
                LegendaDefault();
                atualizaDados();
            }
        }

        private void LegendaDefault()
        {
            TBIa.Value = 0;
            TBVa.Value = 0;
            TBFP.Value = 0;
            TBRPM.Value = 0;
            TBF.Value = 0;
            TBAngle.Value = 0;
        }


        private void atualizaBaseDeDados()
        {
            if (ListData.SelectedIndex != -1)
            {
                try
                {
                    int index = Phase.SelectedIndex;

                    ListData1.colectedData[ListData.SelectedIndex].setIa((float)TBIa.Value, index);
                    ListData1.colectedData[ListData.SelectedIndex].setVa((float)TBVa.Value, index);
                    ListData1.colectedData[ListData.SelectedIndex].setFP((float)TBFP.Value, index);
                    ListData1.colectedData[ListData.SelectedIndex].setRPM((float)TBRPM.Value);
                    ListData1.colectedData[ListData.SelectedIndex].setFrequency((float)TBF.Value);

                    atualizaDados();
                    this.main.getGraficos().getFasorial().drawLines();
                    this.main.getGraficos().getTemporal().atualizaGraph();

                    // Desativa edição
                    btSaveAfterEdit.Visibility = Visibility.Hidden;
                    TBVa.IsEnabled = false;
                    TBFP.IsEnabled = false;
                    TBIa.IsEnabled = false;
                    TBRPM.IsEnabled = false;
                    TBF.IsEnabled = false;
                    edit = false;
                }
                catch
                {
                    MessageBox.Show("Erro.");
                }
            }
        }

        private void changeVisibility()
        {
            if (edit)
            {
                TBIa.IsEnabled = false;
                TBVa.IsEnabled = false;
                TBFP.IsEnabled = false;
                TBRPM.IsEnabled = false;
                TBF.IsEnabled = false;
                edit = false;
                btSaveAfterEdit.Visibility = Visibility.Hidden;
            }
            else
            {
                TBIa.IsEnabled = true;
                TBVa.IsEnabled = true;
                TBFP.IsEnabled = true;
                TBRPM.IsEnabled = true;
                TBF.IsEnabled = true;
                edit = true;
                btSaveAfterEdit.Visibility = Visibility.Visible;
                btSaveAfterEdit.IsEnabled = true;
            }
        }

        private void SalvarArquivo()
        {
            if (ListData1.colectedData.Count != 0)
            {

                SaveFileDialog SaveWindow = new SaveFileDialog();
                SaveWindow.Filter = "Arquivo Excel (*.xlsx)|*.xlsx";
                SaveWindow.Title = "Escolher caminho do arquivo de dados";

                if (SaveWindow.ShowDialog() == true)
                {
                    string caminhoArquivo = SaveWindow.FileName;

                    FileInfo fileInfo = new FileInfo(caminhoArquivo);

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
                    {
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Dados");

                        // Adiciona numeros de verificação
                        Random random = new Random();
                        int A = random.Next(0, 101);
                        int B = random.Next(0, 101);

                        int resposta = (A % B) * (A + B);

                        worksheet.Cells[1, 1].Value = A;
                        worksheet.Cells[1, 2].Value = B;
                        worksheet.Cells[1, 3].Value = resposta;

                        // Adiciona Xs
                        worksheet.Cells[1, 4].Value = "Xs";
                        worksheet.Cells[1, 5].Value = ListData1.configData.getXs();

                        // Adiciona O numero de intens
                        worksheet.Cells[1, 6].Value = "Itens";
                        worksheet.Cells[1, 7].Value = ListData1.colectedData.Count;

                        // Adiciona os cabeçalhos
                        worksheet.Cells[2, 1].Value = "Tempo";
                        worksheet.Cells[2, 2].Value = "RPM";
                        worksheet.Cells[2, 3].Value = "Freq.";
                        worksheet.Cells[2, 4].Value = "Va";
                        worksheet.Cells[2, 5].Value = "Ia";
                        worksheet.Cells[2, 6].Value = "Ea";
                        worksheet.Cells[2, 7].Value = "FP";
                        worksheet.Cells[2, 8].Value = "Tipo";
                        worksheet.Cells[2, 9].Value = "VaA";
                        worksheet.Cells[2, 10].Value = "IaA";
                        worksheet.Cells[2, 11].Value = "EaA";
                        worksheet.Cells[2, 12].Value = "FPA";
                        worksheet.Cells[2, 13].Value = "TipoA";
                        worksheet.Cells[2, 14].Value = "VaB";
                        worksheet.Cells[2, 15].Value = "IaB";
                        worksheet.Cells[2, 16].Value = "EaB";
                        worksheet.Cells[2, 17].Value = "FPB";
                        worksheet.Cells[2, 18].Value = "TipoB";
                        worksheet.Cells[2, 19].Value = "VaC";
                        worksheet.Cells[2, 20].Value = "IaC";
                        worksheet.Cells[2, 21].Value = "EaC";
                        worksheet.Cells[2, 22].Value = "FPC";
                        worksheet.Cells[2, 23].Value = "TipoC";

                        // Adiciona os dados
                        int i = 0;
                        bool u = ListData1.configData.getUnidade();
                        ConfigData c = ListData1.configData;
                        List<ColectedData> dados;

                        if (ListData1.cache.Count > 0)
                        {
                            dados = ListData1.cache;

                            foreach (ColectedData colect in ListData1.colectedData)
                            {
                                dados.Add(colect);
                            }
                        }
                        else
                        {
                            dados = ListData1.colectedData;
                        }

                        foreach (ColectedData data in dados)
                        {
                            int p = c.getDecimals();

                            // Adiciona os valores comuns
                            worksheet.Cells[i + 3, 1].Value = data.getTempo();
                            worksheet.Cells[i + 3, 2].Value = Math.Round(data.getRPM(), 2);
                            worksheet.Cells[i + 3, 3].Value = Math.Round(data.getFrequency(), 2);

                            for (int index = 0; index < 4; index++)
                            {
                                worksheet.Cells[i + 3, index * 5 + 4].Value = Math.Round(data.getVa(index), p);
                                worksheet.Cells[i + 3, index * 5 + 5].Value = Math.Round(data.getIa(index), p);
                                worksheet.Cells[i + 3, index * 5 + 6].Value = Math.Round(data.getEa(index), p);
                                worksheet.Cells[i + 3, index * 5 + 7].Value = Math.Round(data.getFP(index), p);

                                if (data.getFP(index) == 1)
                                    worksheet.Cells[i + 3, index * 5 + 8].Value = "Resistiva";
                                else if (data.getFPType(index) == 'i')
                                    worksheet.Cells[i + 3, index * 5 + 8].Value = "Indutiva";
                                else if (data.getFPType(index) == 'c')
                                    worksheet.Cells[i + 3, index * 5 + 8].Value = "Capacitiva";
                                else if (data.getFPType(index) == '?')
                                    worksheet.Cells[i + 3, index * 5 + 8].Value = "Inválido";
                            }

                            i++;
                        }

                        excelPackage.Save();
                    }
                }
            }
        }

        private void btSaveAfterEdit_Click(object sender, RoutedEventArgs e)
        {
            atualizaBaseDeDados();
        }

        private void Buscar_MouseEnter(object sender, RoutedEventArgs e)
        {
            BorderButton.Background = Brushes.DarkCyan;
        }

        private void Buscar_MouseLeave(object sender, RoutedEventArgs e)
        {
            BorderButton.Background = defaultColor;
        }


        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            SalvarArquivo();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Temporário, remover na versão final
            int tempo = 0;
            if (ListData1.colectedData.Count > 0)
                tempo = ListData1.colectedData[ListData1.colectedData.Count - 1].getTempo();
            else if (ListData1.cache.Count > 0)
                tempo = ListData1.cache[ListData1.cache.Count - 1].getTempo();

            ListData1.colectedData.Add(new ColectedData(tempo + 1));

            this.atualizaDados();

            this.main.getGraficos().getFasorial().drawLines();

            this.main.saveCache();
        }

        private void Phase_SelectionChanged(object sender, RoutedEventArgs e)
        {
            atualizaDados();
        }
    }
}
