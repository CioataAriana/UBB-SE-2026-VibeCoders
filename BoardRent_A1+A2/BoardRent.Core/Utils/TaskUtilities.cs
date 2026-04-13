namespace BoardRent.Utils
{
    using System;
    using System.Threading.Tasks;

    public static class TaskUtilities
    {
        /// <summary>
        /// Lansează un task fără a-l aștepta, dar gestionează eventualele erori 
        /// pentru a preveni crash-ul aplicației.
        /// </summary>
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                // Aici se poate adăuga logica de logare a erorii
                // De exemplu: Debug.WriteLine(exception.Message);

                // Într-o aplicație profesională, aici ai trimite eroarea 
                // către un serviciu de monitorizare.
            }
        }
    }
}