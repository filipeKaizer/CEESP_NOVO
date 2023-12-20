using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
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

        private float IaValue;
        private float VaValue;
        private float FPValue;
        private float RPMValue;
        private float FValue;
        private char Type;


        bool edit = true;
        public Dados(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            this.defaultColor = BorderButton.Background;

            this.Type = '?';
            changeVisibility();
        }

        public void atualizaDados()
        {
            ListViewItem item;
            int index = Phase.SelectedIndex;

            ListData.Items.Clear();

            if (ListData1.colectedData.Count > 0)
            {
                foreach (ColectedData i in ListData1.colectedData)
                {
                    if (i != null)
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
            }
        }

        private void ListData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListData.SelectedIndex != -1)
            {
                try
                {
                    int p = ListData1.configData.getDecimals();
                    int index = Phase.SelectedIndex;

                    float fp = ListData1.colectedData[ListData.SelectedIndex].getFP(index);
                    IaValue = (float)Math.Round(ListData1.colectedData[ListData.SelectedIndex].getIa(index), p);
                    VaValue = (float)Math.Round(ListData1.colectedData[ListData.SelectedIndex].getVa(index), p);
                    FPValue = (float)Math.Round(fp, p);
                    RPMValue = (float)Math.Round(ListData1.colectedData[ListData.SelectedIndex].getRPM(), p);
                    FValue = (float)Math.Round(ListData1.colectedData[ListData.SelectedIndex].getFrequency(), p);
                    Type = ListData1.colectedData[ListData.SelectedIndex].getFPType(index);

                    this.refreshValores();

                    try
                    {
                        TBAngle.Text = (fp <= 0
                            ? (double?)90
                            : fp >= 1 ? (double?)0 : Math.Round((float)(Math.Acos((float)fp) * 180) / Math.PI, ListData1.configData.getDecimals())).ToString() + "º";
                    }
                    catch
                    {
                        MessageBox.Show("Falha no calculo de fp.");
                    }
                } catch (Exception erro)
                {
                    MessageBox.Show("Erro: " + erro.Message + "\nValor de index: " + ListData.SelectedIndex);
                }
            }
        }

        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ListData.SelectedIndex != -1 && ListData1.colectedData.Count > 0)
                changeVisibility();
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListData.SelectedIndex != -1)
            {
                ListData1.colectedData.Remove(ListData1.colectedData[ListData.SelectedIndex]);
                LegendaDefault();
                atualizaDados();

                if (ListData1.colectedData.Count > 0) {
                    this.main.getGraficos().getFasorial().setDado(ListData1.colectedData[ListData1.colectedData.Count - 1]);
                    this.main.getGraficos().getFasorial().drawLines();
                } else
                {
                    this.main.getGraficos().getFasorial().clearGraph();
                }
            }
        }

        private void LegendaDefault()
        {
            TBIa.Text = "0";
            TBVa.Text = "0";
            TBFP.Text = "0";
            TBRPM.Text = "0";
            TBF.Text = "0";
            TBAngle.Text = "0º";
        }


        private void atualizaBaseDeDados()
        {
            if (ListData.SelectedIndex != -1)
            {
                try
                {
                    int index = Phase.SelectedIndex;

                    ListData1.colectedData[ListData.SelectedIndex].setIa((float)IaValue, index);
                    ListData1.colectedData[ListData.SelectedIndex].setVa((float)VaValue, index);
                    ListData1.colectedData[ListData.SelectedIndex].setFP((float)FPValue, index);
                    ListData1.colectedData[ListData.SelectedIndex].setRPM((float)RPMValue);
                    ListData1.colectedData[ListData.SelectedIndex].setFrequency((float)FValue);
                    ListData1.colectedData[ListData.SelectedIndex].setFPType(Type, index);

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

                        // Adiciona cores
                        for (int col = 1; col < 8; col++)
                        {
                            // Adicionar paterntype
                            worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        }
                        worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        worksheet.Cells[1, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

                        worksheet.Cells[1, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                        worksheet.Cells[1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);

                        worksheet.Cells[1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);
                        worksheet.Cells[1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);

                        for (int col = 1; col < 24; col++)
                        {
                            worksheet.Cells[2, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[2, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.BlueViolet);
                        }

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
            try
            {
                this.main.getGraficos().getFasorial().setDado(ListData1.colectedData[ListData1.colectedData.Count - 1]);
                this.main.getGraficos().getFasorial().drawLines();
                atualizaBaseDeDados();
            } catch
            {

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


        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            SalvarArquivo();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            int tempo = 0;
            if (ListData1.colectedData.Count > 0)
            {
                try
                {
                    tempo = ListData1.colectedData[ListData1.colectedData.Count - 1].getTempo();
                } catch
                {
                    tempo = 0;
                }
            }

            else if (ListData1.cache.Count > 0)
                tempo = ListData1.cache[ListData1.cache.Count - 1].getTempo();

            ListData1.colectedData.Add(new ColectedData(tempo + 1));

            this.atualizaDados();
            this.main.getGraficos().getFasorial().setDado(ListData1.colectedData[ListData1.colectedData.Count - 1]);
            this.main.getGraficos().getFasorial().drawLines();

            this.main.saveCache();
        }

        private void refreshValores()
        {
            TBIa.Text = Math.Round(IaValue, 1).ToString() + "A";
            TBVa.Text = Math.Round(VaValue, 1).ToString() + "V";
            TBFP.Text = Math.Round(FPValue, 2).ToString();
            TBRPM.Text = Math.Round(RPMValue, 0).ToString();
            TBF.Text = Math.Round(FValue, 0).ToString() + "Hz";

            if (Type == 'r')
                TBType.Text = "Resistivo";
            else
                TBType.Text = (Type == 'i') ? "Indutivo" : "Capacitivo";
        }

        private void Phase_SelectionChanged(object sender, RoutedEventArgs e)
        {
            atualizaDados();
        }

        private void minusIa_Click(object sender, RoutedEventArgs e)
        {
            if (this.IaValue > 0)
            {
                this.IaValue--;
                refreshValores();
            }
        }

        private void plusIa_Click(object sender, RoutedEventArgs e)
        {
           if (this.IaValue < 300)
            {
                this.IaValue++;
                refreshValores();
            }
        }

        private void minusVa_Click(object sender, RoutedEventArgs e)
        {
            if (this.VaValue - 20 > 0)
            {
                this.VaValue -= 20;
                refreshValores();
            }
        }

        private void plusVa_Click(object sender, RoutedEventArgs e)
        {
            if (this.VaValue + 20 < 6000)
            {
                this.VaValue += 20;
                refreshValores();
            }
        }

        private void minusFP_Click(object sender, RoutedEventArgs e)
        {
            if (this.FPValue > 0)
            {
                this.FPValue -= 0.05f;
                refreshValores();
            }
        }

        private void plusFP_Click(object sender, RoutedEventArgs e)
        {
            if (this.FPValue + 0.05f <= 1)
            {
                this.FPValue += 0.05f;
                refreshValores();
            }
        }

        private void minusRPM_Click(object sender, RoutedEventArgs e)
        {
            if(this.RPMValue - 200 > 0)
            {
                this.RPMValue -= 200;
                refreshValores();
            } 
        }

        private void plusRPM_Click(object sender, RoutedEventArgs e)
        {
            if (this.RPMValue < 10000)
            {
                this.RPMValue += 200;
                refreshValores();
            }
        }


        private void minusF_Click(object sender, RoutedEventArgs e)
        {
            if (this.FValue - 20 > 0)
            {
                this.FValue -= 20;
                refreshValores();
            }
        }

        private void plusF_Click(object sender, RoutedEventArgs e)
        {
            if (this.FValue < 360)
            {
                this.FValue += 20;
                refreshValores();
            }
        }

        private void minusType_Click(object sender, RoutedEventArgs e)
        {
            if (this.Type == 'r')
            {
                this.Type = 'i';
            }
            else
            {
                this.Type = (this.Type == 'i') ? 'c' : 'r';
            }
            refreshValores();
        }


    }
}
