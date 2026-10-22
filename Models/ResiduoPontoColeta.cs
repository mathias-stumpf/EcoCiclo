namespace EcoCiclo.Models;

public class ResiduoPontoColeta
{
    public int ResiduoId { get; set; }
    public Residuo Residuo { get; set; } = null!;
    public int PontoColetaId { get; set; }
    public PontoColeta PontoColeta { get; set; } = null!;
    public string? FonteConfirmacao { get; set; }
    public DateTime? DataConfirmacao { get; set; }
}
