////////////////////////////////////////////////////////////////////////////////
//
//    TG2 Leo e Kenneth
//    Classe e funcoes responsaveis pelas transicoes das TAGs entrem ambientes
//
////////////////////////////////////////////////////////////////////////////////



using System;
using System.Collections;
using System.Collections.Generic;
using Impinj.OctaneSdk;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeGe;

namespace TeGe2
{
	public class TransicaoTags
	{

		// Faz a transicao dos ambientes de uma tag, dependendo da variavel "passagem" e da leitora que leu
		public static void transicao_sala(string leitora, string tag_epc, bool passagem, string nome, Tags_TG tag)
		{
			switch (leitora)
			{
				case "SpeedwayR-10-9F-C8.local":
					if (passagem == true)
					{
						GlobalData.tag_sala[tag_epc] = ConstantesAmbiente.SalaPrincipal;
						TransicaoTags.transicaoRemove(tag_epc, tag.Ambiente);
						TransicaoTags.transicaoADD(tag_epc, 1, tag);
						Console.WriteLine("{0} realizou uma transicao para a Sala Principal", nome);
						TransicaoTags.DisplayAmbientes();
					}
					else
					{
						GlobalData.tag_sala[tag_epc] = ConstantesAmbiente.AmbienteExterno;
						TransicaoTags.transicaoRemove(tag_epc, tag.Ambiente);
						TransicaoTags.transicaoADD(tag_epc, 0, tag);
						Console.WriteLine("{0} realizou uma transicao para o Ambiente Externo", nome);
						TransicaoTags.DisplayAmbientes();
					}
					break;

				case "SpeedwayR-10-9F-3F.local":
					if (passagem == true)
					{
						GlobalData.tag_sala[tag_epc] = ConstantesAmbiente.SalaDeReunioes;
						TransicaoTags.transicaoRemove(tag_epc, tag.Ambiente);
						TransicaoTags.transicaoADD(tag_epc, 2, tag);
						Console.WriteLine("{0} realizou uma transicao para a Sala De Reuniões", nome);
						TransicaoTags.DisplayAmbientes();
					}
					else
					{
						GlobalData.tag_sala[tag_epc] = ConstantesAmbiente.SalaPrincipal;
						TransicaoTags.transicaoRemove(tag_epc, tag.Ambiente);
						TransicaoTags.transicaoADD(tag_epc, 1, tag);
						Console.WriteLine("{0} realizou uma transicao para a Sala Principal", nome);
						TransicaoTags.DisplayAmbientes();
					}
					break;

				case "SpeedwayR-10-9F-BB.local":
					if (passagem == true)
					{
						GlobalData.tag_sala[tag_epc] = ConstantesAmbiente.CorredorDeBaias;
						TransicaoTags.transicaoRemove(tag_epc, tag.Ambiente);
						TransicaoTags.transicaoADD(tag_epc, 3, tag);
						Console.WriteLine("{0} realizou uma transicao para o Corredor De Baias", nome);
						TransicaoTags.DisplayAmbientes();
					}
					else
					{
						GlobalData.tag_sala[tag_epc] = ConstantesAmbiente.SalaPrincipal;
						TransicaoTags.transicaoRemove(tag_epc, tag.Ambiente);
						TransicaoTags.transicaoADD(tag_epc, 1, tag);
						Console.WriteLine("{0} realizou uma transicao para a Sala Principal", nome);
						TransicaoTags.DisplayAmbientes();
					}
					break;

				default:
					Console.Write("Caso não identificado");
					break;
			}
		}



		// Funcao responsavel por fazer a remocao da TAG no dicionario da sala anterior a transicao
		public static void transicaoRemove(string EPC, int sala_anterior)
		{
			switch (sala_anterior)
			{
				case 0:

					GlobalData.DictAmbienteExterno.Remove(EPC);
					break;

				case 1:

					GlobalData.DictSalaPrincipal.Remove(EPC);
					if (GlobalData.DictSalaPrincipal.Count <= 2 && GlobalData.AcionamtSalaPrincipal == 1)
					{
						Console.WriteLine("\n*Desacionamento do ventilador na Sala Principal*\n");
						GlobalData.AcionamtSalaPrincipal = 0;
						SerialCom.Controle_Ar("Sala Principal");
					}
					break;

				case 2:

					GlobalData.DictSalaReunioes.Remove(EPC);
					if (GlobalData.DictSalaReunioes.Count <= 2 && GlobalData.AcionamtSalaReunioes == 1)
					{
						Console.WriteLine("\n*Desacionamento do ventilador na Sala de Reunioes*\n");
						GlobalData.AcionamtSalaReunioes = 0;
						SerialCom.Controle_Ar("Sala Reuniões");
					}
					break;

				case 3:

					GlobalData.DictCorredorBaias.Remove(EPC);
					if (GlobalData.DictCorredorBaias.Count <= 2 && GlobalData.AcionamtCorredorBaias == 1)
					{
						Console.WriteLine("\n*Desacionamento do ventilador no Corredor de Baias*\n");
						GlobalData.AcionamtCorredorBaias = 0;
						SerialCom.Controle_Ar("Corredor de Baias");
					}
					break;

				default:
					Console.Write("Caso não identificado");
					break;
			}
		}



