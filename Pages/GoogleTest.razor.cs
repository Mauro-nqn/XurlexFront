using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Net.Http;

namespace IurixBlazor.Pages
{
    public class GoogleTestBase : ComponentBase
    {
        [Inject] protected GoogleService? GoogleService { get; set; }
        [Inject] protected UsuarioService? UsuarioService { get; set; }

        protected List<UsuarioDto> Usuarios = new();
        protected int UsuarioSeleccionadoId;
        protected string EmailVinculado = string.Empty;

        // Evento
        protected string TituloEvento { get; set; } = "";
        protected string DescripcionEvento { get; set; } = "";
        protected DateTime FechaInicio { get; set; } = DateTime.Today;
        protected DateTime FechaFin { get; set; } = DateTime.Today;
        protected TimeOnly? HoraInicio { get; set; } = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9));
        protected TimeOnly? HoraFin { get; set; } = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10));

        protected bool EsFechaValida =>
            FechaFin.Add((HoraFin?.ToTimeSpan() ?? TimeSpan.Zero)) >
            FechaInicio.Add((HoraInicio?.ToTimeSpan() ?? TimeSpan.Zero));

        // Correo
        protected string Destinatario { get; set; } = "";
        protected string AsuntoCorreo { get; set; } = "";
        protected string CuerpoCorreo { get; set; } = "";

        [Inject] private IJSRuntime JS { get; set; } = default!;

        protected string Token { get; set; } = string.Empty;

        // Archivo
        private IBrowserFile? ArchivoSeleccionado;

        protected override async Task OnInitializedAsync()
        {
            Usuarios = await UsuarioService!.ObtenerUsuariosAsync();

            // 🔑 Traer token desde localStorage // revisar si es el token para authorize
            // en el endpoint y luego hay que agregarlo en cada llamada al backend
            Token = await JS.InvokeAsync<string>("localStorage.getItem", "token");

            if (!string.IsNullOrEmpty(Token))
            {
                GoogleService!.ConfigurarToken(Token); // 🔧 Método en GoogleService que setea el header
            }
        }

        protected void OnUsuarioSeleccionado(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var id))
                UsuarioSeleccionadoId = id;
        }



        private async Task MostrarToast(string mensaje, string tipo = "success")
        {
            await JS.InvokeVoidAsync("mostrarToast", mensaje, tipo);
        }


        //protected async Task ConsultarEmail()
        //{
        //    if (UsuarioSeleccionadoId == 0)
        //    {
        //        EmailVinculado = "⚠️ Seleccioná un usuario.";
        //        return;
        //    }

        //    var estado = await GoogleService.ObtenerEmailGoogleAsync(UsuarioSeleccionadoId);
        //    EmailVinculado = estado?.Email ?? "❌ No vinculado.";
        //}


        protected async Task ConsultarEmail()
        {
            if (UsuarioSeleccionadoId == 0)
            {
                await MostrarToast("⚠️ Seleccioná un usuario.", "warning");
                return;
            }

            var estado = await GoogleService!.ObtenerEmailGoogleAsync(UsuarioSeleccionadoId);

            if (estado == null || string.IsNullOrEmpty(estado.Email))
            {
                await MostrarToast("❌ No hay cuenta de Google vinculada.", "error");
            }
            else
            {
                await MostrarToast($"📧 Email vinculado: {estado.Email}", "success");
                EmailVinculado = estado.Email; // Si además querés mostrarlo en pantalla
            }
        }


        //protected async Task CrearEvento()
        //{
        //    if (!EsFechaValida) return;

        //    var dto = new CrearEventoDto
        //    {
        //        Titulo = TituloEvento,
        //        Descripcion = DescripcionEvento,
        //        FechaInicio = FechaInicio.Add((HoraInicio?.ToTimeSpan() ?? TimeSpan.Zero)).ToUniversalTime(),
        //        FechaFin = FechaFin.Add((HoraFin?.ToTimeSpan() ?? TimeSpan.Zero)).ToUniversalTime()
        //    };

        //    var msg = await GoogleService.CrearEventoAsync(UsuarioSeleccionadoId, dto);
        //    EmailVinculado = $"📅 Evento: {msg}";
        //}

        protected async Task CrearEvento()
        {
            if (UsuarioSeleccionadoId == 0)
            {
                await MostrarToast("Selecciona un usuario antes de continuar.", "error");
                return;
            }

            // 🔍 Validar el token de Google
            var estado = await GoogleService!.ObtenerEstadoGoogleAsync(UsuarioSeleccionadoId);
            if (estado == null)
            {
                await MostrarToast("No se pudo obtener el estado de Google.", "error");
                return;
            }

            // ⏳ Refrescar token si expiró
            if (estado.Expira <= DateTime.UtcNow)
            {
                //var refrescado = await GoogleService.RefrescarTokenAsync(UsuarioSeleccionadoId);
                //if (!refrescado)
                //{
                //    await MostrarToast("Token expirado. Vuelve a vincular tu cuenta de Google.", "error");
                //    return;
                //}

                var estadoOk = await GoogleService.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId);
                if (!estadoOk)
                {
                    await MostrarToast("Refresh falló (ver consola/log). Probable invalid_grant → re-vincular.", "error");
                    return;
                }

            }

            // ✅ Ahora el token es válido, crear el evento
            var dto = new CrearEventoDto
            {
                Titulo = TituloEvento,
                Descripcion = DescripcionEvento,
                FechaInicio = FechaInicio.Add((HoraInicio?.ToTimeSpan() ?? TimeSpan.Zero)).ToUniversalTime(),
                FechaFin = FechaFin.Add((HoraFin?.ToTimeSpan() ?? TimeSpan.Zero)).ToUniversalTime()
            };

            var msg = await GoogleService.CrearEventoAsync(UsuarioSeleccionadoId, dto);
            await MostrarToast($"📅 {msg}", "success");
        }


        protected async Task EnviarCorreo()
        {
            if (UsuarioSeleccionadoId == 0) return;

            if (!await GoogleService!.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId))
            {
                await MostrarToast("Debes vincular o refrescar tu cuenta de Google.", "error");
                return;
            }


            var dto = new CrearEmailDto
            {
                Destinatario = Destinatario,
                Asunto = AsuntoCorreo,
                Cuerpo = CuerpoCorreo
            };

            var msg = await GoogleService.EnviarCorreoAsync(UsuarioSeleccionadoId, dto);
            EmailVinculado = $"📧 Gmail: {msg}";
            await MostrarToast($"📧 {msg}", "success"); // 👈 AÑADIDO
        }

        protected void OnArchivoSeleccionado(InputFileChangeEventArgs e)
        {
            ArchivoSeleccionado = e.File;
        }

        protected async Task SubirArchivo()
        {
            if (UsuarioSeleccionadoId == 0 || ArchivoSeleccionado == null) return;

            if (!await GoogleService!.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId))
            {
                await MostrarToast("Debes vincular o refrescar tu cuenta de Google.", "error");
                return;
            }

            await using var stream = ArchivoSeleccionado.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            var msg = await GoogleService.SubirArchivoAsync(UsuarioSeleccionadoId, stream, ArchivoSeleccionado.Name);
            EmailVinculado = $"📁 Drive: {msg}";
            await MostrarToast($"📧 {msg}", "success"); // 👈 AÑADIDO
        }








    }
}
