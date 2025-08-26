using CommunityToolkit.Mvvm.ComponentModel; // ObservableObject

namespace lofi.Models
{
    public partial class TodoEntry : ObservableObject
    {
        [ObservableProperty] private int id;
        [ObservableProperty] private string title = "";
        [ObservableProperty] private bool isDone;
        [ObservableProperty] private int orderIndex;
    }
}