using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PiXeL_Apps.Common
{
    static class CopyFiles
    {
        /// <summary>
        /// Copy videos to USB device
        /// </summary>
        /// <returns></returns>
        public async static Task copyVideosToUSB()
        {
            //Deleting the old photos and videos
            //await deleteOldMedia();

            //Setting the source folder
            StorageFolder sourceFolder;
            sourceFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");

            //Selecting the right USB device
            StorageFolder rightDevice = null;
            var devices = await KnownFolders.RemovableDevices.GetFoldersAsync();
            foreach (var device in devices)
            {
                var deviceHelper = device.DisplayName.ToLower();
                deviceHelper = deviceHelper.Substring(0, deviceHelper.Length - 5);
                if (deviceHelper.Equals("wagenmap"))
                {
                    rightDevice = device;
                }
            }

            //Setting the destination folder
            StorageFolder destinationFolder;
            string carnumber = LocalDB.database.GetAutoId();
            destinationFolder = await rightDevice.CreateFolderAsync(carnumber + @"\Videos", CreationCollisionOption.OpenIfExists);

            //Reading the files in the source folder
            IReadOnlyList<StorageFile> videos = await sourceFolder.GetFilesAsync();

            //Copying the videos
            foreach (StorageFile video in videos)
            {
                await video.CopyAsync(destinationFolder, video.Name, NameCollisionOption.ReplaceExisting);
            }

        }

        /// <summary>
        /// Copy videos via the network to a server
        /// </summary>
        /// <returns></returns>
        public async static Task copyVideosViaNetwork()
        {
            //Deleting the old photos and videos
            //await deleteOldMedia();

            //Setting the source folder
            StorageFolder sourceFolder;
            sourceFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");

            //Setting the destination folder
            StorageFolder destinationFolder;
            destinationFolder = await KnownFolders.VideosLibrary.CreateFolderAsync("Videos", CreationCollisionOption.OpenIfExists);

            //Reading the files in the source folder
            IReadOnlyList<StorageFile> videos = await sourceFolder.GetFilesAsync();

            //Copying the videos
            foreach (StorageFile video in videos)
            {
                await video.CopyAsync(destinationFolder, video.Name, NameCollisionOption.ReplaceExisting);
            }
        }

        /// <summary>
        /// Copy photos to USB device
        /// </summary>
        /// <returns></returns>
        public async static Task copyPhotosToUSB()
        {
            //Deleting the old photos and videos
            //await deleteOldMedia();

            //Setting the source folder
            StorageFolder sourceFolder;
            sourceFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Photos");

            //Selecting the right USB device
            StorageFolder rightDevice = null;
            var devices = await KnownFolders.RemovableDevices.GetFoldersAsync();
            foreach (var device in devices)
            {
                var deviceHelper = device.DisplayName.ToLower();
                deviceHelper = deviceHelper.Substring(0, deviceHelper.Length - 5);
                if (deviceHelper.Equals("wagenmap"))
                {
                    rightDevice = device;
                }
            }

            //Setting the destination folder
            StorageFolder destinationFolder;
            string carnumber = LocalDB.database.GetAutoId();
            destinationFolder = await rightDevice.CreateFolderAsync(carnumber + @"\Photos", CreationCollisionOption.OpenIfExists);

            //Reading the files in the source folder
            IReadOnlyList<StorageFile> photos = await sourceFolder.GetFilesAsync();

            //Copying the photos
            foreach (StorageFile photo in photos)
            {
                await photo.CopyAsync(destinationFolder, photo.Name, NameCollisionOption.ReplaceExisting); 
            }

        }

        /// <summary>
        /// Copy photos via the network to a server
        /// </summary>
        /// <returns></returns>
        public async static Task copyPhotosViaNetwork()
        {
            //Deleting the old photos and videos
            //await deleteOldMedia();

            //Setting the source folder
            StorageFolder sourceFolder;
            sourceFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Photos");

            //Setting the destination folder
            StorageFolder destinationFolder;
            destinationFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Photos", CreationCollisionOption.ReplaceExisting);

            //Reading the files in the source folder
            IReadOnlyList<StorageFile> photos = await sourceFolder.GetFilesAsync();

            //Copying the photos
            foreach (StorageFile photo in photos)
            {
                await photo.CopyAsync(destinationFolder, photo.Name, NameCollisionOption.ReplaceExisting);
            }
        }

        /// <summary>
        /// Deletes old photos and videos to clean up diskspace/less data to synchronize
        /// </summary>
        /// <returns></returns>
       /*public async static Task deleteOldMedia()
        {
            //setting the source folders
            StorageFolder photoFolder;
            StorageFolder videoFolder;
            try
            {
                photoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Photos");
                if (photoFolder != null)
                {
                    //Reading the files in the source folders
                    IReadOnlyList<StorageFile> photos = await photoFolder.GetFilesAsync();
                    //Deleting the old photos
                    foreach (StorageFile photo in photos)
                    {
                        DateTime now = DateTime.Now.AddDays(-1);
                        if (photo.DateCreated < now)
                        {
                            await photo.DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.paLogging.log.Warn(e.Message);
            }

            try
            {
                videoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");
                if (videoFolder != null)
                {
                    //Reading the files in the source folders
                    IReadOnlyList<StorageFile> videos = await videoFolder.GetFilesAsync();
                    //Deleting the old videos
                    foreach (StorageFile video in videos)
                    {

                        if (video.DateCreated < DateTime.Now.AddHours(-24))
                        {
                            await video.DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.paLogging.log.Warn(e.Message);
            }
        }*/
    }
}