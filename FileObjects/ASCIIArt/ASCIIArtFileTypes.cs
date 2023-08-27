using AAP.BackgroundTasks;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class TXTASCIIArt : IAAPFile<ASCIIArt>
    {
        public ASCIIArt FileObject { get; set; }
        public string FilePath { get; set; }

        public TXTASCIIArt(ASCIIArt art, string filePath)
        {
            FileObject = art;
            FilePath = filePath;
        }

        public void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Reading text...", true));
            string[] txtLines = File.ReadAllLines(FilePath);

            if (txtLines.Length <= 0)
                throw new Exception($"ASCIIArtFile.ImportFile(path: {FilePath}): txt file contains no lines!");

            int txtWidth = 0;
            int txtHeight = txtLines.Length;

            //Get total width
            foreach (string line in txtLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Creating art...", true));
            FileObject.SetSize(txtWidth, txtHeight);
            FileObject.CreatedInVersion = ASCIIArt.VERSION;
            ArtLayer txtArtLayer = new("Imported Art", FileObject.Width, FileObject.Height);

            for (int y = 0; y < txtHeight; y++)
            {
                bgWorker?.ReportProgress((int)((double)(y + 1)/txtHeight * 100), new BackgroundTaskUpdateArgs("Creating lines...", false));
                char[] chars = txtLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
            }

            FileObject.ArtLayers.Add(txtArtLayer);
        }

        public bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Getting art string...", true));
            string artString = FileObject.GetArtString();

            if (bgWorker != null)
            {
                if (bgWorker.CancellationPending)
                    return false;

                bgWorker.WorkerSupportsCancellation = false;
            }

            bgWorker?.ReportProgress(50, new BackgroundTaskUpdateArgs("Writing to file...", true));
            StreamWriter sw = File.CreateText(FilePath);
            sw.Write(artString);

            sw.Close();

            return true;
        }
    }

    public class AAFASCIIArt : IAAPFile<ASCIIArt>
    {
        //private static readonly string UncompressedExportPath = @$"{App.ApplicationDataFolderPath}\uncompressedArtExport";

        public ASCIIArt FileObject { get; set; }
        public string FilePath { get; set; }

        public AAFASCIIArt(ASCIIArt art, string filePath)
        {
            FileObject = art;
            FilePath = filePath;
        }

        public void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Decompressing art file...", true));
            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                output.CopyTo(fs);

            fs.Close();

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Deserializing decompressed art file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            ASCIIArt? importedArt = js.Deserialize<ASCIIArt>(jr);
            if (importedArt != null)
            {
                FileObject.CreatedInVersion = importedArt.CreatedInVersion;
                FileObject.SetSize(importedArt.Width, importedArt.Height);

                for (int i = 0; i < importedArt.ArtLayers.Count; i++)
                    FileObject.ArtLayers.Insert(i, importedArt.ArtLayers[i]);
            }
            else
                throw new Exception("No art could be imported!");

            jr.CloseInput = true;
            jr.Close();

            bgWorker?.ReportProgress(100, new BackgroundTaskUpdateArgs("Deleting decompressed path...", true));
            File.Delete(tempFilePath);
        }

        public bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            string tempFilePath = Path.GetTempFileName();

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);
            js.Serialize(sw, FileObject);
            sw.Close();

            if (bgWorker != null)
            {
                if (bgWorker.CancellationPending)
                {
                    File.Delete(tempFilePath);
                    return false;
                }

                bgWorker.WorkerSupportsCancellation = false;
            }

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
                using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                    fs.CopyTo(output);

            bgWorker?.ReportProgress(100, new BackgroundTaskUpdateArgs("Deleting uncompressed path", true));
            File.Delete(tempFilePath);

            return true;
        }
    }
}
