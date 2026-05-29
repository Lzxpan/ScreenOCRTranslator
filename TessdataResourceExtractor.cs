using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ScreenOCRTranslator
{
    internal static class TessdataResourceExtractor
    {
        private static readonly string[] TessdataFiles =
        {
            "chi_sim.traineddata",
            "chi_tra.traineddata",
            "eng.traineddata",
            "jpn.traineddata"
        };

        public static string TessdataDirectory { get; private set; }

        public static string EnsureTessdata()
        {
            if (!string.IsNullOrWhiteSpace(TessdataDirectory) && Directory.Exists(TessdataDirectory))
                return TessdataDirectory;

            string baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ScreenOCRTranslator",
                "tessdata");
            Directory.CreateDirectory(baseDir);

            Assembly assembly = typeof(TessdataResourceExtractor).Assembly;
            string[] resources = assembly.GetManifestResourceNames();

            foreach (string fileName in TessdataFiles)
            {
                string targetPath = Path.Combine(baseDir, fileName);
                string resourceName = resources.FirstOrDefault(r =>
                    r.EndsWith(".tessdata." + fileName, StringComparison.OrdinalIgnoreCase) ||
                    r.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrWhiteSpace(resourceName))
                    throw new FileNotFoundException("找不到內嵌 OCR 語言資料：" + fileName);

                using (Stream source = assembly.GetManifestResourceStream(resourceName))
                {
                    if (source == null)
                        throw new FileNotFoundException("無法讀取內嵌 OCR 語言資料：" + fileName);

                    bool shouldWrite = !File.Exists(targetPath) || new FileInfo(targetPath).Length != source.Length;
                    if (!shouldWrite)
                        continue;

                    using (FileStream destination = File.Create(targetPath))
                    {
                        source.CopyTo(destination);
                    }
                }
            }

            TessdataDirectory = baseDir;
            return TessdataDirectory;
        }
    }
}
