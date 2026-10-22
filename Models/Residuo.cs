namespace EcoCiclo.Models;

public class Residuo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NomeNormalizado { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public string? InstrucoesPreparacao { get; set; }
    public string? InstrucoesDescarte { get; set; }
    public string? OQueNaoFazer { get; set; }
    public string? Cuidados { get; set; }
    public string? DestinoRecomendado { get; set; }
    public bool? PodeSerReciclado { get; set; }
    public string? ImpactoAmbiental { get; set; }
    public string? FonteInformacao { get; set; }
    public DateTime? DataRevisao { get; set; }
    public bool Ativo { get; set; } = true;
    public ICollection<Sinonimo> Sinonimos { get; set; } = new List<Sinonimo>();
    public ICollection<ResiduoPontoColeta> PontosColeta { get; set; } = new List<ResiduoPontoColeta>();
}
