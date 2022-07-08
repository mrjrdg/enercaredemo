namespace EnercareDemo2.Service
{
    public class SignUpMagicLinkService
    {
        private readonly IdTokenHintService _idTokenHintBuilder;
        private readonly MailDeliveryService _mailDeliveryService;
        private readonly IConfiguration _configuration;

        public SignUpMagicLinkService(
            IdTokenHintService idTokenHintBuilder,
            MailDeliveryService mailDeliveryService,
            IConfiguration configuration)
        {
            _idTokenHintBuilder = idTokenHintBuilder;
            _mailDeliveryService = mailDeliveryService;
            _configuration = configuration;
        }

        public string GetRedirectUrlWithIdTokenHint(string email, string contactId)
        {
            var idTokenHint = _idTokenHintBuilder
                .BuildIdTokenForSignup(email, contactId);

            var redirectUrl = _configuration
                   .GetSection("SignUpMagicLinkInvitationConfiguration")["RedirectUri"];

            return $"{redirectUrl}?id_token_hint={idTokenHint}";
        }

        public string GetRedirectUrlWithIdTokenHintForSpa(string email, string contactId)
        {
            var idTokenHint = _idTokenHintBuilder
                .BuildIdTokenForSignup(email, contactId);

            var redirectUrl = "https://localhost:3000";

            return $"{redirectUrl}?id_token_hint={idTokenHint}";
        }

        public string GetIdTokenHint(string email)
        {
            return _idTokenHintBuilder
                .BuildIdToken(email);
        }
    }
}
