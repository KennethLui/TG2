/*
 * Trabalho de Graduação II - Controle antecipativo de sistemas de conforto térmico usando RFID
 * 
 * Autor: André Abreu Rodrigues de Almeida
 *        Graduando em Engenharia Mecatrônica
 *        Matrícula: 12/0007100
 * Departamento de Engenharia Elétrica
 * Universidade de Brasília - UnB
 * 
 * 
 * Brasília, Agosto a Dezembro de 2019
 * 
 */

/* Aviso: A biblioteca 'Octane SDK' foi desenvolvida e é propriedade da empresa IMPINJ, INC., e está sendo utilizada com base na licença de software aberto descrita a seguir
 * 
 * Para mais informações, visitar o site: https://support.impinj.com/hc/en-us/articles/360000468370-Software-Tools-License-Disclaimer
 */


/*PLEASE READ THE FOLLOWING LICENSE & DISCLAIMER (“AGREEMENT”) CAREFULLY BEFORE USING ANY SOFTWARE TOOLS (AS DEFINED BELOW) MADE AVAILABLE TO YOU (“LICENSEE”) BY IMPINJ, INC. (“IMPINJ”). BY USING THE SOFTWARE TOOLS, YOU ACKNOWLEDGE THAT YOU HAVE READ AND UNDERSTOOD ALL THE TERMS AND CONDITIONS OF THE AGREEMENT, YOU WILL BE CONSENTING TO BE BOUND BY THEM, AND YOU ARE AUTHORIZED TO DO SO. IF YOU DO NOT ACCEPT THESE TERMS AND CONDITIONS, DO NOT USE THE SOFTWARE TOOLS.

1. PURPOSE OF AGREEMENT. From time to time, Impinj technical personnel may make available to Licensee certain software, including code (in source and object form), tools, libraries, configuration files, translations, and related documentation (collectively, “Software Tools”), upon specific request or to assist with a specific deployment. This Agreement sets forth Licensee's limited rights and Impinj's limited obligations with respect to the Software Tools. Licensee acknowledges that Impinj provides the Software Tools free of charge. This Agreement does not grant any rights with respect to Impinj standalone software products (e.g., ItemSense, ItemEncode, SpeedwayConnect) or the firmware on Impinj hardware, all of which are subject to separate license terms.

2. LIMITED LICENSE. Subject to the terms and conditions of this Agreement, Impinj hereby grants to Licensee a limited, royalty-free, worldwide, non-exclusive, perpetual and irrevocable (except as set forth below), non-transferable license, without right of sublicense, to (a) use the Software Tools and (b) only with respect to Software Tools provided in source code form, modify and create derivative works of such Software Tools, in each case, solely for Licensee’s internal development related to the deployment of Impinj products (“Purpose”). The Software Tools may only be used by employees of Licensee that must have access to the Software Tools in connection with the Purpose.

3. TERMINATION. Impinj may immediately terminate this Agreement if Licensee breaches any provision hereof. Upon the termination of this Agreement, Licensee must (a) discontinue all use of the Software Tools, (b) uninstall the Software Tools from its systems, (c) destroy or return to Impinj all copies of the Software Tools and any other materials provided by Impinj, and (d) promptly provide Impinj with written confirmation (including via email) of Licensee’s compliance with these provisions. Sections 4-10 will survive termination of this Agreement.

4. OWNERSHIP. The Software Tools are licensed, not sold, by Impinj to Licensee. Impinj and its suppliers own and retain all right, title, and interest, including all intellectual property rights, in and to the Software Tools. Except for those rights expressly granted in this Agreement, no other rights are granted, either express or implied, to Licensee. Impinj reserves the right to develop, price and sell software products that have features similar to or competitive with Software Tools. Licensee grants Impinj a limited, royalty-free, worldwide, perpetual and irrevocable, transferable, sublicensable, license to Licensee’s derivative works of Software Tools; provided that Licensee has no obligation under this Agreement to deliver to Impinj any such derivative works.

5. CONFIDENTIALITY. In order to protect the trade secrets and proprietary know-how contained in the Software Tools, Licensee will not decompile, disassemble, or reverse engineer, or otherwise attempt to gain access to the source code or algorithms of the Software Tools (unless Impinj provides the Software Tools in source code format). Licensee will maintain the confidentiality of and not disclose to any third party: (a) all non-public information disclosed by Impinj to Licensee under this Agreement and (b) all performance data and all other information obtained through the Software Tools.

6. WARRANTY DISCLAIMER. LICENSEE AC KNOWLEDGES THAT IMPINJ PROVIDES THE SOFTWARE TOOLS FREE OF CHARGE AND ONLY FOR THE PURPOSE. ACCORDINGLY, THE SOFTWARE TOOLS ARE PROVIDED “AS IS” WITHOUT QUALITY CHECK, AND IMPINJ DOES NOT WARRANT THAT THE SOFTWARE TOOLS WILL OPERATE WITHOUT ERROR OR INTERRUPTION OR MEET ANY PERFORMANCE STANDARD OR OTHER EXPECTATION. IMPINJ EXPRESSLY DISCLAIMS ALL WARRANTIES, EXPRESS OR IMPLIED, INCLUDING THE IMPLIED WARRANTIES OF MERCHANTABILITY, NONINFRINGEMENT, QUALITY, ACCURACY, AND FITNESS FOR A PARTICULAR PURPOSE. IMPINJ IS NOT OBLIGATED IN ANY WAY TO PROVIDE SUPPORT OR OTHER MAINTENANCE WITH RESPECT TO THE SOFTWARE TOOLS.

7. LIMITATION OF LIABILITY. THE TOTAL LIABILITY OF IMPINJ ARISING OUT OF OR RELATED TO THE SOFTWARE TOOLS WILL NOT EXCEED THE TOTAL AMOUNT PAID BY LICENSEE TO IMPINJ PURSUANT TO THIS AGREEMENT. IN NO EVENT WILL IMPINJ HAVE LIABILITY FOR ANY INDIRECT, INCIDENTAL, SPECIAL, OR CONSEQUENTIAL DAMAGES, EVEN IF ADVISED OF THE POSSIBILITY OF THESE DAMAGES. THESE LIMITATIONS WILL APPLY NOTWITHSTANDING ANY FAILURE OF ESSENTIAL PURPOSE OF ANY LIMITED REMEDY IN THIS AGREEMENT.

8. THIRD PARTY SOFTWARE. The Software Tools may contain software created by a third party. Licensee’s use of any such third party software is subject to the applicable license terms and this Agreement does not alter those license terms. Licensee may not subject any portion of the Software Tools to an open source license.

9. RESTRICTED USE. Licensee will comply with all applicable laws and regulations to preclude the acquisition by any governmental agency of unlimited rights to technical data, software, and documentation provided with Software Tools, and include the appropriate “Restricted Rights” or “Limited Rights” notices required by the applicable U.S. or foreign government agencies. Licensee will comply in all respects with all U.S. and foreign export and re-export laws and regulations applicable to the technology and documentation provided hereunder.

10. MISCELLANEOUS. This Agreement will be governed by the laws of the State of Washington, U.S.A without reference to conflict of law principles. All disputes arising out of or related to it, will be subject to the exclusive jurisdiction of the state and federal courts located in King County, Washington, and the parties agree and submit to the personal and exclusive jurisdiction and venue of these courts. Licensee will not assign this Agreement, directly or indirectly, by operation of law or otherwise, without the prior written consent of Impinj. This Agreement (and any applicable nondisclosure agreement) is the entire agreement between the parties relating to the Software Tools. No waiver or modification of this Agreement will be valid unless contained in a writing signed by each party.
*/

