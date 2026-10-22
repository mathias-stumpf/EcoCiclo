namespace EcoCiclo.Models;

public class PesquisaResiduosViewModel
{
    public string Termo { get; set; } = string.Empty;
    public IReadOnlyList<Residuo> Resultados { get; set; } = Array.Empty<Residuo>();
    public IReadOnlyList<Residuo> Sugestoes { get; set; } = Array.Empty<Residuo>();
    public IReadOnlyList<Categoria> CategoriasDisponiveis { get; set; } = Array.Empty<Categoria>();
    public Categoria? CategoriaSelecionada { get; set; }
    public IReadOnlyList<Residuo> MateriaisCategoria { get; set; } = Array.Empty<Residuo>();
    public IReadOnlyList<Residuo> CatalogoCompleto { get; set; } = Array.Empty<Residuo>();
    public bool MostrarCatalogo { get; set; }
    public string? TituloMensagem { get; set; }
    public string? Mensagem { get; set; }
}
