using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}