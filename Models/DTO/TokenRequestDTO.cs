using System.ComponentModel.DataAnnotations;

namespace Rest_API.Models.DTO;

public class TokenRequestDTO {
    [Required]
    public string Token {get; set;}

    [Required]
    public string RefreshToken {get; set;}
}