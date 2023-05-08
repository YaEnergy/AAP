namespace AAP
{
    public partial class AAPLoadingForm : Form
    {
        public AAPLoadingForm()
        {
            InitializeComponent();
        }

        public void UpdateInfo(int percentage, object? state)
        {
            loadingLabel.Text = state == null ? string.Empty : state.ToString();

            progressBar.Value = percentage;
        }
    }
}