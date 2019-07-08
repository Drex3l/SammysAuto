using System.ComponentModel.DataAnnotations;

namespace SammysAuto.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}