		// Funcao responsavel por adicionar a TAG no dicionario da sala para qual a TAG fez a transicao
		public static void transicaoADD(string EPC, int sala_atual, Tags_TG tag)
		{
			switch (sala_atual)
			{
				case 0:

					GlobalData.DictAmbienteExterno.Add(EPC, tag);
					tag.Ambiente = 0;
					break;

				case 1:

					GlobalData.DictSalaPrincipal.Add(EPC, tag);
					tag.Ambiente = 1;
					if (GlobalData.DictSalaPrincipal.Count > 2 && GlobalData.AcionamtSalaPrincipal == 0)
					{
						Console.WriteLine("\n*Acionamento do ventilador na Sala Principal*\n");
						GlobalData.AcionamtSalaPrincipal = 1;
						SerialCom.Controle_Ar("Sala Principal");
					}
					break;

				case 2:

					GlobalData.DictSalaReunioes.Add(EPC, tag);
					tag.Ambiente = 2;
					if (GlobalData.DictSalaReunioes.Count > 2 && GlobalData.AcionamtSalaReunioes == 0)
					{
						Console.WriteLine("\n*Acionamento do ventilador na Sala de Reunioes*\n");
						GlobalData.AcionamtSalaReunioes = 1;
						SerialCom.Controle_Ar("Sala Reuniões");
					}
					break;

				case 3:

					GlobalData.DictCorredorBaias.Add(EPC, tag);
					tag.Ambiente = 3;
					if (GlobalData.DictCorredorBaias.Count > 2 && GlobalData.AcionamtCorredorBaias == 0)
					{
						Console.WriteLine("\n*Acionamento do ventilador no Corredor de Baias*\n");
						GlobalData.AcionamtCorredorBaias = 1;
						SerialCom.Controle_Ar("Corredor de Baias");
					}
					break;

				default:
					Console.Write("Caso não identificado");
					break;
			}
		}


		// Funcao que mostra a quantidade de pessoas em cada ambiente, assim como seus EPCs e Nomes
		public static void DisplayAmbientes()
		{
			Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n");

			Console.WriteLine("- Numero de pessoas na Sala principal: {0}", GlobalData.DictSalaPrincipal.Count);
			for (int i = 0; i < GlobalData.DictSalaPrincipal.Count; i++)
			{
				Console.WriteLine("\tEPC: {0}, Nome: {1}", GlobalData.DictSalaPrincipal.Keys.ElementAt(i), GlobalData.DictSalaPrincipal[GlobalData.DictSalaPrincipal.Keys.ElementAt(i)].Nome);
			}

			Console.WriteLine("- Numero de pessoas no Corredor de Baias: {0}", GlobalData.DictCorredorBaias.Count);
			for (int i = 0; i < GlobalData.DictCorredorBaias.Count; i++)
			{
				Console.WriteLine("\tEPC: {0}, Nome: {1}", GlobalData.DictCorredorBaias.Keys.ElementAt(i), GlobalData.DictCorredorBaias[GlobalData.DictCorredorBaias.Keys.ElementAt(i)].Nome);
			}

			Console.WriteLine("- Numero de pessoas na Sala de reunioes: {0}", GlobalData.DictSalaReunioes.Count);
			for (int i = 0; i < GlobalData.DictSalaReunioes.Count; i++)
			{
				Console.WriteLine("\tEPC: {0}, Nome: {1}", GlobalData.DictSalaReunioes.Keys.ElementAt(i), GlobalData.DictSalaReunioes[GlobalData.DictSalaReunioes.Keys.ElementAt(i)].Nome);
			}

			Console.WriteLine("- Numero de pessoas no Ambiente Externo: {0}", GlobalData.DictAmbienteExterno.Count);
			for (int i = 0; i < GlobalData.DictAmbienteExterno.Count; i++)
			{
				Console.WriteLine("\tEPC: {0}, Nome: {1}", GlobalData.DictAmbienteExterno.Keys.ElementAt(i), GlobalData.DictAmbienteExterno[GlobalData.DictAmbienteExterno.Keys.ElementAt(i)].Nome);
			}
			Console.WriteLine("\n++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
		}
	}
}

