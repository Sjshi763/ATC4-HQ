using CommunityToolkit.Mvvm.ComponentModel; // 确保您已安装 CommunityToolkit.Mvvm NuGet 包

namespace ATC4_HQ.ViewModels
{
    // ViewModelBase 继承 ObservableObject，用于提供 INotifyPropertyChanged 实现
    public partial class ViewModelBase : ObservableObject
    {
        // 可以在这里添加所有 ViewModel 都可能需要的通用属性或方法
    }
}