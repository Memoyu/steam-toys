namespace SteamToys.Wpfui.Views.Pages
{
    /// <summary>
    /// Parameter.xaml 的交互逻辑
    /// </summary>
    public partial class Parameter : INavigableView<ParameterViewModel>
    {
        public ParameterViewModel ViewModel { get; }

        public Parameter(ParameterViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
