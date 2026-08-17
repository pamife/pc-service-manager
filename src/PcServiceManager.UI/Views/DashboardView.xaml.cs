using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    public DashboardView(DashboardViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
