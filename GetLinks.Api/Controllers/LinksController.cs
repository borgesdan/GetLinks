using GetLinks.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GetLinks.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        readonly LinksService linksService;

        public LinksController(LinksService linksService)
        {
            this.linksService = linksService;
        }

        [HttpPost("Search/Url")]
        public async Task<IActionResult> Search(SearchPostRequest request)
        {
            return await linksService.SearchByUrl(request);
        }

        [HttpPost("Search/Content")]
        public async Task<IActionResult> Search(SearchPostContentRequest request)
        {
            return await linksService.SearchByContent(request);
        }
    }    
}
