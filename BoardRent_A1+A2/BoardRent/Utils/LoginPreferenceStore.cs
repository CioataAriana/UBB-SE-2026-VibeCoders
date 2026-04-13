namespace BoardRent.Utils
{
    using Windows.Storage;

    public class LoginPreferenceStore : ILoginPreferenceStore
    {
        private const string RememberedUsernameKey = "RememberedUsername";

        public string GetRememberedUsername()
        {
            object storedValue = ApplicationData.Current.LocalSettings.Values[RememberedUsernameKey];
            return storedValue as string ?? string.Empty;
        }

        public void SaveRememberedUsername(string username)
        {
            ApplicationData.Current.LocalSettings.Values[RememberedUsernameKey] = username;
        }

        public void ClearRememberedUsername()
        {
            ApplicationData.Current.LocalSettings.Values.Remove(RememberedUsernameKey);
        }
    }
}
