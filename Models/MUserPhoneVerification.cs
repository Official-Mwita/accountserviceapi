namespace accountservice.Models
{
    public class MUserPhoneVerification
    {
        public string phoneNumber { get; set; } = string.Empty;

        public int VerificationCode { get; set; }

        public long ExpiryEpoch { get; set; }

        public string id { get { return "" + ExpiryEpoch; } }

    }
}
