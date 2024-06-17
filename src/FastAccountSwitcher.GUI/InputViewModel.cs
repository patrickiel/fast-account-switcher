namespace FastAccountSwitcher.GUI;

public partial class InputViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private bool rembemberPassword = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Description))]
    private string accountName = "";

    public string Description => $"Enter the password for '{AccountName}'";

    public Action<bool, string> OkAction { get; internal set; } = (_, _) => { };

    public Action CancelAction { get; internal set; } = () => { };

    [RelayCommand]
    public void Ok()
        => OkAction(RembemberPassword, InputText);

    [RelayCommand]
    public void Cancel()
        => CancelAction();
}
