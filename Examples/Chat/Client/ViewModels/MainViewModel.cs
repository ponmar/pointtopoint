using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using Protocol;
using System;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace Client.ViewModels;

public partial class MainViewModel : ObservableObject, IMessengerErrorReporter
{
    public bool CanConnect => IsDisconnected &&
        !string.IsNullOrEmpty(HostnameInput) &&
        int.TryParse(PortInput, out var port) && port <= IPEndPoint.MaxPort && port >= IPEndPoint.MinPort;

    public bool IsConnected => Messenger is not null;
    public bool IsDisconnected => !IsConnected;

    public bool CanSendText => IsConnected && !string.IsNullOrEmpty(TextInput);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string hostnameInput = "127.0.0.1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string portInput = Constants.Port.ToString();

    [ObservableProperty]
    private bool autoConnect;

    partial void OnAutoConnectChanged(bool value)
    {
        EvaluateAutoConnectTimer();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSendText))]
    private string textInput = string.Empty;

    [ObservableProperty]
    private string texts = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(CanSendText))]
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
            ShowText($"$Invalid port: {PortInput}");
            return;
        }

        try
        {
            Messenger = new TcpMessenger(HostnameInput, port,
                new NewtonsoftJsonPayloadSerializer(typeof(PublishText).Namespace!),
                new ReflectionMessageRouter(executor: Application.Current.Dispatcher.Invoke) { MessageHandler = this },
                this, 
                new SocketFactory());

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

    public void Disconnected(Guid messengerId, Exception? e)
    {
        if (IsConnected)
        {
            ShowText("Disconnected from server" + (e is not null ? $" ({e.Message})" : ""));
            Messenger = null;
        }
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
