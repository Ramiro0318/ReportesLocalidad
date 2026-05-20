using FluentValidation;
using ReportesLocalidadApi.Models.DTOs;

namespace ReportesLocalidadApi.Validators
{
    public class RegistroValidator : AbstractValidator<RegistroDto>
    {
        public RegistroValidator()
        {
            RuleFor(x => x.NombreUsuario).NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("El título no puede contener solo espacios vacíos.")
                .MinimumLength(1).WithMessage("Ingrese un nombre de usuario válido.")
                .MaximumLength(40).WithMessage("Un nombre de usuario no puede tener tantas letras.")
                .Matches(@"^[a-zA-Z0-9ñÑ]+$").WithMessage("Un nombre no puede contener simbolos ni caracteres especiales.");

            RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña no puede estar vacía.").
                MinimumLength(6).WithMessage("La contraseña tiene que tener entre 6 y 30 caracteres.").
                MaximumLength(30).WithMessage("La contraseña tiene que tener entre 6 y 30 caracteres.");

            RuleFor(x => x.IdRol).Equal(1).WithMessage("Rol no válido");
        }
    }
}
