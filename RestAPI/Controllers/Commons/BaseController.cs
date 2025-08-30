using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Controllers.Commons;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class BaseController : ControllerBase;