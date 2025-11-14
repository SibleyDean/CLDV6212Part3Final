using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public string? ReturnUrl { get; set; }
    }
}