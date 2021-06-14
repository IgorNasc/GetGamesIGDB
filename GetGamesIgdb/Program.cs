using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GetGamesIgdb
{
    class Game
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<int> alternative_names { get; set; }
        public List<string> alternatives { get; set; }
        public string slug { get; set; }
    }

    class AlternativesNames
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    class Program
    {
        private static readonly string FILE_PATH = "D:/Igor/Documents/fumec/ia/game.csv";
        private static readonly string API_VERSION = "/v4";
        private static readonly HttpClient httpClient = new HttpClient();

        static void Main(string[] args)
        {
            List<Game> games = new List<Game>();
            int qntSoliGames = 0;
            InitHttpClient();

            try
            {
                Console.WriteLine("Quantos jogos você deseja buscar? ");
                bool result = int.TryParse(Console.ReadLine(), out qntSoliGames);

                if (result)
                    games = RequestGame(qntSoliGames).Result;

                if (games.Count > 0)
                {
                    games = BuildAlternativeNames(games).Result;
                    CreateFile(games);
                }
            } catch (HttpRequestException e)
            {
                Console.WriteLine("\n>>>>> Houve erro na tentativa de fazer a requisição na API!");
                Console.WriteLine(e);
            } catch (JsonException e)
            {
                Console.WriteLine("\n>>>>> Houve erro na conversão dos dados da API!");
                Console.WriteLine(e);
            } catch (Exception e)
            {
                Console.WriteLine("\n>>>>> Houve erro inexperado com a execução do codigo!");
                Console.WriteLine(e);
            }
            
            Console.WriteLine("\n>>>>> Finalizado a busca pelos jogos!");
        }

        static void InitHttpClient()
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Client-ID", "456868n0yqhvvf24k5avk9lpdn764h");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer 8ipakf2yr586v8fmamd27589xghkq4");

            httpClient.BaseAddress = new Uri("https://api.igdb.com");
        }

        static async Task<List<Game>> BuildAlternativeNames(List<Game> listGames)
        {
            foreach (Game game in listGames)
            {
                if (game.alternative_names != null)
                {
                    string ids = "";
                    game.alternative_names.ForEach(x => ids += $",{x}");
                    ids = ids.Remove(0, 1);

                    List<AlternativesNames> listAlternativeGames = await ResquestAlternativeName(ids);

                    listAlternativeGames.ForEach(alternative =>
                    {
                        if (game.alternatives == null)
                            game.alternatives = new List<string>();

                        game.alternatives.Add(alternative.name);
                    });
                }
            }

            return listGames;
        }

        static void CreateFile(List<Game> listGames)
        {
            Console.WriteLine($">>>>> Iniciado a criação do arquivo CSV.");

            var csv = new StringBuilder();

            listGames.ForEach(game =>
            {
                string alternativeNames = "";

                if (game.alternatives == null)
                    game.alternatives = new List<string>();

                game.alternatives.ForEach(name => alternativeNames += $",{name}");
                csv.AppendLine($"{game.name.Replace(',', Char.MinValue)},{alternativeNames}");
            });

            if (File.Exists(FILE_PATH))
                File.Delete(FILE_PATH);

            File.WriteAllText(FILE_PATH, csv.ToString());

            Console.WriteLine($">>>>> Finalizado a criação do arquivo CSV.");
            Console.WriteLine($">>>>> O arquivo se encontra em: {FILE_PATH}");
        }

        static async Task<List<Game>> RequestGame(int qntSoliGames)
        {
            var request = await httpClient.PostAsync($"{API_VERSION}/games", new StringContent($"fields name,alternative_names,slug; limit {qntSoliGames};", Encoding.UTF8, "text/plain"));

            return await JsonSerializer.DeserializeAsync<List<Game>>(await request.Content.ReadAsStreamAsync());
        } 

        static async Task<List<AlternativesNames>> ResquestAlternativeName(string ids)
        {
            var request = await httpClient.PostAsync($"{API_VERSION}/alternative_names", new StringContent($"fields name; where id = ({ids});", Encoding.UTF8, "text/plain"));

            return await JsonSerializer.DeserializeAsync<List<AlternativesNames>>(await request.Content.ReadAsStreamAsync());
        }
    }
}
