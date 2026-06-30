using Avalonia.Controls;

namespace Fitness_Club_01.Views;

public partial class ReportPreviewWindow : Window
{
    public ReportPreviewWindow(string title, string content)
    {
        InitializeComponent();
        Title = title;
        ReportText.Text = content;
        CloseButton.Click += (_, _) => Close();
    }
}
