namespace EcoCiclo.Models;

public class PontoColeta
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? HorarioFuncionamento { get; set; }
    public string? Telefone { get; set; }
    public string? FonteInformacao { get; set; }
    public DateTime? DataVerificacao { get; set; }
    public string? UrlMapa { get; set; }
    public bool Ativo { get; set; } = true;
    public ICollection<ResiduoPontoColeta> ResiduosAceitos { get; set; } = new List<ResiduoPontoColeta>();
}
