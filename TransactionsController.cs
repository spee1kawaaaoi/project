
using localhost.Services;
using Microsoft.AspNetCore.Mvc;

namespace localhost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly TransactionService _service;

    public CategoriesController(TransactionService service)
    {
        _service = service;
    }

   
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetCategoriesAsync());
}