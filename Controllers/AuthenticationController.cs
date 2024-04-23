using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Rest_API.Models;
using Rest_API.Models.DTO;
using Rest_API.Services;

namespace Rest_API.Controllers;

[Route("api/v1/[controller]")]      // api/v1/authentication
[ApiController]
public class AuthenticationController : ControllerBase {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public AuthenticationController
    (
        UserManager<IdentityUser> userManager, 
        IConfiguration configuration, 
        IEmailSender emailSender
    ) 
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
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
                EmailConfirmed = false
            };

            // IdentityResult
            var is_created = await _userManager.CreateAsync(new_user, registerRequest.Password);

            if (is_created.Succeeded == true) {
                // create token
                var token = generateJwtToken(new_user);
            }
            else {
                // Password cần phải có kí tự A-Z, có kí tự số, có kí tự symbol
                return BadRequest(is_created);
            }

            // Verify Email
            if (new_user != null) {
                var email_token = await _userManager.GenerateEmailConfirmationTokenAsync(new_user);

                var email_body = $"Please confirm your email address by click here: #URL# ";

                // http / https + `://`
                var callback_url = Request.Scheme + "://" + Request.Host + 
                                    Url.Action("ConfirmEmail", "Authentication", 
                                                new {userId = new_user.Id, code = email_token}); 

                // mã hóa token thành các mã HTML hợp lệ
                var body = email_body.Replace("#URL#", callback_url);

                // Send EMAIL
                var subject = "Verify email";

                try {
                    await _emailSender.SendEmailAsync(new_user.Email, subject, body);

                    return Ok("Send email successfully");
                }
                catch {
                    return BadRequest(new AuthResult() {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Verify Email ERROR!"
                        }
                    });
                }
            }
        }

        return BadRequest();
    }

    [Route("ConfirmEmail")]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code) {
        if (userId == null || code == null) {
            return BadRequest(new AuthResult() 
            {
                Result = false,
                Errors = new List<string>()
                {
                    "Invalid email confirmation url"
                }
            });
        }

        var user = await _userManager.FindByIdAsync(userId);

        // Nếu email đã đăng kí
        if (user == null) {
            return BadRequest(new AuthResult() {
                Result = false,
                Errors = new List<string>()
                {
                    "Invalid email",
                    userId
                }
            });
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        var status = result.Succeeded
                    ? "Thank you for confirming email"
                    : "Your email is not confirmed, please try again later";

        return Ok(status);
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
                        "User isn't existed !"
                    }
                });
            }

            // Check email confirmed
            if (user.EmailConfirmed == false) {
                return BadRequest(new AuthResult() {
                    Result = false,
                    Errors = new List<string>() {
                        "Email haven't confirmed !"
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

            Expires = DateTime.UtcNow
                        .Add(TimeSpan.Parse( _configuration.GetSection("JwtConfig:ExpiryTimeFrame").Value)),

            // SymmetricSecurityKey - khóa đối xứng
            // mã hóa bằng thuật toán HMAC-SHA256
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHanlder.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHanlder.WriteToken(token);

        return jwtToken;
    }
}