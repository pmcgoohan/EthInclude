using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EthInclude
{
    public class Flashbots
    {
        const string FlashbotsUri = @"https://blocks.flashbots.net/v1/blocks?limit=3"; // return the latest 3 blocks to make up for any we missed

        public static async Task Collect(int preDelayMs)
        {
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(preDelayMs);
                FB.Root fb = Flashbots.Get();
                DB.WriteFlashbots(fb);
                return;
            });
        }

        public static FB.Root Get()
        {
            FB.Root root = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(FlashbotsUri);
                using (WebResponse resp = req.GetResponse())
                {
                    StreamReader sr = new StreamReader(resp.GetResponseStream());
                    string json = sr.ReadToEnd();
                    root = JsonConvert.DeserializeObject<FB.Root>(json);
                    resp.Close(); // try everything to close the connection so source ip works
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error Flashbots.Get() " + e.ToString());
            }
            return root;
        }
    }
}