using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace vault.Pages.Replays
{
    public class MapEntryModel : PageModel
    {
        [FromForm(Name = "beatmap")]
        public string? Beatmap { get; set; }

        public const string DefaultBeatmap = "c2a034a5c6d3a7fec931e065f4b12a66";

        public IActionResult OnPost()
        {
            return Redirect($"/replays/map/{Beatmap ?? DefaultBeatmap}");
        }
    }
}