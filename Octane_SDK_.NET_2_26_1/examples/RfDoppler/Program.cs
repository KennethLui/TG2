////////////////////////////////////////////////////////////////////////////////
//
//    RF Doppler
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Impinj.OctaneSdk;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace OctaneSdkExamples
{
    class GlobalData
    {
        public static int RSSILowPassFilter = -60;

        public static FileHandler filehandler = new FileHandler();

        public static List<double> RFdopplerlist = new List<double>();

        public static Dictionary<string, List<double>> leituras_tag = new Dictionary<string, List<double>>();

        public static int counter, aux = 0, ultima_ant;

        public static DateTime tempo_passagem_aux, tempo_passagem_ant1, tempo_passagem_ant2;

    }

    class Program
    {
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();

        static void Main(string[] args)
        {

            try
            {
                Console.WriteLine("Começou");
                GlobalData.filehandler.SetFileHandler();
                GlobalData.filehandler.CreateFile();

                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(SolutionConstants.ReaderHostname);

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

                //Low Duty Cicle -- inicio
                //settings.Report.Mode = ReportMode.Individual;

                //LowDutyCycleSettings ldc = new LowDutyCycleSettings();
                //ldc.EmptyFieldTimeoutInMs = 100;
                //ldc.FieldPingIntervalInMs = 60;
                //ldc.IsEnabled = true;
                //settings.LowDutyCycle = ldc;
                //Low Duty Cicle -- fim

                // Use antenna #1
                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(2).IsEnabled = true;

                // ReaderMode must be set to DenseReaderM8.
                settings.ReaderMode = ReaderMode.DenseReaderM8;
                //settings.ReaderMode = ReaderMode.MaxThroughput;
                //settings.ReaderMode = ReaderMode.AutoSetDenseReader;

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += Captura_tags;

                // Start reading.
                reader.Start();
                Console.WriteLine("\nStart Reading!");

                // Wait for the user to press enter.
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();

                // Stop reading.
                reader.Stop();

                // Disconnect from the reader.
                reader.Disconnect();
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
            //{
            //    Console.WriteLine("EPC : {0} Doppler Frequency (Hz) : {1}",
            //                        tag.Epc, tag.RfDopplerFrequency.ToString("0.00"));
            //}

            //---------------------------------------------

            //filePath = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log_" + DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".csv";

            //GlobalData.filehandler.WriteToFile(tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"));

            //string csvSeperator = ";";
            //StringBuilder streamOutput = new StringBuilder();

        }

        private static void Captura_tags(ImpinjReader sender, TagReport report)
        {
            foreach(Tag tag in report)
            {
                //Console.WriteLine("Entrou em captura tags");
                GlobalData.filehandler.WriteToFile(tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"), tag.PeakRssiInDbm.ToString());

                //GlobalData.RFdopplerlist.Add(tag.RfDopplerFrequency);
                //GlobalData.leituras_tag[tag.Epc.ToString()] = GlobalData.RFdopplerlist;

                DateTime Agora = DateTime.Now;

                if (tag.RfDopplerFrequency > 4)
                {
                    TimeSpan intervalo = Agora - GlobalData.tempo_passagem_aux;
                    if (intervalo.TotalMilliseconds > 700)
                    {
                        GlobalData.counter++;
                        switch (tag.AntennaPortNumber)
                        {
                            case 1:
                                GlobalData.tempo_passagem_ant1 = Agora;
                                Console.WriteLine($"\nPassou na antena 1 em {Agora}");
                                if (GlobalData.ultima_ant == 2)
                                {
                                    Console.WriteLine("\nFOI PRA DIREITA");
                                }
                                if (GlobalData.ultima_ant == 1)
                                {
                                    Console.WriteLine("\nLEITURA REPETIDA");
                                }
                                GlobalData.ultima_ant = 1;
                                break;

                            case 2:
                                GlobalData.tempo_passagem_ant2 = Agora;
                                Console.WriteLine($"\nPassou na antena 2 em {Agora}");
                                if (GlobalData.ultima_ant == 1)
                                {
                                    Console.WriteLine("\nFOI PRA ESQUERDA");
                                }
                                if (GlobalData.ultima_ant == 2)
                                {
                                    Console.WriteLine("\nLEITURA REPETIDA");
                                }
                                GlobalData.ultima_ant = 2;
                                break;

                            default:
                                break;
                        }
                        GlobalData.tempo_passagem_aux = Agora;
                    }
                }

                // Debug
                // Console.WriteLine("RSSI: {0}, Doppler: {1}, 0:{2}, 2:{3}, 3:{4}, 4:{5}, 5:{6}, 6:{7}", tag.PeakRssiInDbm, tag.RfDopplerFrequency, individuo.GetAmbient(0).GetName(), individuo.GetAmbient(2).GetName(), individuo.GetAmbient(3).GetName(), individuo.GetAmbient(4).GetName(), individuo.GetAmbient(5).GetName(), individuo.GetAmbient(6).GetName());
                // Debug.end
                //Console.WriteLine("EPC : {0},   Antenna:{2},    Frequência Doppler:{3},    RSSI:{4}", tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber, tag.RfDopplerFrequency.ToString("0.00"), tag.PeakRssiInDbm.ToString());
            }
            

        }
    }

    

    public class FileHandler
    {

        protected string filePath;

        public void SetFileHandler()
        {
            filePath = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log_" + DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".csv";

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
                                    new string[]{ epc, reader, ant.ToString(), DateTime.Now.ToString("dd/MM/yyyy; HH:mm:ss:ffff"), doppler_frequency, peak_rssi, GlobalData.counter .ToString()}
                                    };
            int length = dataOutput.GetLength(0);
            for (int i = 0; i < length; i++)
                streamOutput.AppendLine(string.Join(csvSeparator, dataOutput[i]));

            // Appends more lines to the csv file
            File.AppendAllText(filePath, streamOutput.ToString());
        }

        public void CreateFile()
        {
            // Set File parameters
            //string csvSeparator = "\t";
            string csvSeparator = ";";
            StringBuilder streamOutput = new StringBuilder();

            string[][] dataOutput = new string[][]{
                                    new string[]{"EPC", "Leitora", "Antena", "Data", "Horario","Frequencia Doppler", "RSSI", "Passagens"}
                                    };
            int length = dataOutput.GetLength(0);
            for (int i = 0; i < length; i++)
                streamOutput.AppendLine(string.Join(csvSeparator, dataOutput[i]));

            // Create and write the csv file
            File.WriteAllText(filePath, streamOutput.ToString());
        }


    }
}


