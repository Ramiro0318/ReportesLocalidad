using FluentValidation;
using ReportesLocalidadApi.Models.DTOs;

namespace ReportesLocalidadApi.Validators
{
    public class EditarReporteValidator : AbstractValidator<SubirReporteDto>
    {
        public EditarReporteValidator()
        {
            RuleFor(x => x.Titulo).NotEmpty().WithMessage("Ingrese un título para el reporte.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("El título no puede contener solo espacios vacíos.")
                .MinimumLength(3).WithMessage("Ingrese un título con al menos 3 caracteres.")
                .MaximumLength(120).WithMessage("Ingrese un título con un máximo de 120 caracteres.")
                .Matches(@"^(?=.*[a-zA-ZñÑ]).+$").WithMessage("El título debe contener al menos una letra.");

            RuleFor(x => x.Descripcion).NotEmpty().WithMessage("Ingrese una descripción para el reporte.")
               .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("La descripción no puede contener solo espacios vacíos.")
               .MaximumLength(800).WithMessage("Ingrese una descripción con un máximo de 800 caracteres.");


            RuleFor(x => x.Direccion).MaximumLength(500).WithMessage("Ingrese una dirección con un máximo de 500 caracteres.");

            RuleFor(x => x.IdCategoria).GreaterThan(0).WithMessage("Categoría no válida.")
                .LessThan(7).WithMessage("Categoría no válida.");
        }
    }
}
