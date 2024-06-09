namespace FastAccountSwitcher;

public record Account(string UserName, string SessionName, string Id, string State, string IdleTime, string LogonTime, bool IsLoggedIn);