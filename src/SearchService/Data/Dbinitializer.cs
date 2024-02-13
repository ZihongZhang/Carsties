using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public class Dbinitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb",MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(a => a.Make, KeyType.Text)
            .Key(a => a.Model,KeyType.Text)
            .Key(a => a.Color,KeyType.Text)
            .CreateAsync();
            var count = await DB.CountAsync<Item>();
            // if (count == 0)
            // {
            //    System.Console.WriteLine("No dara - will attempt to seed");
            //    var itemData = await System.IO.File.ReadAllTextAsync("Data/auctions.json");
            //    var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true}; 
            //    var items = JsonSerializer.Deserialize<List<Item>>(itemData,options);
            //    await DB.SaveAsync(items); 
            // }
            using var scope =app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            var items = await httpClient.GetItemForSearchDb();
            System.Console.WriteLine(items.Count+"returned from the auction service");
            if (items.Count > 0)
            {
                
                await DB.SaveAsync(items);
            }
    }

}
