using ICSharpCode.SharpZipLib.Zip;

namespace GDFrameworkExtend.ZipKit
{
    public class ZipUtility
    {
        public static void ZipFolder(string folderPathToZip, string outputZipFileName)
        {
            var fastZip = new FastZip();
            fastZip.CreateZip(outputZipFileName,
                folderPathToZip, true, string.Empty);
        }
    }
}