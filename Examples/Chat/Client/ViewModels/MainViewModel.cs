using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.ErrorHandler;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using Protocol;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Client.ViewModels;

public partial class MainViewModel : ObservableObject, IMessengerErrorHandler
{
    public bool IsConnected => Messenger is not null;
    public bool IsDisconnected => !IsConnected;

    [ObservableProperty]
    private string hostnameInput = "127.0.0.1";

    [ObservableProperty]
    private string portInput = Constants.Port.ToString();

    [ObservableProperty]
    private bool autoConnect;

    partial void OnAutoConnectChanged(bool value)
    {
        EvaluateAutoConnectTimer();
    }

    [ObservableProperty]
    private string textInput = string.Empty;

    [ObservableProperty]
    private string texts = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    private IMessenger? messenger = null;

    partial void OnMessengerChanged(IMessenger? value)
    {
        EvaluateAutoConnectTimer();
    }

    private readonly DispatcherTimer autoConnectTimer = new() { Interval = TimeSpan.FromSeconds(3) };

    public MainViewModel()
    {
        autoConnectTimer.Tick += AutoConnectTimer_Tick;
    }

    private void AutoConnectTimer_Tick(object? sender, EventArgs e)
    {
        if (IsDisconnected)
        {
            Connect();
        }
    }

    [RelayCommand]
    private void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        if (!int.TryParse(PortInput, out var port))
        {
            ShowText($"Invalid port");
            return;
        }

        try
        {
            Messenger = new TcpMessenger(HostnameInput, port,
                new NewtonsoftJsonPayload(typeof(PublishText).Namespace),
                new ReflectionMessageRouter(executor: Application.Current.Dispatcher.Invoke) { MessageHandler = this },
                this);

            ShowText($"Connected to {HostnameInput}:{PortInput}");
            Messenger.Start();
        }
        catch (Exception e)
        {
            ShowText(e.Message);
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        Messenger?.Close();
        Messenger = null;
    }

    [RelayCommand]
    private void SendText()
    {
        if (Messenger is not null && !string.IsNullOrEmpty(TextInput))
        {
            Messenger.Send(new PublishText(TextInput));
            TextInput = string.Empty;
        }
    }

    public void HandleMessage(Text message, Guid senderId)
    {
        ShowText($"{message.Time:HH:mm:ss}: {message.Message}");
    }

    public void HandleMessage(KeepAlive message, Guid senderId)
    {
    }

    public void MessageRoutingException(Exception e, Guid messengerId)
    {
        ShowText($"Message routing exception: {e.Message}");
    }

    public void PayloadException(Exception e, Guid messengerId)
    {
        ShowText($"Payload exception: {e.Message}");
    }

    public void Disconnected(Guid messengerId)
    {
        ShowText("Disconnected from server");
    }

    private void ShowText(string text)
    {
        Texts += $"\n{text}";
    }

    private void EvaluateAutoConnectTimer()
    {
        if (IsDisconnected && AutoConnect)
        {
            ShowText("Reconnecting in 3 seconds...");
            autoConnectTimer.Start();
        }
        else
        {
            autoConnectTimer.Stop();
        }
    }

    public void Close()
    {
        Messenger?.Close();
    }
}
