using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClientAvalonia.ViewModels;

public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Sender { get; set; }

    [ObservableProperty]
    public partial DateTime Time { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; }

    [ObservableProperty]
    public partial bool IsLocalMessage { get; set; }

    public ChatMessageViewModel(string sender, DateTime time, string message, bool isLocalMessage)
    {
        Time = time;
        Sender = sender;
        Message = message;
        IsLocalMessage = isLocalMessage;
    }
}
