using System.Security.Cryptography;
using System.Text;
using auth_dotnet_api.Data;
using auth_dotnet_api.Models;
using auth_dotnet_api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace auth_dotnet_api.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetUserByLoginAsync(RequestLogin request)
        {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == hash);
        }
        public async Task<User?> GetUserByLoginOauthAsync(string userID)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userID);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            // user.Updated();
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}