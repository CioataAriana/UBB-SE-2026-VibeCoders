namespace BoardRent.Utils
{
    public interface ILoginPreferenceStore
    {
        string GetRememberedUsername();

        void SaveRememberedUsername(string username);

        void ClearRememberedUsername();
    }
}
