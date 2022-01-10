using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using vault.Databases;

namespace vault.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ReplayDbContext dbContext;
        public long TotalReplayCount;
        public DateTime FirstReplay, LastReplay;
        public IndexModel(ReplayDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            TotalReplayCount = await dbContext.Replays.CountAsync();
            FirstReplay = (await dbContext.Replays.FromSqlRaw("SELECT * FROM `replays` ORDER BY `timestamp` ASC LIMIT 1").FirstAsync()).Timestamp;
            LastReplay = (await dbContext.Replays.FromSqlRaw("SELECT * FROM `replays` ORDER BY `timestamp` DESC LIMIT 1").FirstAsync()).Timestamp;
        }
    }
}
