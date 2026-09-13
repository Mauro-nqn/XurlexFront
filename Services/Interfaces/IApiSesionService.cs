using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IurixBlazor.Shared.Dtos;

namespace IurixBlazor.Services.Interfaces
{
    public interface IApiSesionService
    {
        Task<LoginResponseDto?> LoginAsync(UsuarioLoginDto loginDto);
        Task<bool> CerrarSesionAsync(string claveLicencia, string dispositivoSesionId, string token);
    }

   
}
