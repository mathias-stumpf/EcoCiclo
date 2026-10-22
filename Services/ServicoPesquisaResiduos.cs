using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EcoCiclo.Data;
using EcoCiclo.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoCiclo.Services;

public class ServicoPesquisaResiduos(EcoCicloDbContext dbContext)
{
    private static readonly HashSet<string> PalavrasIgnoradas = new(StringComparer.Ordinal)
    {
        "a", "as", "ao", "aos", "de", "da", "das", "do", "dos", "e", "em", "com", "para", "por",
        "usado", "usada", "usados", "usadas", "comum", "comuns"
    };

    public async Task<IReadOnlyList<Residuo>> BuscarAsync(string termo, CancellationToken cancellationToken = default)
    {
        var termoNormalizado = Normalizar(termo);
        if (termoNormalizado.Length == 0)
        {
            return Array.Empty<Residuo>();
        }

        var padrao = "%" + EscaparLike(termoNormalizado) + "%";
        return await dbContext.Residuos
            .AsNoTracking()
            .Include(residuo => residuo.Categoria)
            .Where(residuo => residuo.Ativo &&
                (EF.Functions.Like(residuo.NomeNormalizado, padrao, "\\") ||
                 residuo.Sinonimos.Any(sinonimo => EF.Functions.Like(sinonimo.NomeNormalizado, padrao, "\\"))))
            .OrderBy(residuo => residuo.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Residuo>> BuscarSimilaresAsync(string termo, CancellationToken cancellationToken = default)
    {
        var termosPesquisados = SepararTermos(Normalizar(termo))
            .Where(token => !PalavrasIgnoradas.Contains(token))
            .ToArray();

        if (termosPesquisados.Length == 0)
        {
            return Array.Empty<Residuo>();
        }

        var catalogo = await dbContext.Residuos
            .AsNoTracking()
            .Include(residuo => residuo.Categoria)
            .Include(residuo => residuo.Sinonimos)
            .Where(residuo => residuo.Ativo)
            .ToListAsync(cancellationToken);

        var limite = termosPesquisados.Length == 1 ? 0.62 : 0.42;
        return catalogo
            .Select(residuo => new
            {
                Residuo = residuo,
                Pontuacao = CalcularSimilaridade(termosPesquisados, residuo)
            })
            .Where(item => item.Pontuacao >= limite)
            .OrderByDescending(item => item.Pontuacao)
            .ThenBy(item => item.Residuo.Nome)
            .Take(3)
            .Select(item => item.Residuo)
            .ToList();
    }

    public async Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categorias
            .AsNoTracking()
            .OrderBy(categoria => categoria.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Residuo>> ListarPorCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Residuos
            .AsNoTracking()
            .Include(residuo => residuo.Categoria)
            .Where(residuo => residuo.Ativo && residuo.CategoriaId == categoriaId)
            .OrderBy(residuo => residuo.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Residuo>> ListarCatalogoAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Residuos
            .AsNoTracking()
            .Include(residuo => residuo.Categoria)
            .Where(residuo => residuo.Ativo)
            .OrderBy(residuo => residuo.CategoriaId)
            .ThenBy(residuo => residuo.Nome)
            .ToListAsync(cancellationToken);
    }

    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var espacosNormalizados = Regex.Replace(texto.Trim(), @"\s+", " ");
        var decomposed = espacosNormalizados.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(decomposed.Length);

        foreach (var caractere in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(char.ToLowerInvariant(caractere));
            }
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string EscaparLike(string texto)
    {
        return texto.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    private static IEnumerable<string> SepararTermos(string texto)
    {
        return Regex.Split(texto, @"[^\p{L}\p{N}]+")
            .Where(token => token.Length > 0);
    }

    private static double CalcularSimilaridade(IReadOnlyCollection<string> termos, Residuo residuo)
    {
        var termosCatalogados = SepararTermos(residuo.NomeNormalizado)
            .Concat(residuo.Sinonimos.SelectMany(sinonimo => SepararTermos(sinonimo.NomeNormalizado)))
            .Where(token => !PalavrasIgnoradas.Contains(token))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (termosCatalogados.Length == 0)
        {
            return 0;
        }

        var soma = 0d;
        foreach (var termo in termos)
        {
            var melhorCorrespondencia = termosCatalogados
                .Select(candidato => SimilaridadeEntrePalavras(termo, candidato))
                .DefaultIfEmpty(0)
                .Max();
            soma += melhorCorrespondencia;
        }

        return soma / termos.Count;
    }

    private static double SimilaridadeEntrePalavras(string termo, string candidato)
    {
        if (termo == candidato)
        {
            return 1;
        }

        if (termo.Length < 3 || candidato.Length < 3)
        {
            return 0;
        }

        var distancia = DistanciaLevenshtein(termo, candidato);
        return 1d - (double)distancia / Math.Max(termo.Length, candidato.Length);
    }

    private static int DistanciaLevenshtein(string primeiro, string segundo)
    {
        var linhaAnterior = Enumerable.Range(0, segundo.Length + 1).ToArray();
        var linhaAtual = new int[segundo.Length + 1];

        for (var i = 1; i <= primeiro.Length; i++)
        {
            linhaAtual[0] = i;
            for (var j = 1; j <= segundo.Length; j++)
            {
                var custo = primeiro[i - 1] == segundo[j - 1] ? 0 : 1;
                linhaAtual[j] = Math.Min(
                    Math.Min(linhaAtual[j - 1] + 1, linhaAnterior[j] + 1),
                    linhaAnterior[j - 1] + custo);
            }

            (linhaAnterior, linhaAtual) = (linhaAtual, linhaAnterior);
        }

        return linhaAnterior[segundo.Length];
    }
}
