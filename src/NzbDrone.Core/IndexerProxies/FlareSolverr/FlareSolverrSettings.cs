using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.IndexerProxies.FlareSolverr
{
    public class FlareSolverrSettingsValidator : AbstractValidator<FlareSolverrSettings>
    {
        public FlareSolverrSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.RequestTimeout).InclusiveBetween(1, 180);
        }
    }

    public class FlareSolverrSettings : IIndexerProxySettings
    {
        private static readonly FlareSolverrSettingsValidator Validator = new FlareSolverrSettingsValidator();

        public FlareSolverrSettings()
        {
            Host = "http://localhost:8191/";
            RequestTimeout = 60;
            SessionEnabled = false;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(2, Label = "Request Timeout", Advanced = true, HelpText = "FlareSolverr maxTimeout Request Parameter", Unit = "seconds")]
        public int RequestTimeout { get; set; }

        [FieldDefinition(4, Label = "Session", Advanced = true, HelpText = "FlareSolverr session Request Parameter, if unchecked no session will be used", Type=FieldType.Checkbox)]
        public bool SessionEnabled { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
