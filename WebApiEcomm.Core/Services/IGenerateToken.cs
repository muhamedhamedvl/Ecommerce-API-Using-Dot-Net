using WebApiEcomm.Core.Entites.Identity;

namespace WebApiEcomm.Core.Services
{
    public interface IGenrateToken
    {
        Task<string> CreateTokenAsync(AppUser appUser);
    }
}
