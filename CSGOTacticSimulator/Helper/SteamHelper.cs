using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing.Imaging;
using CSGOTacticSimulator.Controls;

namespace CSGOTacticSimulator.Helper
{
    public static class SteamHelper
    {
        public static bool InitSteamClient()
        {
            try
            {
                SteamClient.Init(730);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async void GetAvatarAsync(ulong steamId, TSImage tSImage)
        {
            SteamId id = new SteamId();
            id.Value = steamId;

            Steamworks.Data.Image? img = await SteamFriends.GetLargeAvatarAsync(id);
            if (img.HasValue)
            {
                tSImage.Source = GetTextureFromImage(img.Value);
            }

            return;
        }

        private static BitmapImage GetTextureFromImage(Steamworks.Data.Image image)
        {
            System.Drawing.Bitmap texture = new System.Drawing.Bitmap((int)image.Width, (int)image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var p = image.GetPixel(x, y);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(p.a, p.r, p.g, p.b);
                    texture.SetPixel(x, y, color);
                }
            }
            return BitmapToBitmapImage(texture);
        }

        private static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
    }
}
