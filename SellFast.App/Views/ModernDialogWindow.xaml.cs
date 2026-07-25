using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SellFast.App.Views
{
    public enum DialogType
    {
        Info,
        Success,
        Warning,
        Error,
        Confirm,
        ReceiptSuccess
    }

    public partial class ModernDialogWindow : Window
    {
        public bool Result { get; private set; } = false;
        private string? _pdfPathToOpen = null;

        public ModernDialogWindow()
        {
            InitializeComponent();
        }

        public static bool Show(string title, string message, DialogType type = DialogType.Info, string? primaryText = null, string? secondaryText = null, string? pdfPath = null, Window? owner = null)
        {
            try
            {
                var dialog = new ModernDialogWindow();

                if (owner != null && owner.IsVisible)
                {
                    dialog.Owner = owner;
                }
                else if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible && Application.Current.MainWindow != dialog)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                dialog.TxtTitle.Text = title;
                dialog.TxtMessage.Text = message;
                dialog._pdfPathToOpen = pdfPath;

                if (!string.IsNullOrEmpty(pdfPath))
                {
                    dialog.BtnPdf.Visibility = Visibility.Visible;
                }

                switch (type)
                {
                    case DialogType.Success:
                    case DialogType.ReceiptSuccess:
                        dialog.TxtIcon.Text = "✅";
                        dialog.IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
                        dialog.BtnPrimary.Content = primaryText ?? "Aceptar";
                        if (secondaryText != null || pdfPath != null)
                        {
                            dialog.BtnSecondary.Content = secondaryText ?? "Cerrar";
                            dialog.BtnSecondary.Visibility = Visibility.Visible;
                        }
                        break;

                    case DialogType.Warning:
                        dialog.TxtIcon.Text = "⚠️";
                        dialog.IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEDD5"));
                        dialog.BtnPrimary.Content = primaryText ?? "Entendido";
                        break;

                    case DialogType.Error:
                        dialog.TxtIcon.Text = "❌";
                        dialog.IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                        dialog.BtnPrimary.Content = primaryText ?? "Aceptar";
                        break;

                    case DialogType.Confirm:
                        dialog.TxtIcon.Text = "❓";
                        dialog.IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF1F6"));
                        dialog.BtnPrimary.Content = primaryText ?? "Sí, Confirmar";
                        dialog.BtnSecondary.Content = secondaryText ?? "Cancelar";
                        dialog.BtnSecondary.Visibility = Visibility.Visible;
                        break;

                    default:
                        dialog.TxtIcon.Text = "ℹ️";
                        dialog.IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF1F6"));
                        dialog.BtnPrimary.Content = primaryText ?? "Aceptar";
                        break;
                }

                dialog.ShowDialog();
                return dialog.Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error mostrando ModernDialogWindow: {ex.Message}");
                var result = MessageBox.Show(message, title, type == DialogType.Confirm ? MessageBoxButton.YesNo : MessageBoxButton.OK);
                return result == MessageBoxResult.Yes || result == MessageBoxResult.OK;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnPdf_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_pdfPathToOpen) && File.Exists(_pdfPathToOpen))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(_pdfPathToOpen) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al abrir PDF: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No se encontró el archivo PDF generado en disco.", "PDF no disponible", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnPrimary_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void BtnSecondary_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
