using System;
using System.Numerics;
using System.Windows;


namespace CEESP
{
    public class ColectedData
    {
        private int errorCode;
        private float[] Ia;
        private float[] Va;
        private float[] Ea;
        private float[] FP;
        private float[] CFP;
        private float[] P;
        private float ExI;
        private float ExV;
        private float RPM = 0;
        private float frequency = 0;
        private int tempo = 0;

        private float Xs = 5;

        private Random rand;

        public ColectedData(int errorCode, float ExI, float ExV, float[] Ia, float[] Va, float[] FP, float[] CFP, float RPM, float frequency)
        {
            this.errorCode = errorCode;
            this.ExI = ExI;
            this.ExV = ExV;
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

        // Gera dados de forma aleatória
        public ColectedData(int tempo)
        {
            this.rand = new Random();
            // Excitatriz
            this.ExI = getRandNumber(0.1f, 5f);
            this.ExV = getRandNumber(0.1f, 250.0f);
            
            // RPM
            this.RPM = getRandNumber(200f, 5000f);

            // Frequencia
            this.frequency = getRandNumber(50f, 61f);

            // Ia M, A, B, C
            this.Ia = new float[4];
            for (int i = 1; i < 4; i++)
                this.Ia[i] = getRandNumber(0.1f, 3f);
            this.Ia[0] = (this.Ia[1] + this.Ia[2] + this.Ia[3]) / 3;

            // Va M, A, B, C
            this.Va = new float[4];
            for (int i = 1; i < 4; i++)
                this.Va[i] = getRandNumber(10f, 250f);
            this.Va[0] = (this.Va[1] + this.Va[2] + this.Va[3]) / 3;

            // FP M, A, B, C
            this.FP = new float[4];
            for (int i = 1; i < 4; i++)
                this.FP[i] = getRandNumber(0f, 1f);
            this.FP[0] = (this.FP[1] + this.FP[2] + this.FP[3]) / 3;

            // Caracteristica do fator de potencia
            this.CFP = new float[4];
            for (int i = 1; i < 4; i++)
                this.CFP[i] = this.rand.Next(1, 3);

            int ind = 0, cap = 0;
            for (int i = 1; i < 4; i++)
            {
                if (this.CFP[i] == 1)
                    ind++;
                else
                    cap++;
            }

            this.CFP[0] = (ind > cap) ? 1 : 2;
            
            // Adiciona o tempo
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

        private float getRandNumber(float inicio, float final)
        {
            return (float)(this.rand.NextDouble() * (final - inicio) + inicio);
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

        public bool getPhaseFail(int index)
        {
            if (this.Va[index] == 0 && this.Ia[index] == 0)
                return true;
            else 
                return false;
        }

        public bool getNullFail()
        {
            for (int i = 0; i < 3; i++)
            {
                if (this.Va[i] != 0) 
                    return false;
            }

            return true;
        }

        public int getErrorCode()
        {
            return this.errorCode;
        }

        public float getExtV()
        {
            return this.ExV;
        }

        public float getExtI()
        {
            return this.ExI;
        }

        public void setExtI(float i)
        {
            this.ExI = i;
        }

        public void setExtV(float v)
        {
            this.ExV = v;
        }

    }
}
