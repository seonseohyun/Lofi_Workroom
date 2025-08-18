using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace lofi.ViewModels
{
    public partial class TodoItem : ObservableObject
    {
        [ObservableProperty] private string _content = string.Empty;
        [ObservableProperty] private bool _isCompleted;
    }

    public partial class TodoListViewModel : ObservableObject
    {
        // 이미지 의존성 제거
        [ObservableProperty] private Brush _textColor = Brushes.Black;

        public ObservableCollection<TodoItem> TodoItems { get; } = new();

        [ObservableProperty] private string _newTodoText = string.Empty;

        public TodoListViewModel()
        {
            UpdateTheme(ThemeMode.Light); // ThemeMode는 같은 네임스페이스의 enum (MainViewModel에 있음)
        }

        public void UpdateTheme(ThemeMode theme)
        {
            TextColor = (theme == ThemeMode.Light) ? Brushes.Black : Brushes.White;
        }

        [RelayCommand]
        private void AddTodo()
        {
            if (!string.IsNullOrWhiteSpace(NewTodoText))
            {
                TodoItems.Add(new TodoItem { Content = NewTodoText });
                NewTodoText = string.Empty;
            }
        }

        [RelayCommand]
        private void ToggleTodo(TodoItem item)
        {
            if (item != null) item.IsCompleted = !item.IsCompleted;
        }

        [RelayCommand]
        private void RemoveTodo(TodoItem item)
        {
            if (item != null) TodoItems.Remove(item);
        }


    }
}
