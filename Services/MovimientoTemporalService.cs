using IurixBlazor.Shared.Dtos;

public class MovimientoTemporalService
{
    private MovimientoTemporalDto? _movimiento;

    public void Guardar(MovimientoTemporalDto dto)
    {
        _movimiento = dto;
    }

    public MovimientoTemporalDto? Obtener()
    {
        return _movimiento;
    }

    public void Limpiar()
    {
        _movimiento = null;
    }
}
