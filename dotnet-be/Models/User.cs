using System;
using System.ComponentModel.DataAnnotations;
using auth_dotnet_api.Enum;
namespace auth_dotnet_api.Models;

public class User : Base
{
    [MaxLength(50)]
    public string? Firstname { get; set; }
    [MaxLength(50)]
    public string? Lastname { get; set; }
    [MaxLength(50)]
    public string Username { get; set; }
    public string? UserID { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; }
    [Required]
    public RoleType Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; } = null;

    // [Column(TypeName = "decimal(18,2)")]
    // public decimal Salary { get; set; }
}
