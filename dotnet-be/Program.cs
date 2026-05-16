using System.Text;
using auth_dotnet_api.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using auth_dotnet_api.Data;

var builder = WebApplication.CreateBuilder(args);

#region Settings Config JWT 
//อ่านค่า JWT จาก appsettings.json
//และเก็บไว้ใน JwtSettings class

// var jwtSettings = new JwtSettings();
// builder.Configuration.Bind("JwtSettings", jwtSettings);
// builder.Services.AddSingleton(jwtSettings);
#endregion

#region Set jwt token
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
#endregion

#region Settings Config Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);
#endregion

builder.Services.AddRepositories();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


#region CORS
//อ่านค่า CORS จาก appsettings.json
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedHosts").Get<string[]>() ?? new string[] { };
        Console.WriteLine($"CORS Allowed Origins: {string.Join(", ", allowedOrigins)}");
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});
#endregion

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs

#region เพิ่ม Swagger ให้ Authentication ได้
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });

    // 🔐 Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    //เพิ่ม Swagger ในโหมดพัฒนา
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}

//ใช้ CORS ที่กำหนดไว้ใน
app.UseCors("AllowAll");

app.UseHttpsRedirection();

//เปิดใช้งาน Authentication
app.UseAuthentication(); //เปิดสำหรับการใช้งาน Authentication ที่ controller ถ้าไม่เปิดจะไม่สามารถใช้ [Authorize] ได้

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Welcome to My API!");

app.Run();
