using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using OfficeOpenXml;

namespace CEESP
{
    internal class Arquivo
    {
        private List<ColectedData> dados;
        private float IaMax;
        private float VaMax;
        private float Xs;
        private int itens;
        private string nome;
        private bool compatible;
        

        public Arquivo(String archivePath)
        {
            this.nome = archivePath;
            this.dados = new List<ColectedData>();
            openArchive();
        }

        private bool openArchive()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (this.nome != "")
            {
                using (var package = new ExcelPackage(new FileInfo(this.nome)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Assume que você está interessado na primeira planilha

                    // Verificar compatibilidade do arquivo
                   if( verifyArchive(worksheet))
                   {
                        this.compatible = true;
                        this.readValues(worksheet);

                   }else
                   {
                        this.compatible = false;
                   }  
                }

                return true;
            } else
            {
                return false;
            }

        }

        public string getNome()
        {
            return this.nome;
        }

        public float getVaMax()
        {
            return this.VaMax;
        }

        public float getIaMax()
        {
            return this.IaMax;
        }

        public int getNumberItems()
        {
            return this.dados.Count;
        }

        public bool isCompatible()
        {
            return this.compatible;
        }

        public float getXs()
        {
            return this.Xs;
        }

        private bool verifyArchive(ExcelWorksheet worksheet)
        {
            int A, B, result;

            if (int.TryParse(worksheet.Cells[1, 1].Value?.ToString(), out A))
            {
                if (int.TryParse(worksheet.Cells[1, 2].Value?.ToString(), out B))
                {
                    if (int.TryParse(worksheet.Cells[1, 3].Value?.ToString(), out result)) {
                        int teste = (A % B) * (A + B);

                        if (teste == result)
                            return true;
                        else 
                            return false;
                    }
                }
            }
            return false;
        }

        private void readValues(ExcelWorksheet worksheet)
        {
            // Obtem o Xs
            this.Xs = float.TryParse(worksheet.Cells[1, 5].Value?.ToString(), out float parsedValue) ? parsedValue : 0.0f;

            // Obtem o numero de itens
            this.itens = int.TryParse(worksheet.Cells[1, 3].Value?.ToString(), out int parsedValueItens) ? parsedValueItens : 0;

            // Obtem todos os valores
            for (int lin = 3; lin <= worksheet.Dimension.End.Row; lin++)
            {
                // Obtem tempo e cria nova instancia de dados
                int tempo = int.TryParse(worksheet.Cells[lin, 1].Value?.ToString(), out int parsedValueTempo) ? parsedValueTempo : 0;

                ColectedData dado = new ColectedData(tempo);

                // Obtem Va
                dado.setVa(float.TryParse(worksheet.Cells[lin, 2].Value?.ToString(), out float parsedValueVa) ? parsedValueVa : 0.0f, 0);

                // Obtem Ia
                dado.setIa(float.TryParse(worksheet.Cells[lin, 3].Value?.ToString(), out float parsedValueIa) ? parsedValueIa : 0.0f, 0);

                // Obtem Ea
                dado.setEa(float.TryParse(worksheet.Cells[lin, 4].Value?.ToString(), out float parsedValueEa) ? parsedValueEa : 0.0f, 0);

                // Obtem FP

                this.dados.Add(dado);
            }


            // Verifica o maior valor de Va e Ia e o numero de cada carga
            this.IaMax = 0;
            this.VaMax = 0;
            if (dados.Count > 0) {
                foreach (ColectedData d in dados)
                {
                    this.IaMax = (d.getIa(0) > IaMax) ? d.getIa(0) : this.IaMax; 
                    this.VaMax = (d.getVa(0) > VaMax) ? d.getVa(0) : this.VaMax;
                }
            }
        }

    }
}
