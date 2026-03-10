using CocktailHub.Api.DTOs.Cocktail;
using FluentValidation;

namespace CocktailHub.Api.Validators;

public class CreateCocktailRequestValidator : AbstractValidator<CreateCocktailRequest>
{
    public CreateCocktailRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);

        RuleFor(x => x.Instructions)
            .NotEmpty().WithMessage("Instructions are required");

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("Country is required");

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("At least one ingredient is required");

        RuleForEach(x => x.Ingredients)
            .ChildRules(i =>
            {
                i.RuleFor(x => x.IngredientId)
                    .GreaterThan(0).WithMessage("Invalid ingredient");
            });
    }
}
