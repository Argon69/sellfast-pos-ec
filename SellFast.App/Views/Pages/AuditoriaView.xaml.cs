using System.Windows.Controls;
using SellFast.App.ViewModels;

namespace SellFast.App.Views.Pages
{
    public partial class AuditoriaView : UserControl
    {
        public AuditoriaView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is AuditoriaViewModel vm)
                {
                    await vm.CargarAuditLogsAsync();
                }
            };
        }
    }
}
