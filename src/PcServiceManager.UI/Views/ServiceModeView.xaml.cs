using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class ServiceModeView : UserControl
{
    public ServiceModeView()
    {
        InitializeComponent();
    }

    public ServiceModeView(ServiceModeViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
