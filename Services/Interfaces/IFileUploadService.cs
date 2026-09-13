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
    public interface IFileUploadService
    {
        Task<(string FileName, string Content)> PickFileAsync(string allowedExtension = "*");
        Task DownloadFileAsync(string fileName, string content);

        
    }

}