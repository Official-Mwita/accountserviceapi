namespace accountservice.ForcedModels
{
    //A model to hold user information as provided by microsoft graph
    public class MUserMicrosoft
    {
        public string? DisplayName { get; set; }

        public string? GivenName { get; set; }

        public string? MobilePhone { get; set; }

        public string UserPrincipalName { get; set; } = string.Empty;

        public string? Mail { get; set; }

        public string? preferredLanguage { get; set; }
        public string Id { get; set; } = string.Empty;

       
    }
}
