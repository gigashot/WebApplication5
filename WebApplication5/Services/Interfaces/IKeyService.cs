using System.Threading.Tasks;

namespace WebApplication5.Services.Interfaces
{
    public interface IKeyService
    {
        Task<string> GetPublicKey(int userId);
    }
}
