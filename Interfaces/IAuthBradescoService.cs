using BradescoGateway.DTOs;

namespace BradescoGateway.Interfaces;

public interface IAuthBradescoService
{
    Task<TokenResponse> GetAuthToken();
}
