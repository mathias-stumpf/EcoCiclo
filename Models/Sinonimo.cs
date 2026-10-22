namespace EcoCiclo.Models;

public class Sinonimo
{
    public int Id { get; set; }
    public int ResiduoId { get; set; }
    public Residuo Residuo { get; set; } = null!;
    public string Nome { get; set; } = string.Empty;
    public string NomeNormalizado { get; set; } = string.Empty;
}
