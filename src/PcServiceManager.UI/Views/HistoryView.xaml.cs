using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class HistoryView : UserControl
{
    public HistoryView(HistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
