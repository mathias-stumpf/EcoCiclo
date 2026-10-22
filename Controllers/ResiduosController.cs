using EcoCiclo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoCiclo.Controllers;

[Route("residuos")]
public class ResiduosController(EcoCicloDbContext dbContext) : Controller
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 170)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        var residuo = await dbContext.Residuos
            .AsNoTracking()
            .Include(item => item.Categoria)
            .SingleOrDefaultAsync(item => item.Slug == slug && item.Ativo, cancellationToken);

        if (residuo == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        return View(residuo);
    }
}
