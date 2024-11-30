using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClientAvalonia.ViewModels;

public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private string sender;

    [ObservableProperty]
    private DateTime time;

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private bool isLocalMessage;

    public ChatMessageViewModel(string sender, DateTime time, string message, bool isLocalMessage)
    {
        Time = time;
        Sender = sender;
        Message = message;
        IsLocalMessage = isLocalMessage;
    }
}
