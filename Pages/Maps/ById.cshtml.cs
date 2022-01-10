using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using vault.Databases;

namespace vault.Pages.Maps
{
    public class ById : PageModel
    {
        private readonly BeatmapDbContext beatmapDbContext;
        public ById(BeatmapDbContext beatmapDbContext)
        {
            this.beatmapDbContext = beatmapDbContext;
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var id = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Redirect("/Maps");
            }

            var beatmapHash = await beatmapDbContext.GetBeatmapHash(id);
            if (beatmapHash != null)
                return Redirect("/Replays/Map/" + beatmapHash);
            return Redirect("/Maps");
        }
    }
}