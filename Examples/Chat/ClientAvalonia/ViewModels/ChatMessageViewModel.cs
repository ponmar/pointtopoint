using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Protocol;

namespace ClientAvalonia.ViewModels;

public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime time;

    [ObservableProperty]
    private string sender;

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private bool isLocalMessage;

    public ChatMessageViewModel(Text text)
    {
        Time = text.Time;
        Sender = text.Sender;
        Message = text.Message;
        IsLocalMessage = false;
    }

    public ChatMessageViewModel(string message)
    {
        Time = DateTime.Now;
        Sender = string.Empty;
        Message = message;
        IsLocalMessage = true;
    }
}
