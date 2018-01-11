using System;
using System.IO;
using System.Net;
using System.Reflection;
using Prism.Commands;
using Prism.Mvvm;
using SevenZip;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using ZstdNet;

namespace ZStandardNETSample.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand CompressCommand { get; set; }

        public DelegateCommand DeCompressCommand { get; set; }

        public DelegateCommand SevenzipArchiveCommand { get; set; }

        public DelegateCommand SevenzipDeArchiveCommand { get; set; }

        public MainWindowViewModel()
        {
            CompressCommand = new DelegateCommand(ZStandardCompress);
            DeCompressCommand = new DelegateCommand(ZStandardDecompress);
            SevenzipArchiveCommand = new DelegateCommand(DoSevenZipArchive);
            SevenzipDeArchiveCommand = new DelegateCommand(DoSevenZipDeArchive);
        }

        private void ZStandardCompress()
        {
            var sourceDir = @"D:\share\ZStandard\Compress\hogeOrigin";
            var destDir = @"D:\share\ZStandard\Compress\mogeZst";

            var files = Directory.GetFiles(sourceDir);

            if(files.Length <= 0) return;

            var option = new CompressionOptions(22);

            using (var compressor = new Compressor(option))
            {
                foreach (var file in files)
                {
                    byte[] compressedData = null;

                    var fileName = Path.GetFileName(file);
                    var destPath = Path.Combine(destDir, fileName);
                    if (File.Exists(destPath)) File.Delete(destPath);

                    var sourceData = File.ReadAllBytes(file);
                    compressedData = compressor.Wrap(sourceData);

                    using (var fs = new FileStream(destPath,FileMode.Create,FileAccess.Write))
                    {
                        //バイト型配列の内容をすべて書き込む
                        fs.Write(compressedData, 0, compressedData.Length);
                    }
                }
            }
            option.Dispose();
            option = null;
        }

        private void ZStandardDecompress()
        {
            var sourceDir = @"D:\share\ZStandard\DeCompress\hogeOrigin";
            var destDir = @"D:\share\ZStandard\DeCompress\mogeZst";

            var files = Directory.GetFiles(sourceDir);

            if (files.Length <= 0) return;

            using (var decompressor = new Decompressor())
            {
                foreach (var file in files)
                {
                    byte[] compressedData = null;

                    var fileName = Path.GetFileName(file);
                    var destPath = Path.Combine(destDir, fileName);
                    if (File.Exists(destPath)) File.Delete(destPath);

                    var sourceData = File.ReadAllBytes(file);
                    compressedData = decompressor.Unwrap(sourceData);

                    using (var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                    {
                        //バイト型配列の内容をすべて書き込む
                        fs.Write(compressedData, 0, compressedData.Length);
                    }
                }
            }
        }

        private void DoSevenZipArchive()
        {
            var sourceDir = @"D:\share\ZStandard\SevenZip\target";
            var destDir = @"D:\share\ZStandard\SevenZip\";
            var destFile = Path.Combine(destDir, "hoge.7z");

            var files = Directory.GetFiles(sourceDir);

            var libPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7z.dll");

            SevenZipCompressor.SetLibraryPath(libPath);

            var szc = new SevenZipCompressor();
            szc.CompressionMethod = CompressionMethod.Lzma;
            szc.CompressionMode = CompressionMode.Create;
            szc.CompressionLevel = CompressionLevel.Normal;
            szc.CompressFiles(destFile, files);
        }

        private void DoSevenZipDeArchive()
        {
            var sourceDir = @"D:\share\ZStandard\SevenZip\";
            var sourceFile = Path.Combine(sourceDir, "hoge.7z");
            var extractDir = @"D:\share\ZStandard\SevenZip\extract";

            using (Stream stream = File.OpenRead(sourceFile))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Console.WriteLine(reader.Entry.Key);
                        reader.WriteEntryToDirectory(extractDir, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }
    }
}
