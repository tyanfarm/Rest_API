using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rest_API.Data;
using Rest_API.Private;
using Rest_API.Repositories;
using Rest_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// DbContext
var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));
builder.Services.AddDbContext<RestapiContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
});

// Info of gmail sender
builder.Services.AddSingleton<EmailInfo>();

// Caching
builder.Services.AddScoped<ICacheService, CacheService>();

// Repositories
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

// Services
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Key for checking JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);
var tokenValidationParameter = new TokenValidationParameters() 
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,     // for dev
    ValidateAudience = false,   // for dev
        
    // Kiểm tra token có ngày hết hạn không
    RequireExpirationTime = true,      // for dev - need to update when refresh token is added

    // Kiểm tra token còn sống không
    ValidateLifetime = true
};

// Authentication
builder.Services
.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Middleware xác thực JWT
.AddJwtBearer(jwt => 
{
    // token sẽ được lưu trong HttpContext sau khi xác thực
    jwt.SaveToken = true;

    jwt.TokenValidationParameters = tokenValidationParameter;
});

builder.Services.AddSingleton(tokenValidationParameter);


// Add ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<RestapiContext>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
        securityScheme: new OpenApiSecurityScheme {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization : `Bearer Generated-JWT-Token`",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement 
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            }, 
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

