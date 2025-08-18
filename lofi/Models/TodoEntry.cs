using CommunityToolkit.Mvvm.ComponentModel; // ObservableObject

namespace lofi.Models
{
    public partial class TodoEntry : ObservableObject
    {
        [ObservableProperty] private string title = "";
        [ObservableProperty] private bool isDone;
    }
}
