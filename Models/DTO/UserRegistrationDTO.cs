using System.ComponentModel.DataAnnotations;

namespace Rest_API.Models.DTO;

public class UserRegistrationDTO {
    [Required]
    public string Name {get; set;}

    [Required]
    public string Email {get; set;}

    [Required]
    public string Password {get; set;}
}