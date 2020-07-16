////////////////////////////////////////////////////////////////////////////////
//
//      TG2 Leo e Kenneth
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Impinj.OctaneSdk;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace TeGe2
{
    class GlobalData
    {
        // Cria um objeto filehandler
        public static FileHandler filehandler = new FileHandler();

        //Variavel que guarda o valor minimo de RSSI para considerar a leitura.
        public static int FiltroPassaBaixaRSSI = -60;

        // Variavel que armazena o valor de RSSI que indica que alguém passou na frente da antena.
        public static int IndicadorRSSIPassagem = -47;

        // Variavel que armazena o DateTime da ultima vez que a TAG foi lida
        public static DateTime LastSeen = DateTime.Now;

        // Flag que armazena se houve uma transição entre ambientes (se saiu ou entrou)
        public static int FlagTrocaAmbiente = 0;

        // Flag que indica se o programa continua rodando ou não.
        // É utilizado para fechar a thread MonitoraTag quando alguem pressiona Enter
        public static int FlagPrograma;

        // Flag que indica se os reports da TAG estão sendo aceitos ou descartados
        public static int TAGsReportsON = 1;

        //Variaveis relacionadas aos dados da primeira antena.
        public static double PicoRSSIAnt1 = -63;
        public static string TempoPicoRSSIAnt1;
        public static DateTime DateTimePicoRSSIAnt1;
        public static double PicoNegFreqDopAnt1 = 0;
        public static string TempoPicoNegFreqDopAnt1;
        public static DateTime DateTimePicoNegFreqDopAnt1;
        public static int FlagPassagemAnt1 = 0;
        public static int LockPicoRSSIAnt1 = 0;

        //Variaveis relacionadas aos dados da segunda antena.
        public static double PicoRSSIAnt2 = -63;
        public static string TempoPicoRSSIAnt2;
        public static DateTime DateTimePicoRSSIAnt2;
        public static double PicoNegFreqDopAnt2 = 0;
        public static string TempoPicoNegFreqDopAnt2;
        public static DateTime DateTimePicoNegFreqDopAnt2;
        public static int FlagPassagemAnt2 = 0;
        public static int LockPicoRSSIAnt2 = 0;

        //Dicionário com todas as EPCs das tags e as respectivas salas em que se encontram
        public static Dictionary<string, int> tag_sala = new Dictionary<string, int>();
    }

    static class constantes
    {
        public const int AmbienteExterno = 0;
        public const int SalaPrincipal = 1;
        public const int SalaDeReunioes = 2;
        public const int CorredorDeBaias = 3;
    }

    class Program
    {
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        protected static List<ImpinjReader> readers = new List<ImpinjReader>();

        static void Main(string[] args)
        {
            try
            {
                // Ao iniciar o programa, seta o fileHandler e cria 
                // os arquivos que armazenam os dados da leitura.
                Console.WriteLine("Programa iniciado");
                GlobalData.filehandler.SetFileHandler();
                GlobalData.filehandler.CreateFile();

                //Dados de todas as leitores utilizadas
                string hostname1 = "SpeedwayR-10-9F-3F.local";
                string hostname2 = "SpeedwayR-10-9F-C8.local";
                //string hostname3 = "SpeedwayR-10-9F-BB.local";

                //Adicionando todas as leitoras à lista
                readers.Add(new ImpinjReader(hostname1, "Reader #1"));
                readers.Add(new ImpinjReader(hostname2, "Reader #2"));
                //readers.Add(new ImpinjReader(hostname3, "Reader #3"));

                GlobalData.FlagPrograma = 1;

                // Conecta com o reader.
                // Troque o ReaderHostname em ConstantesReader.cs
                // para o endereço IP ou hostname do seu reader.
                reader.Connect(ConstantesReader.ReaderHostname);

                // Get the default settings
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                Settings settings = reader.QueryDefaultSettings();

                // Tell the reader to include the
                // RF doppler frequency in all tag reports. 
                settings.Report.IncludeDopplerFrequency = true;
                settings.Report.IncludePeakRssi = true;
                settings.Report.IncludeAntennaPortNumber = true;
                settings.Report.IncludeLastSeenTime = true;

                // Use antenna #1 and #2
                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(2).IsEnabled = true;

                //settings.Antennas.TxPowerMax = true;
                //settings.Antennas.RxSensitivityMax = true;
                //settings.Antennas.TxPowerInDbm = 20.0;
                //settings.Antennas.RxSensitivityInDbm = -70.0;

                // ReaderMode must be set to DenseReaderM8.
                settings.ReaderMode = ReaderMode.DenseReaderM8;

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                //Inicializa a Thread que faz o monitoramento das TAGs
                Thread MonitoraThread = new Thread(MonitoramentoTAG.MonitoraTag);
                MonitoraThread.Start();

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += AnalisaDadosTAG;

                // Start reading.
                reader.Start();

                // Wait for the user to press enter.
                Console.WriteLine("Pressione Enter para sair do programa.\n");
                Console.ReadLine();

                // Stop reading.
                reader.Stop();

                // Disconnect from the reader.
                reader.Disconnect();
                GlobalData.FlagPrograma = 0;
            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }



        static void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            // (Tag tag in report)
            //---------------------------------------------

            //filePath = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log_" + DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".csv";

            //GlobalData.filehandler.WriteToFile(tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"));

            //string csvSeperator = ";";
            //StringBuilder streamOutput = new StringBuilder();

        }



        // Funcao que faz a analise dos reports das TAGs.
        private static void AnalisaDadosTAG(ImpinjReader sender, TagReport report)
        {
            foreach (Tag tag in report)
            {

                // Condicional com um filtro para validar leitura. Valor de RSSI tem que estar acima
                // do valor do filtro e TAGsReportsON tem que ser verdadeiro.
                if (tag.PeakRssiInDbm > GlobalData.FiltroPassaBaixaRSSI && GlobalData.TAGsReportsON == 1)
                {
                    // Checa se a antena 1 que está lendo
                    if (tag.AntennaPortNumber == 1)
                    {
                        // Se o RSSI lido do report for maior que o pico atual 
                        // de RSSI, atualiza o valor máximo de RSSI e seu tempo.
                        if (tag.PeakRssiInDbm > GlobalData.PicoRSSIAnt1 && GlobalData.LockPicoRSSIAnt1 == 0)
                        {
                            GlobalData.PicoRSSIAnt1 = tag.PeakRssiInDbm;
                            GlobalData.TempoPicoRSSIAnt1 = DateTime.Now.ToString("HH-mm-ss-fff");
                            GlobalData.DateTimePicoRSSIAnt1 = DateTime.Now;

                            // Verifica se o pico de RSSI da antena 1 é maior ou igual que o indicador RSSI de passagem.
                            // Se for, atualiza a flag que indica que houve passagem pela primeira antena e
                            // da "lock" nesse valor, para que nao pegue outras medidas desnecessariamente.
                            if (GlobalData.PicoRSSIAnt1 >= GlobalData.IndicadorRSSIPassagem)
                            {
                                GlobalData.FlagPassagemAnt1 = 1;
                                GlobalData.LockPicoRSSIAnt1 = 1;
                            }
                        }

                        // Atualiza o valor mínimo da frequência doppler e seu tempo.
                        if (tag.RfDopplerFrequency < GlobalData.PicoNegFreqDopAnt1)
                        {
                            GlobalData.PicoNegFreqDopAnt1 = tag.RfDopplerFrequency;
                            GlobalData.TempoPicoNegFreqDopAnt1 = DateTime.Now.ToString("HH-mm-ss-fff");
                            GlobalData.DateTimePicoNegFreqDopAnt1 = DateTime.Now;
                        }

                    }

                    // Checa se a antena 2 que está lendo.
                    if (tag.AntennaPortNumber == 2)
                    {
                        // Se o RSSI lido do report for maior que o pico atual 
                        // de RSSI, atualiza o valor máximo de RSSI e seu tempo.
                        if (tag.PeakRssiInDbm > GlobalData.PicoRSSIAnt2 && GlobalData.LockPicoRSSIAnt2 == 0)
                        {
                            GlobalData.PicoRSSIAnt2 = tag.PeakRssiInDbm;
                            GlobalData.TempoPicoRSSIAnt2 = DateTime.Now.ToString("HH-mm-ss-fff");
                            GlobalData.DateTimePicoRSSIAnt2 = DateTime.Now;

                            // Verifica se o pico de RSSI da antena 1 é maior ou igual que o indicador RSSI de passagem.
                            // Se for, atualiza a flag que indica que houve passagem pela primeira antena e
                            // da "lock" nesse valor, para que nao pegue outras medidas desnecessariamente.
                            if (GlobalData.PicoRSSIAnt2 >= GlobalData.IndicadorRSSIPassagem)
                            {
                                GlobalData.FlagPassagemAnt2 = 1;
                                GlobalData.LockPicoRSSIAnt2 = 1;
                            }
                        }

                        // Atualiza o valor mínimo da frequência doppler e seu tempo.
                        if (tag.RfDopplerFrequency < GlobalData.PicoNegFreqDopAnt2)
                        {
                            GlobalData.PicoNegFreqDopAnt2 = tag.RfDopplerFrequency;
                            GlobalData.TempoPicoNegFreqDopAnt2 = DateTime.Now.ToString("HH-mm-ss-fff");
                            GlobalData.DateTimePicoNegFreqDopAnt2 = DateTime.Now;
                        }

                    }

                    // Checa se a flag de passagem das duas antenas foram setadas.
                    // Se sim, significa que houve a passagem da TAG pelas duas antenas.
                    // Ou seja, significa que alguem entrou ou saiu do ambiente.
                    if (GlobalData.FlagPassagemAnt1 == 1 && GlobalData.FlagPassagemAnt2 == 1)
                    {
                        // Checa os tempos de cada pico para saber se a TAG entrou ou saiu do ambiente.
                        // Se a comparacao for negativa, significa que passaram primeiro pela antena 1 e depois
                        // pela antena 2, ou seja, a pessoa entrou no ambiente.
                        if (DateTime.Compare(GlobalData.DateTimePicoRSSIAnt1, GlobalData.DateTimePicoRSSIAnt2) < 0)
                        {
                            Console.WriteLine("**********************************");
                            Console.WriteLine("\nINDIVIDUO ENTROU NO AMBIENTE\n");
                            Console.WriteLine("Leitora endereço: {0}", sender.Address);
                            Console.WriteLine("**********************************");

                            bool passagem = true;

                            MonitoramentoSALA.transicao_sala(sender.Address, tag.Epc.ToString(), passagem);

                            // Seta a flag que indica que esta ocorrendo uma transicao entre ambientes.
                            GlobalData.FlagTrocaAmbiente = 1;

                            // Inicializa a Thread que faz o descarte de reports das tags por 2 segundos.
                            // Isso eh feito para que os dados dos reports durante a transicao da pessoa
                            // nao atrapalhem a analise.  
                            GlobalData.TAGsReportsON = 0;
                            Thread DescartaReportThread = new Thread(MonitoramentoTAG.DescartaReport);
                            DescartaReportThread.Start();
                        }

                        // Checa os tempos de cada pico para saber se a TAG entrou ou saiu do ambiente.
                        // Se a comparacao for positiva, significa que passaram primeiro pela antena 2 e depois
                        // pela antena 1, ou seja, a pessoa saiu do ambiente.
                        if (DateTime.Compare(GlobalData.DateTimePicoRSSIAnt1, GlobalData.DateTimePicoRSSIAnt2) > 0)
                        {
                            Console.WriteLine("******************************");
                            Console.WriteLine("\nINDIVIDUO SAIU DO AMBIENTE\n");
                            Console.WriteLine("Leitora endereço: {0}", sender.Address);
                            Console.WriteLine("******************************");

                            bool passagem = false;

                            // Seta a flag que indica que esta ocorrendo uma transicao entre ambientes.
                            GlobalData.FlagTrocaAmbiente = 1;

                            //Inicializa a Thread que faz o descarte de reports das tags por 2 segundos
                            GlobalData.TAGsReportsON = 0;
                            Thread DescartaReportThread = new Thread(MonitoramentoTAG.DescartaReport);
                            DescartaReportThread.Start();
                        }
                    }

                    // Armazena o DateTime do tempo da ultima vez que foi feito uma leitura valida.
                    GlobalData.LastSeen = DateTime.Now;

                    //Escreve nos arquivos os dados.
                    GlobalData.filehandler.WriteToFile(tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"), tag.PeakRssiInDbm.ToString());

                    // Escreve no prompt os dados obtidos.
                    //Console.WriteLine("EPC : {0},      Name:{1},     Antenna:{2},    Frequencia Doppler:{3},    RSSI:{4}", tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"), tag.PeakRssiInDbm.ToString());

                    //Console.WriteLine("Pico atual de RSSI da ANTENA 1 : {0}", GlobalData.PicoRSSIAnt1);
                    //Console.WriteLine("Tempo do pico RSSI da ANTENA 1 : {0}", GlobalData.TempoPicoRSSIAnt1);
                    //Console.WriteLine("Pico negativo atual da FREQUENCIA Doppler da ANTENA 1 : {0}", GlobalData.PicoNegFreqDopAnt1);
                    //Console.WriteLine("Tempo do pico negativo da FREQUENCIA Doppler da ANTENA 1 : {0}\n", GlobalData.TempoPicoNegFreqDopAnt1);

                    //Console.WriteLine("Pico atual de RSSI da ANTENA 2 : {0}", GlobalData.PicoRSSIAnt2);
                    //Console.WriteLine("Tempo do pico RSSI da ANTENA 2 : {0}", GlobalData.TempoPicoRSSIAnt2);
                    //Console.WriteLine("Pico negativo atual da Frequencia Doppler da ANTENA 2 : {0}", GlobalData.PicoNegFreqDopAnt2);
                    //Console.WriteLine("Tempo do pico negativo da FREQUENCIA Doppler da ANTENA 2 : {0}", GlobalData.TempoPicoNegFreqDopAnt2);
                    //Console.WriteLine("----------------------------------------------------------------------------------------------------------");

                }

                //TimeSpan duration = DateTime.Parse(DateTime.Now.ToString()).Subtract(DateTime.Parse(GlobalData.LastSeen.ToString()));
                //Console.WriteLine("Variavel DURATION : {0}\n", duration.TotalSeconds);
                //if (duration.TotalSeconds >= 5)
                //{
                //    MonitoramentoTAG.ResetaTAG();

                // Escreve no prompt os dados obtidos.
                //Console.WriteLine("EPC : {0},      Name:{1},     Antenna:{2},    Frequencia Doppler:{3},    RSSI:{4}", tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"), tag.PeakRssiInDbm.ToString());

                //Console.WriteLine("Pico atual de RSSI da ANTENA 1 : {0}", GlobalData.PicoRSSIAnt1);
                //Console.WriteLine("Tempo do pico RSSI da ANTENA 1 : {0}", GlobalData.TempoPicoRSSIAnt1);
                //Console.WriteLine("Pico negativo atual da FREQUENCIA Doppler da ANTENA 1 : {0}", GlobalData.PicoNegFreqDopAnt1);
                //Console.WriteLine("Tempo do pico negativo da FREQUENCIA Doppler da ANTENA 1 : {0}\n", GlobalData.TempoPicoNegFreqDopAnt1);

                //Console.WriteLine("Pico atual de RSSI da ANTENA 2 : {0}", GlobalData.PicoRSSIAnt2);
                //Console.WriteLine("Tempo do pico RSSI da ANTENA 2 : {0}", GlobalData.TempoPicoRSSIAnt2);
                //Console.WriteLine("Pico negativo atual da Frequencia Doppler da ANTENA 2 : {0}", GlobalData.PicoNegFreqDopAnt2);
                //Console.WriteLine("Tempo do pico negativo da FREQUENCIA Doppler da ANTENA 2 : {0}", GlobalData.TempoPicoNegFreqDopAnt2);
                //Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                //}
            }
        }
    }



    public class MonitoramentoTAG
    {
        // Faz o monitoramento das TAGs, verificando se a flag que indica
        // uma transicao entre ambientes foi setada.
        public static void MonitoraTag()
        {
            // Roda essa thread enquanto o programa estiver vivo.
            // Caso seja apertado Enter, a flag vai para zero e a thread eh encerrada.
            while (GlobalData.FlagPrograma == 1)
            {

                // Verifica se a flag que indica transicao de ambientes foi setada.
                // Se sim, faz o reset das variaveis da TAG.
                if (GlobalData.FlagTrocaAmbiente == 1)
                {
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("Pico atual de RSSI da ANTENA 1 : {0}", GlobalData.PicoRSSIAnt1);
                    Console.WriteLine("Tempo do pico RSSI da ANTENA 1 : {0}", GlobalData.TempoPicoRSSIAnt1);
                    Console.WriteLine("Pico atual de RSSI da ANTENA 2 : {0}", GlobalData.PicoRSSIAnt2);
                    Console.WriteLine("Tempo do pico RSSI da ANTENA 2 : {0}", GlobalData.TempoPicoRSSIAnt2);
                    Console.WriteLine("------------------------------------------------");
                    MonitoramentoTAG.ResetaTAG();

                    // Escreve no prompt os dados obtidos.
                    //Console.WriteLine("Pico atual de RSSI da ANTENA 1 : {0}", GlobalData.PicoRSSIAnt1);
                    //Console.WriteLine("Tempo do pico RSSI da ANTENA 1 : {0}", GlobalData.TempoPicoRSSIAnt1);
                    //Console.WriteLine("Pico negativo atual da FREQUENCIA Doppler da ANTENA 1 : {0}", GlobalData.PicoNegFreqDopAnt1);
                    //Console.WriteLine("Tempo do pico negativo da FREQUENCIA Doppler da ANTENA 1 : {0}\n", GlobalData.TempoPicoNegFreqDopAnt1);

                    //Console.WriteLine("Pico atual de RSSI da ANTENA 2 : {0}", GlobalData.PicoRSSIAnt2);
                    //Console.WriteLine("Tempo do pico RSSI da ANTENA 2 : {0}", GlobalData.TempoPicoRSSIAnt2);
                    //Console.WriteLine("Pico negativo atual da Frequencia Doppler da ANTENA 2 : {0}", GlobalData.PicoNegFreqDopAnt2);
                    //Console.WriteLine("Tempo do pico negativo da FREQUENCIA Doppler da ANTENA 2 : {0}", GlobalData.TempoPicoNegFreqDopAnt2);
                    //Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                }
                Thread.Sleep(100);
            }
        }


        // Reseta as variaveis da TAG.
        public static void ResetaTAG()
        {
            // Reseta as variaveis relacionadas aos dados da primeira antena.
            GlobalData.PicoRSSIAnt1 = -63;
            GlobalData.PicoNegFreqDopAnt1 = 0;
            GlobalData.FlagPassagemAnt1 = 0;
            GlobalData.LockPicoRSSIAnt1 = 0;

            // Reseta as variaveis relacionadas aos dados da segunda antena.
            GlobalData.PicoRSSIAnt2 = -63;
            GlobalData.PicoNegFreqDopAnt2 = 0;
            GlobalData.FlagPassagemAnt2 = 0;
            GlobalData.LockPicoRSSIAnt2 = 0;

            // Reseta a variavel que indica a troca de ambiente.
            GlobalData.FlagTrocaAmbiente = 0;
        }



        // Funcao que faz o descarte dos reports durante 2 segundos.
        // Seta TAGsReportsON para zero e faz com que AnalisaDadosTAGs
        // descarte os reports durante 2 segundos, depois seta para 1 de novo.
        public static void DescartaReport()
        {
            GlobalData.TAGsReportsON = 0;
            Thread.Sleep(1000);
            GlobalData.TAGsReportsON = 1;
        }
    }

    public class MonitoramentoSALA {
        public static void transicao_sala(string leitora, string tag_epc, bool passagem)
        {
            switch (leitora)
            {
                case "SpeedwayR-10-9F-C8.local":
                    if (passagem == true)
                    {
                        GlobalData.tag_sala[tag_epc] = constantes.SalaPrincipal;
                        Console.WriteLine("Você está na Sala Principal");
                    }
                    else
                    {
                        GlobalData.tag_sala[tag_epc] = constantes.AmbienteExterno;
                        Console.WriteLine("Você está no Ambiente Externo");
                    }
                    break;

                case "SpeedwayR-10-9F-3F.local":
                    if (passagem == true)
                    {
                        GlobalData.tag_sala[tag_epc] = constantes.SalaDeReunioes;
                        Console.WriteLine("Você está na Sala De Reuniões");
                    }
                    else
                    {
                        GlobalData.tag_sala[tag_epc] = constantes.SalaPrincipal;
                        Console.WriteLine("Você está na Sala Principal");
                    }
                    break;

                case "SpeedwayR-10-9F-BB.local":
                    if (passagem == true)
                    {
                        GlobalData.tag_sala[tag_epc] = constantes.CorredorDeBaias;
                        Console.WriteLine("Você está no Corredor De Baias");
                    }
                    else
                    {
                        GlobalData.tag_sala[tag_epc] = constantes.SalaPrincipal;
                        Console.WriteLine("Você está na Sala Principal");
                    }
                    break;
            }
        }
    }

    
    // Classe que lida com o armazenamento dos dados em arquivos.
    public class FileHandler
    {

        protected string filePathCsv;
        protected string filePathTxt;



        public void SetFileHandler()
        {
            filePathCsv = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\logcsv_" + DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".csv";

            filePathTxt = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\logtxt_" + DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".txt";
        }



        public void WriteToFile(string epc, string reader, ushort ant, string doppler_frequency, string peak_rssi)
        {
            // Set File parameters
            //string csvSeparator = "\t";
            string csvSeparator = ";";

            StringBuilder streamOutput = new StringBuilder();

            var antenna = Tuple.Create<string, ushort>(reader, ant);
            //doppler_frequency = String.Concat(csvsignal, doppler_frequency,csvsignal);

            string[][] dataOutput = new string[][]{
                                    new string[]{ epc, reader, ant.ToString(), DateTime.Now.ToString("dd/MM/yyyy; HH:mm:ss:fff"), doppler_frequency, peak_rssi }
                                    };
            int length = dataOutput.GetLength(0);
            for (int i = 0; i < length; i++)
                streamOutput.AppendLine(string.Join(csvSeparator, dataOutput[i]));

            string TxtData = string.Format("Pico RSSI da ANTENA 1 : {0} \nTempo do pico da ANTENA 1 : {1} \nPico negativo da Frequencia Doppler da ANTENA 1 : {2} \nTempo do pico negativo da Frequencia Doppler da ANTENA 1 : {3} \n\nPico RSSI da ANTENA 2 : {4} \nTempo do pico da ANTENA 2 : {5} \nPico negativo da Frequencia Doppler da ANTENA 2 : {6} \nTempo do pico negativo da Frequencia Doppler da ANTENA 2 : {7}", GlobalData.PicoRSSIAnt1, GlobalData.TempoPicoRSSIAnt1, GlobalData.PicoNegFreqDopAnt1, GlobalData.TempoPicoNegFreqDopAnt1, GlobalData.PicoRSSIAnt2, GlobalData.TempoPicoRSSIAnt2, GlobalData.PicoNegFreqDopAnt2, GlobalData.TempoPicoNegFreqDopAnt2);

            // Appends more lines to the csv file.
            File.AppendAllText(filePathCsv, streamOutput.ToString());
            //File.WriteAllText(filePathTxt, TxtData);
        }



        public void CreateFile()
        {
            // Set File parameters
            //string csvSeparator = "\t";
            string csvSeparator = ";";
            StringBuilder streamOutput = new StringBuilder();

            string[][] dataOutput = new string[][]{
                                    new string[]{"EPC", "Leitora", "Antena", "Data", "Horario","Frequencia Doppler", "RSSI"}
                                    };
            int length = dataOutput.GetLength(0);
            for (int i = 0; i < length; i++)
                streamOutput.AppendLine(string.Join(csvSeparator, dataOutput[i]));

            string TxtData = string.Format("Pico RSSI da ANTENA 1 : {0} \nTempo do pico da ANTENA 1 : {1} \nPico negativo da Frequencia Doppler da ANTENA 1 : {2} \nTempo do pico negativo da Frequencia Doppler da ANTENA 1 : {3} \n\nPico RSSI da ANTENA 2 : {4} \nTempo do pico da ANTENA 2 : {5} \nPico negativo da Frequencia Doppler da ANTENA 2 : {6} \nTempo do pico negativo da Frequencia Doppler da ANTENA 2 : {7}", GlobalData.PicoRSSIAnt1, GlobalData.TempoPicoRSSIAnt1, GlobalData.PicoNegFreqDopAnt1, GlobalData.TempoPicoNegFreqDopAnt1, GlobalData.PicoRSSIAnt2, GlobalData.TempoPicoRSSIAnt2, GlobalData.PicoNegFreqDopAnt2, GlobalData.TempoPicoNegFreqDopAnt2);

            // Create and write the csv and txt file.
            File.WriteAllText(filePathCsv, streamOutput.ToString());
            //File.WriteAllText(filePathTxt, TxtData);
        }
    }
}


