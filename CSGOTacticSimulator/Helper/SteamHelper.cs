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
using HltvSharp.Models;
using System.Security.Policy;

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

        public static async void GetAvatarAsync(ulong steamId, string name, TSImage tSImage, bool steamInited, Dictionary<string, string> proAvatarLinkDic)
        {
            SteamId id = new SteamId();
            id.Value = steamId;
            BitmapImage bitmapImage = new BitmapImage();

            if (proAvatarLinkDic.ContainsKey(name))
            {
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(proAvatarLinkDic[name]);
                bitmapImage.EndInit();
            }
            else
            {
                var Search = new HltvSharp.Search();
                List<PlayerSearchItem> res = await Search.Players(name);
                if (res != null && res.Count > 0)
                {
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(res[0].pictureUrl);
                    proAvatarLinkDic[name] = res[0].pictureUrl;
                    bitmapImage.EndInit();
                }
            }

            if (bitmapImage.UriSource == null && steamInited)
            {
                Steamworks.Data.Image? img = await SteamFriends.GetLargeAvatarAsync(id);
                if (img.HasValue)
                {
                    bitmapImage = GetTextureFromImage(img.Value);
                }
            }
            
            if (bitmapImage != null)
            {
                tSImage.Source = bitmapImage;
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
