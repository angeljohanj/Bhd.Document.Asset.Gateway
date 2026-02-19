using FluentValidation;
using Gateway.Application.DTOs;
using System.Text.RegularExpressions;

namespace Gateway.Application.Validators;

public class DocumentUploadRequestValidator : AbstractValidator<DocumentUploadRequestDto>
{
    public DocumentUploadRequestValidator()
    {
        RuleFor(x => x.Filename)
            .NotEmpty().WithMessage("Filename is required.")
            .MaximumLength(255).WithMessage("Filename cannot exceed 255 characters.")
            .Matches(@"^[a-zA-Z0-9_\-\.]+$").WithMessage("Filename contains invalid characters.");

        RuleFor(x => x.EncodedFile)
            .NotEmpty().WithMessage("Encoded file content is required.")
            .Must(BeAValidBase64).WithMessage("Content must be a valid Base64 string.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(x => x.Contains("/")).WithMessage("Invalid content type format.");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Invalid document type.");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Invalid channel.");
    }

    private bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        
        var base64Data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        
        try
        {
            Convert.FromBase64String(base64Data);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
