using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using vault.Services;

namespace vault.Pages.Maps
{
    public class ById : PageModel
    {
        private readonly BeatmapDataService beatmapDataService;
        public ById(BeatmapDataService beatmapDataService)
        {
            this.beatmapDataService = beatmapDataService;
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var id = RouteData.Values["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return Redirect("/Maps");
            }

            var beatmapHash = await beatmapDataService.GetBeatmapHash(id);
            if (beatmapHash != null)
                return Redirect("/Replays/Map/" + beatmapHash);
            return Redirect("/Maps");
        }
    }
}