using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CEESP
{
    public class SerialCOM
    {
        private String portSelected = "";
        private MainWindow main;

        private SerialPort serialPort;

        private int tempoCorrente = 0;

        private int delay = 50;

        public SerialCOM(MainWindow main)
        {
            this.main = main;
        }

        public void actualizeSerialPort()
        {
            try
            {
                this.serialPort = new SerialPort(portSelected, ListData1.configData.getBoundRate(), Parity.None, 8, StopBits.One);
            } catch (Exception e)
            {
                setProgressInvoke("Erro: " + e.Message);
            }    
        }

        public Task<List<string>> SearchPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            List<string> comp = new List<string>();
            Random random = new Random();

            return Task.Run(() =>
            {
                foreach (String port in ports)
                {
                    SerialPort serialPort = new SerialPort(port, ListData1.configData.getBoundRate(), Parity.None, 8, StopBits.One);
                    setProgressInvoke(port + ": testando porta...");

                    try
                    {
                        int i = 0;
                        
                            setProgressInvoke(port + ": abrindo comunicação...");

                            serialPort.Open();
                            //Realiza o teste de compatibilidade da comunicação
                            setProgressInvoke(port + ": gerando valores aleatorios...");

                            int A = random.Next(0, 101);
                            int B = random.Next(0, 101);

                            setProgressInvoke(port + ": calculando resposta...");

                            int resposta = (A % B) * (A + B); // O teste é feito com uma operação matemática

                            setProgressInvoke(port + ": cmd + valores...");

                            serialPort.WriteLine(ListData1.configData.getCmdTest()); //Pede teste
                            serialPort.WriteLine($"{A},{B}"); //Envia os valores de teste


                            setProgressInvoke(port + ": testando...");

                            if (int.Parse(serialPort.ReadLine()) == resposta)
                            {
                                setProgressInvoke(port + ": compativel!");
                                comp.Add(port);
                            } else
                            {
                                setProgressInvoke(port + ": incompativel.");
                            }

                            serialPort.Close(); 
                        }
                    
                    catch (Exception e)
                    {
                        setProgressInvoke("Erro: " + e.Message);
                        if (serialPort.IsOpen)
                        {
                            serialPort.Close();
                        } 
                    }
                }
                return comp;
            });
        }

        public async Task<ColectedData> readValues()
        {
            bool connected = false;

            int errorCode = 0;
            float ExV = 0;
            float ExI = 0;
            float[] Va = new float[4];
            float[] Ia = new float[4];
            float[] FP = new float[4];
            float[] CFP = new float[4];
            float frequency = 0;
            float RPM = 0;


            String[] values = { };

            try
            {
                SerialPort connection = this.serialPort;
                connection.Open();

                connection.WriteLine(ListData1.configData.getCmdSend()); //Pede envio de dados

                if (connection.IsOpen)
                {
                    values = await Receber(connection); //Chama de forma assincrona a função para ler dados do arduino
                    String msg = "";
                    foreach (String i in values)
                    {
                        msg += i + " ";
                    }

                    connection.Close();
                    connected = true;
                }

            }
            catch (Exception)
            {
                connected = false;
            }

            if (connected)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] != "NaN")
                    {
                        string[] separados = values[i].Split('=');

                        float valor = float.Parse(separados[1]) / 100;

                        valor = double.IsNaN(valor) ? 0 : valor;

                        switch (separados[0])
                        {
                            case "Erro":
                                errorCode = (int)valor;
                                break;
                            case "ExV":
                                ExV = valor;
                                break;
                            case "ExI":
                                ExI = valor;
                                break;
                            case "Vm":
                                Va[0] = valor;
                                break;
                            case "Va":
                                Va[1] = valor;
                                break;
                            case "Vb":
                                Va[2] = valor;
                                break;
                            case "Vc":
                                Va[3] = valor;
                                break;
                            case "Im":
                                Ia[0] = valor;
                                break;
                            case "Ia":
                                Ia[1] = valor;
                                break;
                            case "Ib":
                                Ia[2] = valor;
                                break;
                            case "Ic":
                                Ia[3] = valor;
                                break;
                            case "FPt":
                                FP[0] = valor;
                                break;
                            case "FPa":
                                FP[1] = valor;
                                break;
                            case "FPb":
                                FP[2] = valor;
                                break;
                            case "FPc":
                                FP[3] = valor;
                                break;
                            case "CFPt":
                                CFP[0] = valor;
                                break;
                            case "CFPa":
                                CFP[1] = valor;
                                break;
                            case "CFPb":
                                CFP[2] = valor;
                                break;
                            case "CFPc":
                                CFP[3] = valor;
                                break;
                            case "F":
                                frequency = valor;
                                break;
                            case "RPM":
                                RPM = valor;
                                break;
                        }
                    }
                }
            }
            // Adicio o tempo corrente baseado no refreshTime
            tempoCorrente += main.getGraficos().getTimeRefresh();

            // Elimina valores NaN, subtiutuindo por 0
            for (int i = 0; i < 3; i++)
            {
                if (Va[i].ToString() == "NaN")
                    Va[i] = 0;

                if (Ia[i].ToString() == "NaN")
                    Ia[i] = 0;

                if (CFP[i].ToString() == "NaN")
                    CFP[i] = 0;

                if (FP[i].ToString() == "NaN")
                    FP[i] = 0;
            }
            ColectedData colected;

            colected = new ColectedData(errorCode, ExI, ExV, Ia, Va, FP, CFP, RPM, frequency);

            colected.setTempo(tempoCorrente);
            colected.setXs(this.main.getInicio().getXs());
            //   this.cessp.setProgressRingStatus(false);
            return colected;
        }

        private Task<string[]> Receber(SerialPort con)
        {
            String[] values = new string[13];
            return Task.Run(() =>
            {
                try
                {
                    if (con != null)
                    {
                        String response = con.ReadLine();
                        values = response.Split(';');
                    }
                }
                catch (Exception)
                {
                    for (int i = 0; i < 13; i++)
                    {
                        values[i] = "NaN";
                    }
                    MessageBox.Show("Falha na comunicação");
                }
                return values;
            });
        }

        public void serialClose()
        {
            if (this.serialPort != null && this.serialPort.IsOpen)
            {
                this.serialPort.Close();
            }
        }

        public void setPort(String port)
        {
            this.portSelected = port;
        }

        public bool isValidPort()
        {
            return this.portSelected != "";
        }

        private void setProgressInvoke(string msg)
        {
            bool active = (msg == "") ? false : true;
            this.main.getInicio().Dispatcher.Invoke(() =>
            {
                this.main.getInicio().setProgress(msg, active);
            }
            );
            Thread.Sleep(delay);
        }

    }
}

