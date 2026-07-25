namespace SellFast.Core.Services
{
    public interface IWhatsAppService
    {
        void EnviarComprobanteVenta(string? telefono, string numeroTransaccion, decimal total, string nombreCliente, string nombreNegocio);
        void EnviarRecordatorioPago(string? telefono, string nombreCliente, decimal saldoPendiente, string nombreNegocio);
        void EnviarMensajeLibre(string? telefono, string mensaje);
    }
}
