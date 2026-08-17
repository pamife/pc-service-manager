using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class PcInfoView : UserControl
{
    public PcInfoView()
    {
        InitializeComponent();
    }

    public PcInfoView(PcInfoViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
