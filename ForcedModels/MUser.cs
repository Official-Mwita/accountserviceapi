using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace accountservice.ForcedModels
{
    public class MUser
    {
        public static readonly string ADMIN_TYPE = "admin";

        private string? hashedPassword;

        //Used for password hashing
        const int keySize = 64;
        const int iterations = 350000;
        static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserID { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        public string? UserName { get; set; } = string.Empty;


        public string? Password
        {
            get { return hashedPassword; }
            set
            {

                //Hash password here
                if (hashedPassword != null && value?.Trim() != string.Empty)
                    hashedPassword = passwordHash(value ?? "");
                else
                    hashedPassword = value;
            }
        }

        [JsonIgnore]
        public string HashPassword { set { hashedPassword = value; } }

        public string FullName { get; set; } = string.Empty;
        public string PhysicalAddress { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Telephone { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public int Experience { get; set; }
        public string Position { get; set; } = string.Empty;
        public string DisabilityStatus { get; set; } = string.Empty;

        public string? IdNumber { get; set; }

        //Generate password hash

        public static string passwordHash(string plain)
        {
            byte[] salt = Encoding.UTF8.GetBytes("This is my salt/sugar");

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(plain),
                salt,
                iterations,
                hashAlgorithm,
                keySize);

            return Convert.ToHexString(hash); ;
        }
    }
}
