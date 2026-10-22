namespace EcoCiclo.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string IdentificacaoVisual { get; set; } = string.Empty;
    public ICollection<Residuo> Residuos { get; set; } = new List<Residuo>();
}
