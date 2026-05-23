using FluentValidation;
using ReportesLocalidadApi.Models.DTOs;

namespace ReportesLocalidadApi.Validators
{
    public class CambiarEstadoValidator : AbstractValidator<CambiarEstadoReporteDto>
    {
        public CambiarEstadoValidator()
        {
            RuleFor(x => x.IdEstado).Must(x => x > 0 && x <= 3).WithMessage("Ingrese un estado válido.");
        }
    }
}
