namespace EcoCiclo.Models;

public class Sugestao
{
    public int Id { get; set; }
    public string NomeResiduo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? EmailContato { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
}
