using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.IO.Ports;
using TeGe2;

namespace TeGe
{
    class SerialCom
    {
        static int estado;
        static SerialPort _serialPort;
        public static void Controle_Ar(string sala)
        {
            //string comando;

            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";
            _serialPort.BaudRate = 9600;
            _serialPort.Open();
            //_serialPort.WriteLine("0");

            //int Contador_SalaReunioes = GlobalData.DictSalaReunioes.Count;
            //int Contador_SalaPrincipal = GlobalData.DictSalaPrincipal.Count;
            //int Contador_CorredorBaias = GlobalData.DictCorredorBaias.Count;

            estado = 1;

            while (GlobalData.FlagPrograma == 1)
            {
                //Console.WriteLine("Ligar: <0> \t Desligar: <1> \t Sair: <quit>");
                //comando = Console.ReadLine();

                //Contador_SalaReunioes = GlobalData.DictSalaReunioes.Count;
                //Contador_SalaPrincipal = GlobalData.DictSalaPrincipal.Count;
                //Contador_CorredorBaias = GlobalData.DictCorredorBaias.Count;

                switch (sala)
                {
                    case "Sala Principal":
                        //Console.WriteLine("Ligar LED");
                        estado = GlobalData.AcionamtSalaPrincipal;
                        _serialPort.Write(estado.ToString());
                        break;

                    case "Sala Reuniões":
                        //Console.WriteLine("Ligar LED");
                        estado = GlobalData.AcionamtSalaReunioes;
                        _serialPort.Write(estado.ToString());
                        break;

                    case "Corredor de Baias":
                        estado = GlobalData.AcionamtCorredorBaias;
                        _serialPort.Write(estado.ToString());
                        break;

                    default:
                        //Console.WriteLine("Comando não identificado");
                        break;
                }
            }
            _serialPort.Close();
        }

        public static void EscreveSerial(string sala,string estado)
        {
            _serialPort.Write(estado);
        }
    }
}


