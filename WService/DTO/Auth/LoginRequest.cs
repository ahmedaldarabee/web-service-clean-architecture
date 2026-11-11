using System.ComponentModel.DataAnnotations;

namespace WService.DTO.Auth {
    public class LoginRequest {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
