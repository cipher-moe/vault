using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using vault.Services;
using vault.Services.ReplayDatabase;

namespace vault.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ReplayDatabaseService databaseService;
        public long TotalReplayCount;
        public DateTime FirstReplay, LastReplay;
        public IndexModel(ReplayDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        public async Task OnGetAsync()
        {
            TotalReplayCount = await databaseService.Collection.EstimatedDocumentCountAsync();
            var query = databaseService.Collection.Find(FilterDefinition<Replay>.Empty);
            FirstReplay = DateTime.Parse(query.Sort(Builders<Replay>.Sort.Ascending(r => r.Timestamp)).First().Timestamp).ToUniversalTime();
            LastReplay = DateTime.Parse(query.Sort(Builders<Replay>.Sort.Descending(r => r.Timestamp)).First().Timestamp).ToUniversalTime();
        }
    }
}
