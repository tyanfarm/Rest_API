using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Rest_API.Models;
using Rest_API.Models.DTO;

namespace Rest_API.Controllers;

[Route("api/v1/[controller]")]      // api/v1/authentication
[ApiController]
public class AuthenticationController : ControllerBase {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration) {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDTO registerRequest) {
        // Validate the request
        if (ModelState.IsValid) {
            // Check if email already exist
            var user_exist = await _userManager.FindByEmailAsync(registerRequest.Email);

            if (user_exist != null) {
                return BadRequest(new AuthResult() {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Email already exist!"
                    }
                });
            }

            // create a user
            var new_user = new IdentityUser() {
                Email = registerRequest.Email,
                UserName = registerRequest.Email,
            };

            // IdentityResult
            var is_created = await _userManager.CreateAsync(new_user, registerRequest.Password);

            if (is_created.Succeeded == true) {
                // create token
                var token = generateJwtToken(new_user);

                return Ok(new AuthResult() 
                {
                    Result = true,
                    Token = token
                });
            }

            // Password cần phải có kí tự A-Z, có kí tự số, có kí tự symbol
            return BadRequest(is_created);
        }

        return BadRequest();
    }

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO loginRequest) {
        if (ModelState.IsValid) {
            // Check if the user exist
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null) {
                return BadRequest(new AuthResult() {
                    Result = false,
                    Errors = new List<string>() {
                        "User isn't existed"
                    }
                });
            }

            // Validate password
            var check_password = _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (check_password.Result == false) {
                return BadRequest(new AuthResult() {
                    Result = false,
                    Errors = new List<string>() {
                        "Invalid Credentials"
                    }
                });
            }

            // create token
            var jwtToken = generateJwtToken(user);

            return Ok(new AuthResult() {
                Result = true,
                Token = jwtToken
            });
        }

        return BadRequest(new AuthResult() {
            Result = false,
            Errors = new List<string>() {
                "Invalid Payload"
            }
        });
    }

    private string generateJwtToken(IdentityUser user) {
        var jwtTokenHanlder = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

        // Token description
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            // Khai báo claims
            Subject = new ClaimsIdentity(new []
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),

                // JWT ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // Issued At Time - Chứa thời điểm Token được tạo
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
            }),

            Expires = DateTime.Now.AddHours(1),
            // SymmetricSecurityKey - khóa đối xứng
            // mã hóa bằng thuật toán HMAC-SHA256
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHanlder.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHanlder.WriteToken(token);

        return jwtToken;
    }
}