#define DEBUG

using System;
using Impinj.OctaneSdk;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace TG2_RFID
{
    class GlobalData
    {
        public static int RSSILowPassFilter = -60;

        public static FileHandler filehandler = new FileHandler();
    }

    class Program
    {
        // Create a collection to hold all the ImpinjReader instances.
        protected static List<ImpinjReader> readers = new List<ImpinjReader>();

        static void Main(/*string[] args*/)
        {
            //Console.WriteLine("Test Step");
            //Curve cc = new Curve();
            //Curve.PopulateCurveTest(cc);
            //cc.PrintCurveInConsole();
            //Console.WriteLine("MeanY:{0}", cc.CalculateMeanY());
            //Console.WriteLine("MedianY:{0}", cc.GetMedianY());
            //var maxPoint = cc.GetCurveMaxPoint();
            //var minPoint = cc.GetCurveMinPoint();
            //Console.WriteLine("MaxPoint<{0}, {1}>", maxPoint.Item1, maxPoint.Item2);
            //Console.WriteLine("MinPoint<{0}, {1}>", minPoint.Item1, minPoint.Item2);
            //Console.WriteLine("MinX:{0}, MaxX:{1}", cc.GetCurveMinX(), cc.GetCurveMaxX());
            //var crossingPoint = cc.CalculateCrossingPoint();
            //Console.WriteLine("CrossingPoint<{0}, {1}>", crossingPoint.Item1, crossingPoint.Item2);
            //var peaks = cc.CalculatePeaks();
            //Console.WriteLine("NPeaks: {0}", peaks.Count);
            //foreach (var peak in peaks)
            //{
            //    Console.WriteLine("Peak: <{0},{1}>", peak.Item1, peak.Item2);
            //}


            //Console.WriteLine("End Test Step");
            //return;

            try
            {
                // Sets the output file path
                GlobalData.filehandler.SetFileHandler();
                GlobalData.filehandler.CreateFile();

                Console.WriteLine("ESTAMOS NO PROGRAM.CS");
                // holds the paths to the readers
                //string hostname1 = "speedwayr-10-9f-3f.local";
                string hostname2 = "speedwayr-10-9f-c8.local";
                //string hostname3 = "speedwayr-10-9f-bb.local";

                // Create two reader instances and add them to the List of readers.
                //Modificado
                //readers.Add(new ImpinjReader(hostname1, "Reader #1"));
                readers.Add(new ImpinjReader(hostname2, "Reader #2"));
                //Modificado
                //readers.Add(new ImpinjReader(hostname3, "Reader #3"));

                //Create map of rooms
                //Project.RegisterNewAmbient(0, new Ambient("Area_Externa(0)"));
                //Project.RegisterNewAmbient(1, new Ambient("Sala_Principal(1)"));
                Project.RegisterNewAmbient(2, new Ambient("Sala_Reuniao(2)"));
                //Project.RegisterNewAmbient(3, new Ambient("Corredor_Baias(3)"));

                //Create map of transitions
                //Modificado
                //Transition transition1 = new Transition(Project.GetAmbientInstance(0), "Reader #1", 1, Project.GetAmbientInstance(1), "Reader #1", 2);
                Transition transition2 = new Transition(Project.GetAmbientInstance(1), "Reader #2", 2, Project.GetAmbientInstance(2), "Reader #2", 1);
                //Modificado
                //Transition transition3 = new Transition(Project.GetAmbientInstance(1), "Reader #3", 1, Project.GetAmbientInstance(3), "Reader #3", 2);
                //Modificado
                //Project.RegisterNewTransition(Tuple.Create<string, ushort>("Reader #1", 1), transition1);
                //Project.RegisterNewTransition(Tuple.Create<string, ushort>("Reader #1", 2), transition1);
                Project.RegisterNewTransition(Tuple.Create<string, ushort>("Reader #2", 1), transition2);
                Project.RegisterNewTransition(Tuple.Create<string, ushort>("Reader #2", 2), transition2);
                //Project.RegisterNewTransition(Tuple.Create<string, ushort>("Reader #3", 1), transition3);
                //Project.RegisterNewTransition(Tuple.Create<string, ushort>("Reader #3", 2), transition3);

                //Create Map of Cardholders
                Project.PopulateProjectCardholders();

                // Loop through the List of readers to configure and start them.
                foreach (ImpinjReader reader in readers)
                {
                    // Connect to the reader
                    reader.Connect();

                    // Get the default settings
                    // We'll use these as a starting point
                    // and then modify the settings we're 
                    // interested in.
                    Settings settings = reader.QueryDefaultSettings();

                    settings.Report.IncludeAntennaPortNumber = true;
                    settings.Report.IncludeFirstSeenTime = true;
                    settings.Report.IncludeLastSeenTime = true;
                    settings.Report.IncludeSeenCount = true;
                    settings.Report.IncludeDopplerFrequency = true;
                    settings.Report.IncludePeakRssi = true;
                    // Send a tag report for every tag read
                    settings.Report.Mode = ReportMode.Individual;// BatchAfterStop;

                    // Reading tags for 1 seconds every 0.25 second
                    //settings.AutoStart.Mode = AutoStartMode.Periodic;
                    //settings.AutoStart.PeriodInMs = 500;
                    //settings.AutoStop.Mode = AutoStopMode.Duration;
                    //settings.AutoStop.DurationInMs = 500;

                    //Settings de antena
                    settings.Antennas.DisableAll();
                    settings.Antennas.GetAntenna(1).IsEnabled = true;
                    settings.Antennas.GetAntenna(2).IsEnabled = true;
                    // Set all the antennas to the max transmit power and receive sensitivity
                    settings.Antennas.TxPowerMax = true;
                    settings.Antennas.RxSensitivityMax = true;
                    // Or set all antennas to a specific value in dBm
                    //settings.Antennas.TxPowerInDbm = 28.0;
                    //settings.Antennas.RxSensitivityInDbm = -70.0;
                    // Or set each antenna individually
                    //settings.Antennas.GetAntenna(1).MaxTxPower = true;
                    //settings.Antennas.GetAntenna(1).MaxRxSensitivity = true;
                    //settings.Antennas.GetAntenna(2).TxPowerInDbm = 30.0;
                    //settings.Antennas.GetAntenna(2).RxSensitivityInDbm = -70.0;
                    // ...


                    settings.ReaderMode = ReaderMode.DenseReaderM8;



                    // Apply the newly modified settings.
                    reader.ApplySettings(settings);

                    // Assign the TagsReported event handler.
                    // This specifies which method to call
                    // when tags reports are available.
                    reader.TagsReported += Captura_tags;

                    // Start reading.
                    reader.Start();
                }

                // Wait for the user to press enter.
                //Console.WriteLine("Press enter to exit.");
                //Console.ReadKey();
                while(true)
                {
                    Console.WriteLine("Press 0, 1, 2, and 3 to set debuggin cardholder ambient and anything else to exit.");
                    var stringConsole = Console.ReadKey().KeyChar;
                    var aux = Convert.ToInt32(stringConsole) - 48;
                    if (Project.realAmbient <= 3 && Project.realAmbient >= 0)
                    {
                        Project.realAmbient = (ushort)(aux);
                    }
                    else
                    {
                        Project.realAmbient = 0;
                        break;
                    }
                }

                // Stop all the readers and disconnect from them.
                foreach (ImpinjReader reader in readers)
                {
                    try
                    {
                        reader.Stop();
                        reader.Disconnect();
                    }
                    catch (OctaneSdkException) { }
                }
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


            // Wait for the user to press enter.
            Console.WriteLine("Press enter to exit.");
            Console.ReadKey();
        }



        private static void Captura_tags(ImpinjReader sender, TagReport report)
        {
            foreach (Tag tag in report)
            {
                if (Project.IsTagRegistered(tag) && tag.PeakRssiInDbm > GlobalData.RSSILowPassFilter && Math.Abs(tag.RfDopplerFrequency) > 0.5 )
                {
                    Project.ReadingCardholderTag(tag, sender.Name);
                    Project.ProcessCardholderData(tag, sender.Name);
                    var individuo = Project.GetCardholder(tag.Epc.ToString());

                    // Writes data to file
                    GlobalData.filehandler.WriteToFile(individuo, tag.Epc.ToString(), sender.Name, tag.AntennaPortNumber);

                    // Debug
                    // Console.WriteLine("RSSI: {0}, Doppler: {1}, 0:{2}, 2:{3}, 3:{4}, 4:{5}, 5:{6}, 6:{7}", tag.PeakRssiInDbm, tag.RfDopplerFrequency, individuo.GetAmbient(0).GetName(), individuo.GetAmbient(2).GetName(), individuo.GetAmbient(3).GetName(), individuo.GetAmbient(4).GetName(), individuo.GetAmbient(5).GetName(), individuo.GetAmbient(6).GetName());
                    // Debug.end
                    Console.WriteLine("Current Ambient: {0},      Peaks:{1},     Dopppler:{2},    Combined:{3}", individuo.GetCurAmbient().GetName(), individuo.GetAmbient(0).GetName(), individuo.GetAmbient(5).GetName(), individuo.GetAmbient(6).GetName());

                }
            }
        }





    }
}