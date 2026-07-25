using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace SellFast.Core.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public void EnviarComprobanteVenta(string? telefono, string numeroTransaccion, decimal total, string nombreCliente, string nombreNegocio)
        {
            string mensaje = $"Hola {nombreCliente}, gracias por tu compra en *{nombreNegocio}*.\n\n" +
                             $"📄 *Comprobante:* #{numeroTransaccion}\n" +
                             $"💰 *Total Pagado:* ${total:N0}\n" +
                             $"📅 *Fecha:* {DateTime.Now:dd/MM/yyyy HH:mm}\n\n" +
                             $"¡Esperamos verte pronto de nuevo! ⚡";

            EnviarMensajeLibre(telefono, mensaje);
        }

        public void EnviarRecordatorioPago(string? telefono, string nombreCliente, decimal saldoPendiente, string nombreNegocio)
        {
            string mensaje = $"Hola {nombreCliente}, te saludamos de *{nombreNegocio}*.\n\n" +
                             $"📌 Queremos recordarte cordialmente que tienes un saldo pendiente por valor de *${saldoPendiente:N0}*.\n\n" +
                             $"Si ya realizaste tu pago, por favor omite este mensaje. ¡Quedamos atentos a tus comentarios! 😊";

            EnviarMensajeLibre(telefono, mensaje);
        }

        public void EnviarMensajeLibre(string? telefono, string mensaje)
        {
            string cleanPhone = LimpiarTelefono(telefono);
            string encodedMsg = Uri.EscapeDataString(mensaje);

            string url = string.IsNullOrEmpty(cleanPhone)
                ? $"https://api.whatsapp.com/send?text={encodedMsg}"
                : $"https://wa.me/{cleanPhone}?text={encodedMsg}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir WhatsApp: {ex.Message}");
            }
        }

        private string LimpiarTelefono(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            // Remove non-digit characters
            return Regex.Replace(input, @"[^\d]", "");
        }
    }
}
