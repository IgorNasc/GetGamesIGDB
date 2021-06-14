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
        static void Main(string[] args)
        {
            int qntSoliGames = 0, qntGames = 0;
            List<Game> games = new List<Game>();

            Console.WriteLine("Quantos jogos você deseja buscar? ");
            bool result = int.TryParse(Console.ReadLine(), out qntSoliGames);

            if (result)
                games = RequestGame(qntSoliGames).Result;

            //before your loop
            var csv = new StringBuilder();

            games.ForEach(game => 
            {
                string alternativeNames = "";

                if (game.alternatives == null)
                    game.alternatives = new List<string>();

                game.alternatives.ForEach(name => alternativeNames += $",{name}");
                csv.AppendLine($"{game.name},{alternativeNames}");
            });

            //after your loop
            File.WriteAllText("D:/Igor/Documents/fumec/ia/game.csv", csv.ToString());

            Console.WriteLine("Finalizado a busca pelos jogos!");
        }

        static async Task<List<Game>> RequestGame(int qntSoliGames)
        {
            HttpClient client = new HttpClient();
            List<Game> listGames = new List<Game>();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Client-ID", "456868n0yqhvvf24k5avk9lpdn764h");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer 8ipakf2yr586v8fmamd27589xghkq4");

            var streamTask = await client.PostAsync("https://api.igdb.com/v4/games", new StringContent($"fields name,alternative_names,slug; limit {qntSoliGames};", Encoding.UTF8, "text/plain"));
            List<Game> response = await JsonSerializer.DeserializeAsync<List<Game>>(await streamTask.Content.ReadAsStreamAsync());

            listGames.AddRange(await ResquestAlternativeName(response));

            return listGames;
        } 

        static async Task<List<Game>> ResquestAlternativeName(List<Game> listGames)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Client-ID", "456868n0yqhvvf24k5avk9lpdn764h");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer 8ipakf2yr586v8fmamd27589xghkq4");

            foreach (Game game in listGames)
            {
                if (game.alternative_names != null)
                {
                    List<int> alternativesIds = new List<int>();
                    alternativesIds.AddRange(game.alternative_names);

                    string text = "";
                    alternativesIds.ForEach(x => text += $",{x}");
                    text = text.Remove(0,1);

                    var streamTask = await client.PostAsync("https://api.igdb.com/v4/alternative_names", new StringContent($"fields name; where id = ({text});", Encoding.UTF8, "text/plain"));
                    List<AlternativesNames> response = await JsonSerializer.DeserializeAsync<List<AlternativesNames>>(await streamTask.Content.ReadAsStreamAsync());

                    foreach (AlternativesNames alternative in response)
                    {
                        if (game.alternatives == null)
                            game.alternatives = new List<string>();

                        game.alternatives.Add(alternative.name);
                    }
                }
            }

            return listGames;
        }
    }
}
