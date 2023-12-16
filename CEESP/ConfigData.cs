using System.Windows;

namespace CEESP
{
    public class ConfigData
    {
        private float XsDefault;
        private float centerX;
        private float centerY;

        private double HEIGTH;
        private double WIDTH;

        private int decimals;
        private int boundRate;
        private int IaMultiplier;
        private int LarguraLinha;
        private int dataBits;
        private int MaxReadItems;

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
            this.centerX = (float)SystemParameters.WorkArea.Width * (float)0.1;
            this.centerY = (float)SystemParameters.WorkArea.Height / 2;
            this.LarguraLinha = 2;
            this.HEIGTH = SystemParameters.PrimaryScreenHeight;
            this.WIDTH = SystemParameters.PrimaryScreenWidth;

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

            // Leitura e armazenamento
            this.MaxReadItems = 60;
        }

        public float getCenterX()
        {
            return this.centerX;
        }

        public float getWidth()
        {
            return (float)this.WIDTH;
        }

        public float getHeigth()
        {
            return (float)this.HEIGTH;
        }

        public float getCenterY()
        {
            return this.centerY;
        }

        public float getXs()
        {
            return this.XsDefault;
        }

        public void setXs(float Xs)
        {
            this.XsDefault = Xs;
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

        public int getMaxItems()
        {
            return this.MaxReadItems;
        }
    }
}
