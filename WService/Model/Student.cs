namespace WService.Model {
    public class Student {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public int Grade { get; set; }

        //For Authentication and JWT Side
        public string Email { get; set; } = null!;
        public string HashedPassword { get; set; } = null!;
        public string Role { get; set; } = null!;

        //For Refresh-Token Handling
        public string HashedRefreshToken { get; set; } = null!;
        public DateTime? RefreshTokenExpiredAt { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }

    }
}
