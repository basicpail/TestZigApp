using DegaussingTestZigApp.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace DegaussingTestZigApp.Views.Pages
{
    public partial class DataPage : INavigableView<DataViewModel>
    {
        public DataViewModel ViewModel { get; }

        public DataPage(DataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            InitializeComponent();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
    }
}
