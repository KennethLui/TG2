using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TG2_RFID
{
    public class FileHandler
    {

        protected string filePath;

        public void SetFileHandler()
        {
            filePath = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log_" + DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + ".csv";

        }

        public void WriteToFile(Cardholder person, string epc, string reader, ushort ant)
        {
            // Set File parameters
            string csvSeperator = ";";
            StringBuilder streamOutput = new StringBuilder();

            var antenna = Tuple.Create<string,ushort> (reader,ant);
            var strRSSIcurve = person.GetPowerCurve(antenna).GetCurveIndexY(0).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(1).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(2).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(3).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(4).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(5).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(6).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(7).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(8).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(9).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexY(10).ToString() + ";"
                               ;

            var strRSSItime =  person.GetPowerCurve(antenna).GetCurveIndexX(0).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(1).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(2).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(3).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(4).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(5).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(6).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(7).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(8).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(9).ToString() + ";" +
                               person.GetPowerCurve(antenna).GetCurveIndexX(10).ToString() + ";"
                               ;

            var strDopplercurve = person.GetDopplerEffectCurve(antenna).GetCurveIndexY(0).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(1).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(2).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(3).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(4).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(5).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(6).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(7).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(8).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(9).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexY(10).ToString()
                                  ;

            var strDopplertime =  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(0).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(1).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(2).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(3).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(4).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(5).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(6).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(7).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(8).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(9).ToString() + ";" +
                                  person.GetDopplerEffectCurve(antenna).GetCurveIndexX(10).ToString()
                                  ;

            string[][] dataOutput = new string[][]{
                                    new string[]{person.GetName(), epc, reader, ant.ToString(), DateTime.Now.ToString("dd/MM/yyyy; HH:mm:ss:ffff"), person.GetCurAmbient().GetName(),";", strRSSIcurve, strRSSItime, strDopplercurve,";", strDopplertime, ";", person.GetAmbient(0).GetName(), person.GetAmbient(2).GetName(), person.GetAmbient(3).GetName(), person.GetAmbient(4).GetName(), person.GetAmbient(5).GetName(), person.GetAmbient(6).GetName() }
                                    };
            int length = dataOutput.GetLength(0);
            for (int i = 0; i < length; i++)
                streamOutput.AppendLine(string.Join(csvSeperator, dataOutput[i]));

            // Appends more lines to the csv file
            File.AppendAllText(filePath, streamOutput.ToString());
        }

        public void CreateFile()
        {
            // Set File parameters
            string csvSeperator = ";";
            StringBuilder streamOutput = new StringBuilder();

            string[][] dataOutput = new string[][]{
                                    new string[]{"Nome", "EPC", "Leitora", "Antena", "Data", "Horario", "Ambiente Correto", ";", "Curva RSSI", ";;;;;;;;",";", "Tempo RSSI", ";;;;;;;;", ";", "Curva Doppler", ";;;;;;;;;", ";", "Tempo Doppler", ";;;;;;;;",  ";;", "Ambiente-LAST-RSSI-PEAK-TIME", "Ambiente-RSSI-Last-value", "Ambiente-RSSI-Mean", "Ambiente-RSSI-Median", "Ambiente-Doppler" , "DopplerAndRSSI"}
                                    };
            int length = dataOutput.GetLength(0);
            for (int i = 0; i < length; i++)
                streamOutput.AppendLine(string.Join(csvSeperator, dataOutput[i]));

            // Create and write the csv file
            File.WriteAllText(filePath, streamOutput.ToString());
        }


    }
}
