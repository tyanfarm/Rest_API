using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rest_API.Data;
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
    private readonly RestapiContext _context;
    // Xác thực jwt token
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthenticationController
    (
        UserManager<IdentityUser> userManager, 
        IConfiguration configuration, 
        IEmailSender emailSender,
        RestapiContext context,
        TokenValidationParameters tokenValidationParameters
    ) 
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
        _context = context;
        _tokenValidationParameters = tokenValidationParameters;
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

                // (http / https -- Scheme) + `://` + (localhost:5281 -- Host)
                var callback_url = Request.Scheme + "://" + Request.Host + 
                                    Url.Action("ConfirmEmail", "Authentication", 
                                                new {userId = new_user.Id, code = email_token}); 

                // Thay thế link vào chuỗi string
                var body = email_body.Replace("#URL#", callback_url);

                // Send EMAIL
                var subject = "Verify email";

                try {
                    // email receiver - Subject (chủ đề) - Content (nội dung mail)
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
            var jwtToken = await generateJwtToken(user);

            return Ok(jwtToken);
        }

        return BadRequest(new AuthResult() {
            Result = false,
            Errors = new List<string>() {
                "Invalid Payload"
            }
        });
    }

    [HttpPost]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDTO tokenRequestDTO) {
        if (ModelState.IsValid) {
            var result = await VerifyAndGenerateToken(tokenRequestDTO);

            if (result == null) {
                return BadRequest(new AuthResult() {
                    Result = false,
                    Errors = new List<string>() {
                        "Invalid tokens"
                    }
                });
            }   

            return Ok(_tokenValidationParameters.ValidateLifetime);
        }

        return BadRequest(new AuthResult() {
            Result = false,
            Errors = new List<string>() {
                "Invalid parameters"
            }
        });
    }

    private async Task<AuthResult> VerifyAndGenerateToken(TokenRequestDTO tokenRequestDTO) {
        var jwtTokenHanlder = new JwtSecurityTokenHandler();

        try {
            // Không kiểm tra thời gian sống vì cần kiểm tra nếu access token hết hạn
            // -> chuyển qua kiểm tra refresh token
            _tokenValidationParameters.ValidateLifetime = false;

            // Xác thực access token - trả về ClaimsPrincipal
            var tokenInVerification = jwtTokenHanlder.ValidateToken
                (tokenRequestDTO.Token, _tokenValidationParameters, out var validatedToken);
            // Kiểm tra xem token có được mã hóa bằng HMACSHA256 không
            if (validatedToken is JwtSecurityToken jwtSecurityToken) {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase);

                if (result == false) {
                    return null;
                }
            }

            // Nhận về giá trị UnixTimeStamp
            var utcExpiryDate = long.Parse(tokenInVerification.Claims   
                .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            // Chuyển đổi qua DateTime
            var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

            // Kiểm tra access token còn hiệu lực không
            if (expiryDate > DateTime.UtcNow) {
                return new AuthResult() {
                    Result = false,
                    Errors = new List<string>() {
                        "Valid Access Token"
                    }
                };
            }

            // Nếu access token hết hạn -> kiểm tra Refresh Token
            var storedToken = await _context.Refreshtokens.
                                    FirstOrDefaultAsync(x => x.Token == tokenRequestDTO.RefreshToken);

            // Kiểm tra refresh token có phải của access token gửi vào không
            // trả về type claim đầu tiên = jti
            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (storedToken.JwtId != jti) {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>(){
                        "Invalid tokens"
                    }
                };
            }
            
            // Kiểm tra các attribute khác của refresh token
            if (storedToken == null) {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>(){
                        "Invalid tokens"
                    }
                };
            }

            if (storedToken.IsUsed) {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>(){
                        "Invalid tokens"
                    }
                };
            }

            if (storedToken.IsRevoked) {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>(){
                        "Invalid tokens"
                    }
                };
            }

            // Kiểm tra refresh token hết hạn chưa
            if (storedToken.ExpiryDate < DateTime.UtcNow) {
                return new AuthResult() {
                    Result = false,
                    Errors = new List<string>(){
                        "Expired tokens"
                    }
                };
            }

            // Update token validation
            _tokenValidationParameters.ValidateLifetime = true;

            // Cập nhật refresh token đã sử dụng để generate access token mới
            storedToken.IsUsed = true;
            _context.Refreshtokens.Update(storedToken);
            await _context.SaveChangesAsync();

            // Return new access token
            var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
            return await generateJwtToken(dbUser);
        }
        catch (Exception e) {
            return new AuthResult() {
                Result = false,
                Errors = new List<string>(){
                    "Server error"
                }
            };
        }
    }

    // Hàm chuyển đổi UnixTimeStamp qua DateTime
    private DateTime UnixTimeStampToDateTime(long unixTimeStamp) {
        var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

        return dateTimeVal;
    }

    private async Task<AuthResult> generateJwtToken(IdentityUser user) {
        // thằng xử lý và generate ra token
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

        var refreshToken = new Refreshtoken() {
            JwtId = token.Id,       // jti
            Token = GenerateRandomString(24),            // generate a new refresh token 
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(2),
            IsRevoked = false,
            IsUsed = false,
            UserId = user.Id
        };

        // Add refresh
        await _context.Refreshtokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        var result = new AuthResult() {
            Result = true,
            RefreshToken = refreshToken.Token,
            Token = jwtToken
        };

        return result;
    }

    private string GenerateRandomString(int length) {
        var random = new Random();
        var chars = "QWERTYUIOPASDFGHJKLZXCVBNM1234567890qwertyuiopasdfghjklzxcvbnm_";

        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}