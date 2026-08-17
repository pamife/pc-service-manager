using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class MaintenanceView : UserControl
{
    public MaintenanceView(MaintenanceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
