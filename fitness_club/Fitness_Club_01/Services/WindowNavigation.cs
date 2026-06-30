using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Fitness_Club_01.Services;

public static class WindowNavigation
{
    public static void ShowAsMain(Window window)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = window;

        window.WindowState = WindowState.Normal;
        window.Show();
        BringToFront(window);
    }

    /// <summary>
    /// Открывает второе окно, первое скрывает (оба остаются отдельными окнами приложения).
    /// </summary>
    public static void OpenCompanionWindow(Window hideCurrent, Window showNext)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = showNext;

        showNext.WindowState = WindowState.Normal;
        showNext.Show();

        Dispatcher.UIThread.Post(() =>
        {
            hideCurrent.Hide();
            BringToFront(showNext);
        }, DispatcherPriority.Loaded);
    }

    public static void ShowExistingMain(Window window)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = window;

        window.WindowState = WindowState.Normal;
        window.Show();
        BringToFront(window);
    }

    public static void ReplaceMainWindow(Window current, Window next)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = next;

        next.WindowState = WindowState.Normal;
        next.Show();

        Dispatcher.UIThread.Post(() =>
        {
            BringToFront(next);
            current.Close();
        }, DispatcherPriority.Loaded);
    }

    public static void EnterApplication(Window register, Window? login, Window destination)
    {
        ShowAsMain(destination);

        Dispatcher.UIThread.Post(() =>
        {
            register.Close();
            login?.Close();
        }, DispatcherPriority.Loaded);
    }

    private static void BringToFront(Window window)
    {
        window.Activate();
        window.Focus();
    }
}
