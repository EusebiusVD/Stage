using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PiXeL_Apps.Logging
{
    public static class Feedback
    {
        static string filename = "Feedback.txt";
        static StorageFile file = null;
        static StorageFolder folder = null;

        public async static void writeFeedback(string feedback)
        {
            folder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("Feedback", CreationCollisionOption.OpenIfExists);
            file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            string date = DateTime.Now.ToString();
            string text = date + " (" + LocalDB.database.GetIngelogdeGebruiker().Username + "): \t" + feedback;
            await FileIO.WriteTextAsync(file, text);
        }

        public async static void writeFeedbackAnonymous(string feedback)
        {
            folder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("Feedback", CreationCollisionOption.OpenIfExists);
            file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            string date = DateTime.Now.ToString();
            string text = date + " (anoniem): \t" + feedback;
            await FileIO.WriteTextAsync(file, text);
        }

    }
}
