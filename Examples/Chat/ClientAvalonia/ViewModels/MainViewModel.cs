using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PointToPoint.MessageRouting.CommunityToolkitMvvm;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload.NewtonsoftJson;
using PointToPoint.Protocol;
using Protocol;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using IMessenger = PointToPoint.Messenger.IMessenger;

namespace ClientAvalonia.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string ApplicationName = "Chat";
    private const int MessageEventChannel = 10;

    public string Title => IsConnected ? $"{ApplicationName} - {HostnameInput}:{PortInput}" : ApplicationName;

    public bool CanConnect => IsDisconnected &&
        !string.IsNullOrEmpty(HostnameInput) &&
        int.TryParse(PortInput, out var port) && port <= IPEndPoint.MaxPort && port >= IPEndPoint.MinPort;

    public bool IsConnected => Messenger is not null;
    public bool IsDisconnected => !IsConnected;

    public bool CanSendText => IsConnected && !string.IsNullOrEmpty(TextInput);
    public bool CanSetName => IsConnected && !string.IsNullOrEmpty(Name);

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
    [NotifyPropertyChangedFor(nameof(CanSetName))]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSendText))]
    private string textInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> messages = [];

    [ObservableProperty]
    private ChatMessageViewModel? selectedMessage;

    [ObservableProperty]
    private ObservableCollection<string> users = [];

    [ObservableProperty]
    private string? selectedUser;

    partial void OnSelectedUserChanged(string? value)
    {
        if (value is not null)
        {
            TextInput += value;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(CanSendText))]
    [NotifyPropertyChangedFor(nameof(CanSetName))]
    [NotifyPropertyChangedFor(nameof(Title))]
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

        WeakReferenceMessenger.Default.Register<AssignName, int>(this, MessageEventChannel, (r, t) => Name = t.Name);
        WeakReferenceMessenger.Default.Register<Text, int>(this, MessageEventChannel, (r, t) => ShowText(new ChatMessageViewModel(t.Sender, t.Time, t.Message, false)));
        WeakReferenceMessenger.Default.Register<KeepAlive, int>(this, MessageEventChannel, (r, t) => keepAliveReceivedAt = DateTime.Now);
        WeakReferenceMessenger.Default.Register<Users, int>(this, MessageEventChannel, (r, t) =>
        {
            Users.Clear();
            foreach (var user in t.Names.Order())
            {
                Users.Add(user);
            }
        });
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
        if (Messenger != null)
        {
            KeepAliveSupervisionStatus = DateTime.Now - keepAliveReceivedAt < 2 * Messenger.KeepAliveSendInterval ? "Good" : "Bad";
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

        Messages.Clear();
        Users.Clear();

        try
        {
            Messenger = new TcpMessenger(HostnameInput, port,
                new NewtonsoftJsonPayloadSerializer(typeof(PublishText).Assembly),
                new CommunityToolkitMvvmEventMessageRouter(WeakReferenceMessenger.Default, MessageEventChannel, Dispatcher.UIThread.Invoke),
                new SocketFactory(),
                TcpMessenger.DefaultSocketOptions);

            Messenger.Disconnected += Messenger_Disconnected;
            Messenger.Start();

            ShowText($"Connected to {HostnameInput}:{PortInput}");
        }
        catch (Exception e)
        {
            ShowText($"Connect error: {e.Message}");
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        ShowText("Disconnected from server");
        CloseMessenger();
    }

    private void CloseMessenger()
    {
        if (Messenger is not null)
        {
            Messenger.Disconnected -= Messenger_Disconnected;
            Messenger.Stop();
            Messenger = null;
        }
    }

    private void Messenger_Disconnected(object? sender, Exception? e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (IsConnected)
            {
                ShowText("Disconnected from server" + (e is not null ? $" ({e.Message})" : ""));
                CloseMessenger();
            }
        });
    }

    [RelayCommand]
    private void SetName()
    {
        Messenger!.Send(new ChangeName(Name));
    }

    [RelayCommand]
    private void SendText()
    {
        Messenger!.Send(new PublishText(TextInput));
        TextInput = string.Empty;
    }

    private void ShowText(ChatMessageViewModel text)
    {
        var autoScroll = !Messages.Any() || SelectedMessage is null || SelectedMessage == Messages.Last();
        Messages.Add(text);
        if (autoScroll)
        {
            SelectedMessage = Messages.Last();
        }
    }

    private void ShowText(string text)
    {
        ShowText(new ChatMessageViewModel(string.Empty, DateTime.Now, text, true));
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

    public void ExitApplication()
    {
        CloseMessenger();
    }
}
