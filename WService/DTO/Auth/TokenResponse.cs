using System.ComponentModel.DataAnnotations;

namespace WService.DTO.Auth {
    public class TokenResponse{
        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
