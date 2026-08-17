using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class MaintenanceView : UserControl
{
    public MaintenanceView()
    {
        InitializeComponent();
    }

    public MaintenanceView(MaintenanceViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
