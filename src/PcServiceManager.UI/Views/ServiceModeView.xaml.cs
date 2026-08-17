using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class ServiceModeView : UserControl
{
    public ServiceModeView(ServiceModeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
