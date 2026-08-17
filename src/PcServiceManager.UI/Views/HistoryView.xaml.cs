using System.Windows.Controls;
using PcServiceManager.UI.ViewModels;

namespace PcServiceManager.UI.Views;

public partial class HistoryView : UserControl
{
    public HistoryView()
    {
        InitializeComponent();
    }

    public HistoryView(HistoryViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
