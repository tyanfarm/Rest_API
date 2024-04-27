namespace Rest_API.Models;

public class RefreshToken {
    public int Id {get; set;}

    public string UserId {get; set;}

    public string Token {get; set;}

    public string JwtId {get; set;}

    // Sau khi sử dụng Refresh Token để generate ra Access token mới -> IsUsed = 1
    public bool IsUsed {get; set;}

    // Thu hồi
    public bool IsRevoked {get; set;}

    public DateTime AddedDate {get; set;}

    public DateTime ExpiryDate {get; set;}
}