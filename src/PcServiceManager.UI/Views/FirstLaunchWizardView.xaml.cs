using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class FirstLaunchWizardView : UserControl
{
    public FirstLaunchWizardView()
    {
        InitializeComponent();
    }

    public FirstLaunchWizardView(FirstLaunchWizardViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
