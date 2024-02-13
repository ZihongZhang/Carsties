using MongoDB.Entities;

namespace SearchService;

public class AuctionSvcHttpClient
{
        private readonly HttpClient _httpClient;
        public IConfiguration Configuration { get; }
    public AuctionSvcHttpClient(HttpClient httpClient,IConfiguration configuration)
    {
            Configuration = configuration;
            _httpClient = httpClient;
        
    }
    public async Task<List<Item>> GetItemForSearchDb()
    {
        var lastUpdateed =await DB.Find<Item,string>()
        .Sort(x=>x.Descending(y=>y.UpdatedAt))
        .Project(x=>x.UpdatedAt.ToString())
        .ExecuteFirstAsync();
        return await _httpClient.GetFromJsonAsync<List<Item>>(Configuration["AuctionServiceUrl"]
        +"/api/auctions?date="+lastUpdateed);
         
    }
    
        
    
    
        
    

}
