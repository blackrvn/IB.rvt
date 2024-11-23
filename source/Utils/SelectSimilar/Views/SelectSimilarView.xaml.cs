using Library.Views;
using SelectSimilar.ViewModels;

namespace SelectSimilar.Views
{
    public sealed partial class SelectSimilarView : BaseView
    {
        public SelectSimilarView(SelectSimilarViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}