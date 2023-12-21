using System;
using System.Numerics;
using System.Windows;


namespace CEESP
{
    public class ColectedData
    {

        private float[] Ia;
        private float[] Va;
        private float[] Ea;
        private float[] FP;
        private float[] CFP;
        private float[] P;
        private float RPM = 0;
        private float frequency = 0;
        private int tempo = 0;

        private float Xs = 5;

        public ColectedData(float[] Ia, float[] Va, float[] FP, float[] CFP, float RPM, float frequency)
        {
            this.Ia = Ia;
            this.Va = Va;
            this.FP = FP;
            this.CFP = CFP;
            this.RPM = RPM;
            this.frequency = frequency;

            float[] EaValues = { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (this.getFPType(i) == 'i')
                    EaValues[i] = (this.Xs * Ia[i]) + Va[i];
                if (this.getFPType(i) == 'c')
                    EaValues[i] = Va[i] - (this.Xs * Ia[i]);
            }

            // Calcular potencia
            this.calcularPotencia();

            this.Ea = EaValues;
        }

        public ColectedData(int tempo)
        {
            this.Ia = new float[4] { 2, 2, 2, 2 };
            this.Va = new float[4] { 220, 220, 220, 220 };
            this.FP = new float[4] { 0.87f, 0.87f, 0.87f, 0.87f };
            this.CFP = new float[4] { 1, 1, 2, 2 };
            this.RPM = 2000;
            this.frequency = 60;
            this.tempo = tempo;

            float[] EaValues = { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (this.getFPType(i) == 'i')
                    EaValues[i] = (this.Xs * Ia[i]) + Va[i];
                if (this.getFPType(i) == 'c')
                    EaValues[i] = Va[i] - (this.Xs * Ia[i]);
            }

            this.Ea = EaValues;

            // Calcular Potencia
            this.calcularPotencia();
        }

        public float getIa(int index)
        {
            return this.Ia[index];
        }

        public float getVa(int index)
        {
            return this.Va[index];
        }

        public float getFP(int index)
        {
            return this.FP[index];
        }

        public char getFPType(int index)
        {
            int type = (int)this.CFP[index];

            switch (type)
            {
                case 0:
                    return 'r';
                case 1:
                    return 'i';
                case 2:
                    return 'c';
                default:
                    return '?';
            }
        }

        public void setFPType(char type, int index)
        {
            this.CFP[index] = type == 'r' ? 0 : type == 'i' || type == 'c' ? (type == 'i') ? 1 : 2 : 3;
            this.calcularPotencia();
        }

        public float getRPM()
        {
            return this.RPM;
        }

        public float getFrequency()
        {
            return this.frequency;
        }

        public void setFrequency(float valor)
        {
            this.frequency = valor;
        }

        public void setFP(float valor, int index)
        {
            this.FP[index] = valor;

            this.Ea[index] = calculaEa(index, this.getVa(index), valor);
            this.calcularPotencia();

        }

        public void setRPM(float valor)
        {
            this.RPM = valor;
        }

        public void setVa(float valor, int index)
        {
            this.Va[index] = valor;

            this.Ea[index] = calculaEa(index, valor, this.getFP(index));
            this.calcularPotencia();
        }

        public void setIa(float valor, int index)
        {
            this.Ia[index] = valor;
            this.calcularPotencia();
        }

        public int getTempo()
        {
            return this.tempo;
        }

        public float getEa(int index)
        {
            return this.Ea[index];
        }

        public void setTempo(int tempo)
        {
            this.tempo = tempo;
        }

        private float calculaEa(int i, float Va, float FP)
        {
            float Ea = 0;

            float angle = (float)Math.Acos(FP);
            //                                   R                     Aj
            Complex complexo = new Complex((Va * FP), (Va * Math.Sin(angle)));


            Ea = (float)complexo.Real;

            return Ea;
        }

        public void setXs(float Xs)
        {
            this.Xs = Xs;
        }

        public void setEa(float EaValue, int index)
        {
            this.Ea[index] = EaValue;
            this.calcularPotencia();
        }

        public float getPotencia(int index)
        {
            return this.P[index];
        }

        private void calcularPotencia()
        {
            float[] Potencia = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                // P = V * I * FP
                Potencia[i] = this.Va[i] * this.Ia[i] * this.FP[i];
            }

            this.P = Potencia;
        }

    }
}
