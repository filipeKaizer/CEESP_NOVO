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
        private int numIndutivo;
        private int numResistivo;
        private int numCapacitivo;

        private string nome;
        private bool compatible;
        

        public Arquivo(String archivePath)
        {
            this.nome = archivePath;
            this.dados = new List<ColectedData>();
            this.numResistivo = 0;
            this.numCapacitivo = 0;
            this.numIndutivo = 0;

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
                return false;
        }

        public List<ColectedData> getDados()
        {
            return this.dados;
        }

        public string getNome()
        {

            return Path.GetFileName(this.nome);
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
            return this.itens;
        }

        public bool isCompatible()
        {
            return this.compatible;
        }

        public float getXs()
        {
            return this.Xs;
        }

        public int getIndutivo()
        {
            return this.numIndutivo;
        }

        public int getResistivo()
        {
            return this.numResistivo;
        }

        public int getCapacitivo()
        {
            return this.numCapacitivo;
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

                // Obtem RPM
                dado.setRPM(float.TryParse(worksheet.Cells[lin, 2].Value?.ToString(), out float parsedValueRPM) ? parsedValueRPM : 0.0f);

                // Obtem a frequencia
                dado.setFrequency(float.TryParse(worksheet.Cells[lin, 3].Value?.ToString(), out float parsedValueF) ? parsedValueF : 0.0f);

                for (int i = 0; i < 4; i++)
                {
                    // Obtem Va
                    dado.setVa(float.TryParse(worksheet.Cells[lin, i * 5 + 4].Value?.ToString(), out float parsedValueVa) ? parsedValueVa : 0.0f, i);

                    // Obtem Ia
                    dado.setIa(float.TryParse(worksheet.Cells[lin, i * 5 + 5].Value?.ToString(), out float parsedValueIa) ? parsedValueIa : 0.0f, i);

                    // Obtem Ea
                    dado.setEa(float.TryParse(worksheet.Cells[lin, i * 5 + 6].Value?.ToString(), out float parsedValueEa) ? parsedValueEa : 0.0f, i);

                    // Obtem FP
                    dado.setFP(float.TryParse(worksheet.Cells[lin, i * 5 + 7].Value?.ToString(), out float parsedValueFP) ? parsedValueFP : 0.0f, i);

                    
                    // Obtem e classifica o FP
                    string tipo = worksheet.Cells[lin, i * 5 + 8].Value.ToString();
                    char type = '?';

                    if (tipo == "Resistiva")
                    {
                        type = 'r';
                        this.numResistivo++;
                    }
                    else if (tipo == "Indutiva")
                    {
                        type = 'i';
                        this.numIndutivo++;
                    }
                    else if (tipo == "Capacitiva")
                    {
                        type = 'c';
                        this.numCapacitivo++;
                    }
                    else
                        type = '?';

                    dado.setFPType(type, i);
                }
                // Adiciona
                this.dados.Add(dado);
            }

            // Verifica o numero de itens
            this.itens = dados.Count;

            // Verifica o maior valor de Va e Ia
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
