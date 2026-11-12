using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WService.DataSimulation;
using WService.DTO.Auth;

namespace WService.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration){
            this._logger = logger;
            this._configuration = configuration;
        }

        //Most Important Endpoint that loved to the hackers where that enable hacker to be as you !
        //so we must increasing security level on it!
        [HttpPost("Login")]
        [EnableRateLimiting("AuthLimiter")]
        public IActionResult Login([FromBody] LoginRequest request){
            try {

                var IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "UnKnown";

                if (!ModelState.IsValid) {
                    return BadRequest("Please re-send credentials correctly");
                }

                var student = StudentDataSimulation.studentList.FirstOrDefault(std => std.Email == request.Email);

                if (student == null){

                    this._logger.LogWarning("Field to logging this email {0} with IP Address: {1}",request.Email, IPAddress);
                    return Unauthorized("Invalid Credentials");
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, student.HashedPassword);
                if (!isValidPassword) {
                    this._logger.LogWarning("Field to logging this email {0} with IP Address: {1}", request.Email, IPAddress);
                    return Unauthorized("Invalid Credentials");
                }

                //now after we check about EXISTENCY of student, we want to create JWT for it!
                string accessToken = GenerationAccessToken(student);
                string refreshToken = GenerationRefreshToken();
                RotationRefreshToken(student!, refreshToken);

                this._logger.LogInformation("Successfully to logging this email {0} with IP Address: {1}", request.Email, IPAddress);

                //here final calculation to the jwt where also the Signature that happening here once we doing .WriteToken(JWTToken)!
                //Note: this token must re-sending it in every api request !
                return Ok(new {
                    accessToken,
                    refreshToken
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error occurred while logging");
                return StatusCode(500, "Internal server error");
            }            
        }

        [HttpPost("RefreshToken")]
        [EnableRateLimiting("AuthLimiter")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request){
            try {

                var student = StudentDataSimulation.studentList.FirstOrDefault(std => std.Email == request.Email);
                var IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "UnKnown";

                if (student == null){
                    this._logger.LogWarning("student doesn't exist by email {0} with IP Address: {1}", request.Email, IPAddress);
                    return Unauthorized("something went wrong");
                }

                //if be as null, that mean refresh token is valid
                if (student?.RefreshTokenRevokedAt != null){
                    this._logger.LogWarning("Refresh Token Revoked");
                    return Unauthorized("something went wrong");
                }

                //if be as have date, that mean refresh token isn't expired so that be valid
                if (student?.RefreshTokenExpiredAt == null || student?.RefreshTokenExpiredAt <= DateTime.UtcNow){
                    this._logger.LogWarning("Refresh Token Expired");
                    return Unauthorized("something went wrong");
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken,student?.HashedRefreshToken);

                //Breach alert
                if (!isValid){
                    // so we will broken the main hashed refresh token to create new one that be not changed!
                    // [ Security Enhancement - this section that mean main refresh token that known by someone ]
                    
                    //that mean some one that change refresh-token, so we will handle this scenario
                    student!.RefreshTokenRevokedAt = DateTime.UtcNow;
                    student.HashedRefreshToken = null!;

                    this._logger.LogWarning("refresh token abuse by email {0} with IP Address: {1}", request.Email, IPAddress);

                    return Unauthorized("Potential security breach detected. Token revoked.");
                }

                string newAccessToken = GenerationAccessToken(student!);
                string newRefreshToken = GenerationRefreshToken();

                RotationRefreshToken(student!,newRefreshToken);

                this._logger.LogInformation("refresh token created successfully to email {0} with IP Address: {1}", request.Email, IPAddress);

                return Ok(new {
                    newAccessToken,
                    newRefreshToken
                });
            }
            catch (Exception ex){
                _logger.LogError(ex, "Error occurred while refreshing token");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Logout")]
        [EnableRateLimiting("AuthLimiter")]
        public IActionResult Logout([FromBody] LogoutRequest request){
            try {

                var student = StudentDataSimulation.studentList.FirstOrDefault(std => std.Email == request.Email);
                var isValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student!.HashedRefreshToken);

                //to avoid given hacker signal about doing hacking process and try to distracting the attacker
                if (student == null) return Ok("Logged out successfully");
                if (!isValid) return Ok("Logged out successfully");

                student!.RefreshTokenRevokedAt = DateTime.UtcNow;

                // also are you recommended to add next section to enable creating new refresh token ?
                student!.RefreshTokenExpiredAt = DateTime.UtcNow;
                student.HashedRefreshToken = null!;

                return Ok("Student Logged out successfully");

            } catch (Exception ex){
                _logger.LogError(ex, "Error occurred while logging out");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GenerationRefreshToken(){
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            //[ using ] Clean Memory after end of doing generation random number
            return Convert.ToBase64String(bytes);
        }

        private string GenerationAccessToken(Model.Student student){
            try
            {
                var JWTPayload = new[] {
                    new Claim(ClaimTypes.NameIdentifier , student.Id.ToString()!),
                    new Claim(ClaimTypes.Email , student.Email!),
                    new Claim(ClaimTypes.Role , student.Role!)
            };

                //Note: this key must EXIST in secured location [ settings ], not Hardcoded as exist here , here just for simulation!
                //This Part Must be more strong and secured!

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["JWTSettings:SecretKey"]!));
                var JWTHeader = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                //Payload be as: issuer + audience + expires + claims
                var JWTToken = new JwtSecurityToken(
                    issuer: this._configuration["JWTSettings:Issuer"],
                    audience: this._configuration["JWTSettings:Audience"],
                    expires: DateTime.UtcNow.AddMinutes(10),//_configuration.GetValue<int>("JwtSettings:ExpiringAt")
                    claims: JWTPayload,
                    signingCredentials: JWTHeader
                );

                string AccessToken = new JwtSecurityTokenHandler().WriteToken(JWTToken);
                return AccessToken;
            }
            catch (Exception ex){
                _logger.LogError(ex, "Error occurred while token generation");
                throw;
            }
        }
    
        private void RotationRefreshToken(Model.Student student,string refreshToken){
            //in Rotation we will creating new Refresh Token for level up security
            student.HashedRefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            student.RefreshTokenExpiredAt = DateTime.UtcNow.AddDays(7);
            student.RefreshTokenRevokedAt = null; // null here that mean refresh token work and valid!
        }
    
    }
}
