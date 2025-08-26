using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lofi.Data;
using lofi.Services;                   
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace lofi.ViewModels
{
    public partial class TodoItem : ObservableObject
    {
        [ObservableProperty] private int _id;                 
        [ObservableProperty] private string _content = string.Empty;
        [ObservableProperty] private bool _isCompleted;
    }

    public partial class TodoListViewModel : ObservableObject
    {
        [ObservableProperty] private Brush _textColor = Brushes.Black;
        [ObservableProperty] private string _newTodoText = string.Empty;

        public ObservableCollection<TodoItem> TodoItems { get; } = new();

        private readonly ITodoRepository _repo;

        public TodoListViewModel() : this(new SqliteTodoRepository()) { }

        public TodoListViewModel(ITodoRepository repo)
        {
            _repo = repo;
            UpdateTheme(ThemeMode.Light);

            TodoItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (var obj in e.NewItems)
                        if (obj is TodoItem it)
                            it.PropertyChanged += OnItemPropertyChanged;

                if (e.OldItems != null)
                    foreach (var obj in e.OldItems)
                        if (obj is TodoItem it)
                            it.PropertyChanged -= OnItemPropertyChanged;
            };

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _repo.InitializeAsync();  
            await RefreshAsync();           
        }

        private async void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not TodoItem item) return;
            if (e.PropertyName == nameof(TodoItem.IsCompleted))
            {
                if (item.Id > 0)
                    await _repo.SetDoneAsync(item.Id, item.IsCompleted);
            }
        }

        public void UpdateTheme(ThemeMode theme)
        {
            TextColor = (theme == ThemeMode.Light) ? Brushes.Black : Brushes.White;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            TodoItems.Clear();
            var rows = await _repo.GetAllAsync();
            foreach (var (id, content, isDone) in rows)
            {
                TodoItems.Add(new TodoItem
                {
                    Id = id,
                    Content = content,
                    IsCompleted = isDone
                });
            }
        }

        [RelayCommand]
        private async Task AddTodo()
        {
            if (string.IsNullOrWhiteSpace(NewTodoText)) return;

            var text = NewTodoText.Trim();
            var id = await _repo.AddAsync(text);
            TodoItems.Insert(0, new TodoItem
            {
                Id = id,
                Content = text,
                IsCompleted = false
            });
            NewTodoText = string.Empty;
        }

        [RelayCommand]
        private void ToggleTodo(TodoItem item)
        {
            if (item != null) item.IsCompleted = !item.IsCompleted;
        }

        [RelayCommand]
        private async Task RemoveTodo(TodoItem item)
        {
            if (item == null) return;
            if (item.Id > 0)
                await _repo.SoftDeleteAsync(item.Id);
            TodoItems.Remove(item);
        }
    }
}
