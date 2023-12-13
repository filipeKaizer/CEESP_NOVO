using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEESP
{
    public class ConfigData
    {
        private float XsDefault;
        private float centerX;
        private float centerY;

        private int decimals;
        private int boundRate;
        private int IaMultiplier;
        private int LarguraLinha;
        private int dataBits;

        private string cmdSend;
        private string cmdRele;
        private string cmdTest;
        private string UnTensao;
        private string UnCorrente;
        private string UnRPM;
        private string UnFreq;
        private string UnTempo;

        private bool AdicionarUnidade;


        public ConfigData()
        {
            // Valores
            this.XsDefault = 5.0f;
            this.decimals = 2;
            this.IaMultiplier = 5;

            // Grafico
            this.centerX = 250;
            this.centerY = 450;
            this.LarguraLinha = 2;

            // Comunicação
            this.boundRate = 9600;
            this.cmdTest = "test";
            this.cmdSend = "snd";
            this.cmdRele = "rele";
            this.dataBits = 8;

            // Arquivo
            this.AdicionarUnidade = false;
            this.UnCorrente = "A";
            this.UnTensao = "V";
            this.UnFreq = "Hz";
            this.UnRPM = "RPM";
            this.UnTempo = "s";

        }

        public float getCenterX()
        {
            return this.centerX;
        }

        public float getCenterY()
        {
            return this.centerY;
        }

        public float getXs()
        {
            return this.XsDefault;
        }

        public int getDecimals()
        {
            return this.decimals;
        }

        public int getBoundRate()
        {
            return this.boundRate;
        }

        public int getDataBits()
        {
            return this.dataBits;
        }

        public string getCmdSend()
        {
            return this.cmdSend;
        }

        public string getCmdRele()
        {
            return this.cmdRele;
        }

        public string getCmdTest()
        {
            return this.cmdTest;
        }

        public int getIaMultiplier()
        {
            return this.IaMultiplier;
        }

        public int getLarguraLinha()
        {
            return this.LarguraLinha;
        }


        public string getUnCorrente()
        {
            return this.UnCorrente;
        }

        public string getUnTensao()
        {
            return this.UnTensao;
        }

        public string getUnRPM()
        {
            return this.UnRPM;
        }

        public string getUnFreq()
        {
            return this.UnFreq;
        }

        public string getUnTempo()
        {
            return this.UnTempo;
        }

        public bool getUnidade()
        {
            return this.AdicionarUnidade;
        }
    }
}
