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
    
    // Programa principal.
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
                Console.WriteLine("Iniciando programa...");
                GlobalData.filehandler.SetFileHandler();
                GlobalData.filehandler.CreateFile();


                // Verifica as TAGs que estao registradas na lista de TAGs
                //foreach (var tags in GlobalData.ListaTAGs)
                //{
                //    Console.WriteLine("Tag: {0}, {1}", tags.EPC, tags.Nome);
                //}
                
                // Hostname de todas as leitores utilizadas
                //string hostname1 = "SpeedwayR-10-9F-3F.local";
                //string hostname2 = "SpeedwayR-10-9F-C8.local";
                string hostname3 = "SpeedwayR-10-9F-BB.local";

                // Adicionando todas as leitoras à lista
                //readers.Add(new ImpinjReader(hostname1, "Reader #1"));
                //readers.Add(new ImpinjReader(hostname2, "Reader #2"));
                readers.Add(new ImpinjReader(hostname3, "Reader #3"));

                GlobalData.FlagPrograma = 1;

                foreach (ImpinjReader reader in readers)
                {
                    // Conecta com o reader.
                    // Troque o ReaderHostname em ConstantesReader.cs para o endereço IP ou hostname do seu reader.
                    //reader.Connect(ConstantesReader.ReaderHostname);
                    reader.Connect();

                    // Get the default settings
                    // We'll use these as a starting point and then modify the settings we're interested in.
                    Settings settings = reader.QueryDefaultSettings();

                    // Tell the reader to include the RF doppler frequency in all tag reports.
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
                }

                // Wait for the user to press enter.
                Console.WriteLine("Programa Iniciado.\nPressione Enter para sair do programa.\n");
                Console.ReadLine();

                // Para de ler e encerra a conexao com os readers
                foreach (ImpinjReader reader in readers)
                {
                    // Stop reading.
                    reader.Stop();

                    // Disconnect from the reader.
                    reader.Disconnect();
                }
                
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
                                GlobalData.ContadorPassagemAnt1 += 1;
                            }
                        }

                        // Atualiza o valor minimo da frequência doppler e seu tempo.
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
                                GlobalData.ContadorPassagemAnt2 += 1;
                            }
                        }

                        // Atualiza o valor minimo da frequência doppler e seu tempo.
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
                            Console.WriteLine("\n\nLeitora endereço: {0}", sender.Address);

                            bool passagem = true;

                            // Verifica o EPC das tags que estao registradas para verificar quem entrou no ambiente,
                            // e ai passa o nome da pessoa para a funcao MonitoramentoSALA.
                            foreach (var tags in GlobalData.ListaTAGs)
                            {
                                if (tags.EPC == tag.Epc.ToString())
                                {
                                    TransicaoTags.transicao_sala(sender.Address, tag.Epc.ToString(), passagem, tags.Nome, tags);
                                }
                            }

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
                            Console.WriteLine("\n\nLeitora endereço: {0}", sender.Address);

                            bool passagem = false;

                            // Verifica o EPC das tags que estao registradas para verificar quem saiu do ambiente,
                            // e ai passa o nome da pessoa para a funcao MonitoramentoSALA.
                            foreach (var tags in GlobalData.ListaTAGs)
                            {
                                if (tags.EPC == tag.Epc.ToString())
                                {
                                    TransicaoTags.transicao_sala(sender.Address, tag.Epc.ToString(), passagem, tags.Nome, tags);
                                }
                            }

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
                    GlobalData.filehandler.WriteToFile(tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"), tag.PeakRssiInDbm.ToString(), GlobalData.ContadorPassagemAnt1.ToString(), GlobalData.ContadorPassagemAnt2.ToString());
                }
            }
        }
    }



    // Classe que faz o monitoramento das TAGs, verificando se a flag que indica que uma transicao entre ambientes foi setada.
    public class MonitoramentoTAG
    {
      
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
                    MonitoramentoTAG.ResetaTAG();
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



        public void WriteToFile(string epc, string reader, ushort ant, string doppler_frequency, string peak_rssi, string contador1, string contador2)
        {
            // Set File parameters
            //string csvSeparator = "\t";
            string csvSeparator = ";";

            StringBuilder streamOutput = new StringBuilder();

            var antenna = Tuple.Create<string, ushort>(reader, ant);
            //doppler_frequency = String.Concat(csvsignal, doppler_frequency,csvsignal);

            string[][] dataOutput = new string[][]{
                                    new string[]{ epc, reader, ant.ToString(), DateTime.Now.ToString("dd/MM/yyyy; HH:mm:ss:fff"), doppler_frequency, peak_rssi, contador1, contador2}
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
                                    new string[]{"EPC", "Leitora", "Antena", "Data", "Horario","Frequencia Doppler", "RSSI", "Contador Passagem Ant1", "Contador Passagem Ant2" }
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


