////////////////////////////////////////////////////////////////////////////////
//
//    TG2 Leo e Kenneth
//    Variaveis relacionadas ao Reader, Antena 1, Antena 2 e flags do programa.
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
        public static int FiltroPassaBaixaRSSI = -58;

        // Variavel que armazena o valor de RSSI que indica que alguém passou na frente da antena.
        public static int IndicadorRSSIPassagem = -50;

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
        public static int ContadorPassagemAnt1 = 0;

        //Variaveis relacionadas aos dados da segunda antena.
        public static double PicoRSSIAnt2 = -63;
        public static string TempoPicoRSSIAnt2;
        public static DateTime DateTimePicoRSSIAnt2;
        public static double PicoNegFreqDopAnt2 = 0;
        public static string TempoPicoNegFreqDopAnt2;
        public static DateTime DateTimePicoNegFreqDopAnt2;
        public static int FlagPassagemAnt2 = 0;
        public static int LockPicoRSSIAnt2 = 0;
        public static int ContadorPassagemAnt2 = 0;

        //Dicionário com todas as EPCs das tags e as respectivas salas em que se encontram
        public static Dictionary<string, int> tag_sala = new Dictionary<string, int>();

        // Cria uma lista de objetos Tags_TG com os EPCs e nomes das pessoas.
        // Adicione as novas TAGs colocando seu EPC e o nome relacionado.
        public static List<Tags_TG> ListaTAGs = new List<Tags_TG>
        {
            new Tags_TG{EPC = "E200 001B 2609 0146 2580 7745", Nome = "Leonardo" ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0146 2700 7715", Nome = "Fernanda" ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0146 2770 76FD", Nome = "Kenneth"  ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0146 2630 7739", Nome = "Jhon"     ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0146 2780 76F5", Nome = "Marcelo"  ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0146 2710 7719", Nome = "Gabriela" ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0146 2650 772D", Nome = "Julia"    ,Ambiente = 0},
            new Tags_TG{EPC = "E200 001B 2609 0145 2880 76A4", Nome = "Guilherme",Ambiente = 0}
        };

        // Cria dicionarios dos ambientes para fazer a contagem de pessoas por ambiente e inicializa eles.
        // Nesse caso, todos estao sendo inicializados na sala principal.
        public static Dictionary<string, Tags_TG> DictAmbienteExterno = new Dictionary<string, Tags_TG>(){
            { "E200 001B 2609 0146 2580 7745", ListaTAGs[0] },
            { "E200 001B 2609 0146 2700 7715", ListaTAGs[1] },
            { "E200 001B 2609 0146 2770 76FD", ListaTAGs[2] },
            { "E200 001B 2609 0146 2630 7739", ListaTAGs[3] },
            { "E200 001B 2609 0146 2780 76F5", ListaTAGs[4] },
            { "E200 001B 2609 0146 2710 7719", ListaTAGs[5] },
            { "E200 001B 2609 0146 2650 772D", ListaTAGs[6] },
            { "E200 001B 2609 0145 2880 76A4", ListaTAGs[7] }
        };
        public static Dictionary<string, Tags_TG> DictSalaReunioes = new Dictionary<string, Tags_TG>();
        public static Dictionary<string, Tags_TG> DictCorredorBaias = new Dictionary<string, Tags_TG>();
        public static Dictionary<string, Tags_TG> DictSalaPrincipal = new Dictionary<string, Tags_TG>();
    }



    public static class ConstantesReader
    {
        public const string ReaderHostname1 = "SpeedwayR-10-9F-3F.local";
        public const string ReaderHostname2 = "SpeedwayR-10-9F-C8.local";
        public const string ReaderHostname3 = "SpeedwayR-10-9F-BB.local";
    }



    static class ConstantesAmbiente
    {
        public const int AmbienteExterno = 0;
        public const int SalaPrincipal = 1;
        public const int SalaDeReunioes = 2;
        public const int CorredorDeBaias = 3;
    }



    public class Tags_TG
    {
        public string EPC { get; set; }
        public string Nome { get; set; }
        public int Ambiente { get; set; }
    }



}
