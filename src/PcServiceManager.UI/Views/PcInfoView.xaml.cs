using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class PcInfoView : UserControl
{
    public PcInfoView(PcInfoViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
