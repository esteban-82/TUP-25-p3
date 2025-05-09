namespace TP4.Models;

public class ResultadoExamen
{
    public int Id { get; set; }
    public string NombreAlumno { get; set; }
    public int CantidadCorrectas { get; set; }
    public int TotalPreguntas { get; set; }
    public double NotaFinal { get; set; }
    public ICollection<RespuestaExamen> Respuestas { get; set; } 
}