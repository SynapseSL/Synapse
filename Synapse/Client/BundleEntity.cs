using System;
using System.IO;
using System.Net;
using System.Reflection;
using Synapse;

namespace UnityEngine
{
    public abstract class BundleEntity
    {
        public AssetBundle bundle;

        public void LoadBundle()
        {
            if (bundle != null)
            {
                Synapse.Api.Logger.Get.Error("Bundle already loaded");
                return;
            }
            var descriptor = GetType().GetCustomAttribute(typeof(BundleDescriptor)) as BundleDescriptor;
            var loc = Path.Combine(Server.Get.Files.BundleDirectory, descriptor.Bundle);
            Synapse.Api.Logger.Get.Info(loc);
            if (!File.Exists(loc) && descriptor.Source != null)
            {
                var client = new WebClient();
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                client.DownloadFile("https://github.com/SynapseSL/ClientPackages/raw/main/" + descriptor.Source,loc);
                Synapse.Api.Logger.Get.Info($"Downloaded bundle via GitHub {descriptor.Source}");
            }
            else if (!File.Exists(loc))
            {
                Synapse.Api.Logger.Get.Error($"Bundle {descriptor.Bundle} not found!");
                return;
            }
            var stream = File.OpenRead(loc);
            bundle = AssetBundle.LoadFromStream(stream);
        }

        public void LoadPrefabs()
        {
            foreach (var field in GetType().GetFields())
            {
                if (field.GetCustomAttribute(typeof(AssetDescriptor)) is not AssetDescriptor ad) continue;
                field.SetValue(this, bundle.LoadAsset<GameObject>(ad.Asset));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BundleDescriptor : Attribute
    {
        public string Bundle { get; set; }
        public string Source { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetDescriptor : Attribute
    {
        public string Asset { get; set; }
    }
}