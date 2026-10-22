using System.Diagnostics;
using EcoCiclo.Models;
using EcoCiclo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace EcoCiclo.Controllers;

public class HomeController(
    ServicoPesquisaResiduos servicoPesquisa,
    ILogger<HomeController> logger) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? q, int? categoriaId, bool mostrarCatalogo, CancellationToken cancellationToken)
    {
        var termo = q?.Trim() ?? string.Empty;
        if (termo.Length == 0)
        {
            return View("Search", new PesquisaResiduosViewModel
            {
                TituloMensagem = "Pesquise um resíduo",
                Mensagem = "Digite o nome de um resíduo para pesquisar."
            });
        }

        if (termo.Length > 100)
        {
            return View("Search", new PesquisaResiduosViewModel
            {
                Termo = termo[..100],
                TituloMensagem = "Pesquisa muito longa",
                Mensagem = "A pesquisa deve ter no máximo 100 caracteres."
            });
        }

        try
        {
            var resultados = await servicoPesquisa.BuscarAsync(termo, cancellationToken);
            var categorias = resultados.Count == 0
                ? await servicoPesquisa.ListarCategoriasAsync(cancellationToken)
                : Array.Empty<Categoria>();
            var categoriaSelecionada = categoriaId.HasValue
                ? categorias.SingleOrDefault(categoria => categoria.Id == categoriaId.Value)
                : null;
            var materiaisCategoria = categoriaSelecionada is null
                ? Array.Empty<Residuo>()
                : await servicoPesquisa.ListarPorCategoriaAsync(categoriaSelecionada.Id, cancellationToken);
            var catalogoCompleto = resultados.Count == 0 && mostrarCatalogo
                ? await servicoPesquisa.ListarCatalogoAsync(cancellationToken)
                : Array.Empty<Residuo>();

            return View("Search", new PesquisaResiduosViewModel
            {
                Termo = termo,
                Resultados = resultados,
                CategoriasDisponiveis = categorias,
                CategoriaSelecionada = categoriaSelecionada,
                MateriaisCategoria = materiaisCategoria,
                CatalogoCompleto = catalogoCompleto,
                MostrarCatalogo = mostrarCatalogo,
                TituloMensagem = resultados.Count == 0 ? "Não encontramos esse material" : null,
                Mensagem = resultados.Count == 0
                    ? categoriaSelecionada is not null
                        ? $"Confira os materiais cadastrados na categoria {categoriaSelecionada.Nome}."
                        : mostrarCatalogo
                            ? "Confira abaixo os materiais disponíveis no catálogo EcoCiclo."
                            : "Esse item pertence a alguma das categorias cadastradas? Escolha uma para ver os materiais disponíveis."
                    : null
            });
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Falha ao pesquisar resíduos no banco SQLite.");
            return View("Search", new PesquisaResiduosViewModel
            {
                Termo = termo,
                TituloMensagem = "Busca temporariamente indisponível",
                Mensagem = "Não foi possível consultar os resíduos agora. Tente novamente em instantes."
            });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
