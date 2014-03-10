using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Collections.Generic;

namespace PiXeL_Apps.Common
{
    static class CopyFiles
    {
        /// <summary>
        /// Copying videos to an USB device
        /// </summary>
        /// <returns></returns>
        public async static Task copyVideosToUSB() 
        {
            //Setting the source folder
            StorageFolder sourceFolder;
            sourceFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");

            //Setting the destination folder
            StorageFolder destinationFolder;
            destinationFolder = await KnownFolders.RemovableDevices.CreateFolderAsync("Videos", CreationCollisionOption.ReplaceExisting);

            //Reading the files in the source folder
            IReadOnlyList <StorageFile> videos = await sourceFolder.GetFilesAsync();

            //Copying the videos
            foreach (StorageFile video in videos)
            {
                await video.CopyAsync(destinationFolder);
            }

        }

        public async static Task copyVideosViaNetwork()
        {
            //Setting the source folder
            StorageFolder sourceFolder;
            sourceFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");

            //Setting the destination folder
            StorageFolder destinationFolder;
            destinationFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Videos", CreationCollisionOption.ReplaceExisting);

            //Reading the files in the source folder
            IReadOnlyList <StorageFile> videos = await sourceFolder.GetFilesAsync();

            //Copying the videos
            foreach (StorageFile video in videos)
            {
                await video.CopyAsync(destinationFolder);
            }
        }
            
    }
}
