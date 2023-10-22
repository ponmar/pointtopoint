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

public partial class MainViewModel : ObservableObject
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
    private string portInput = Constants.DefaultPort.ToString();

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
    private readonly DispatcherTimer keepAliveSupervisionTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };

    private DateTime keepAliveReceivedAt = DateTime.MinValue;

    [ObservableProperty]
    private string keepAliveSupervisionStatus = string.Empty;

    public MainViewModel()
    {
        autoConnectTimer.Tick += AutoConnectTimer_Tick;
        keepAliveSupervisionTimer.Tick += KeepAliveSupervisionTimer_Tick;
        keepAliveSupervisionTimer.Start();
    }

    private void AutoConnectTimer_Tick(object? sender, EventArgs e)
    {
        if (IsDisconnected)
        {
            Connect();
        }
    }

    private void KeepAliveSupervisionTimer_Tick(object? sender, EventArgs e)
    {
        KeepAliveSupervisionStatus = DateTime.Now - keepAliveReceivedAt < 2 * Constants.KeepAliveSendInterval ? "Good" : "Bad";
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
                new ReflectionMessageRouter(Application.Current.Dispatcher.Invoke) { MessageHandler = this },
                new SocketFactory(),
                Constants.KeepAliveSendInterval);

            Messenger.Disconnected += Messenger_Disconnected;
            Messenger.Start();

            ShowText($"Connected to {HostnameInput}:{PortInput}");
        }
        catch (Exception e)
        {
            ShowText(e.Message);
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        if (Messenger is not null)
        {
            Messenger.Disconnected -= Messenger_Disconnected;
            Messenger.Close();
            Messenger = null;
            ShowText("Disconnected from server");
        }
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

    public void HandleMessage(Text message, IMessenger messenger)
    {
        if (messenger == Messenger)
        {
            ShowText($"{message.Time:HH:mm:ss}: {message.Message}");
        }        
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
        if (messenger == Messenger)
        {
            keepAliveReceivedAt = DateTime.Now;
        }
    }

    private void Messenger_Disconnected(object? sender, MessengerDisconnected disconnected)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (IsConnected)
            {
                ShowText("Disconnected from server" + (disconnected.Exception is not null ? $" ({disconnected.Exception.Message})" : ""));
                Messenger!.Disconnected -= Messenger_Disconnected;
                Messenger = null;
            }
        });
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
        if (Messenger is not null)
        {
            Messenger.Disconnected -= Messenger_Disconnected;
            Messenger.Close();
            Messenger = null;
        }
    }
}
