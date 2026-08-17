using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class FirstLaunchWizardView : UserControl
{
    public FirstLaunchWizardView(FirstLaunchWizardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
