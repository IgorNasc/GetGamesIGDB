using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GetGamesIgdb
{
    class Game
    {
        private int id { get; set; }
        private string name { get; set; }
        private List<int> alternative_names { get; set; }
        private string slug { get; set; }
    }

    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            int qntSoliGames = 0, qntGames = 0;

            Console.WriteLine("Quantos jogos você deseja buscar? ");
            bool result = int.TryParse(Console.ReadLine(), out qntSoliGames);

            if (result)
            {
                RequestGame(qntGames, qntSoliGames);
            }

            Console.WriteLine("Finalizado a busca pelos jogos!");
        }

        static async Task RequestGame(int qntGames, int qntSoliGames)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Client-ID", "456868n0yqhvvf24k5avk9lpdn764h");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer 8ipakf2yr586v8fmamd27589xghkq4");

            while (qntGames < qntSoliGames)
            {
                var streamTask = await client.PostAsync("https://api.igdb.com/v4/games", new StringContent("fields name,alternative_names,slug; limit 50;", Encoding.UTF8, "text/plain"));
                var response = await JsonSerializer.DeserializeAsync<List<Game>>(await streamTask.Content.ReadAsStreamAsync());

                qntGames += 50;

                Console.WriteLine(response.ToString());
            };
        } 
    }
}
