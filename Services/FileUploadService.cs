using Microsoft.JSInterop;
using IurixBlazor.Services.Interfaces;
using System.Text;

namespace IurixBlazor.Services
{ 
public class FileUploadService : IFileUploadService
{
    private readonly IJSRuntime _js;

    public FileUploadService(IJSRuntime js)
    {
        _js = js;
    }

        public async Task<(string FileName, string Content)> PickFileAsync(string allowedExtension = "*")
        {
            var file = await _js.InvokeAsync<UploadedFile>("pickFile", allowedExtension);
            return (file.FileName, file.Content); // El Content ya es texto PEM completo
        }





        //SI USAMOS ESTA FUNCION LO COMPRIME EN BASE64 
        //public async Task DownloadFileAsync(string fileName, string content)
        //{
        //    var bytes = Encoding.UTF8.GetBytes(content);
        //    var base64 = Convert.ToBase64String(bytes);
        //    await _js.InvokeVoidAsync("downloadFile", fileName, base64);
        //}


        public async Task DownloadFileAsync(string fileName, string content)
        {
            await _js.InvokeVoidAsync("downloadFile", fileName, "application/x-pem-file", content);
        }



        public class UploadedFile
        {
            public string FileName { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty; // Base64
        }



    }


}