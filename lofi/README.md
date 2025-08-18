# lofi (MVVM 정리본)

- .NET: `net8.0-windows7.0`
- WPF + CommunityToolkit.Mvvm
- AssemblyName/RootNamespace: `lofi`

## 구조
- `App.xaml` : Application 정의 (리소스 포함)
- `MainWindow.xaml` : 배경 바인딩 + 테마 토글 버튼 + `Views/TodoList` 포함
- `Views/TodoList` : 투두 위젯(이미지 배경, Add/Toggle/Remove)
- `ViewModels/MainViewModel` : 테마 토글, 배경/버튼 아이콘, `TodoListViewModel` 제공
- `ViewModels/TodoListViewModel` : 위젯 배경/텍스트색, 리스트/커맨드, 테마 반영
- `Resources/` : `light.jpg`, `dark.jpg`, `lightmodebtn.jpg`, `darkmodebtn.jpg`, `lighttodo.jpg`, `darktodo.jpg`

## 빌드 방법
1. `lofi.csproj`를 Visual Studio로 열기
2. NuGet 복원(CommunityToolkit.Mvvm)
3. 빌드/실행

## 참고
- pack URI는 `pack://application:,,,/lofi;component/Resources/...` 형식으로 명시
- 같은 어셈블리라 XAML에서는 `/Resources/...` 단축도 가능하지만 코드에서는 pack URI를 사용
