using System.ComponentModel.DataAnnotations;

namespace WService.DTO.Auth {

    public class RefreshTokenRequest {

        [Required]
        public string RefreshToken { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;
    }
}
