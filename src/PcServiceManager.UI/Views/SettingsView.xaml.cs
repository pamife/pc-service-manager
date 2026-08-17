using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
