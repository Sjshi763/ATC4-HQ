using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ATC4_HQ.ViewModels;

namespace ATC4_HQ;

public class ViewLocator : IDataTemplate
{

    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        // 如果找不到，尝试在Views命名空间中查找
        var viewName = "ATC4_HQ.Views." + param.GetType().Name.Replace("ViewModel", "");
        type = Type.GetType(viewName);
        
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        // 如果还是找不到，尝试加上View后缀
        viewName = "ATC4_HQ.Views." + param.GetType().Name.Replace("ViewModel", "View");
        type = Type.GetType(viewName);
        
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
