using SevenZip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ImageGlass.ImageListView
{
    public class ArchiveAdaptor : ImageListViewItemAdaptors.FileSystemAdaptor
    {
        private SevenZipExtractor _extr;
        private string _zippath;

        public ArchiveAdaptor(string zippath)
        {
            _zippath = zippath;
        }

        private Image UnpackImage(string key)
        {
            try
            {
                using (SevenZipExtractor extr = new SevenZipExtractor(_zippath))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        extr.ExtractFile(key, mem);
                        return Image.FromStream(mem);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC)
        {
            if (disposed)
                return null;

            Image img = UnpackImage((string)key);
            if (img == null)
                return null;
            Image ret = ThumbnailExtractor.GetThumbnailBmp(img, size, 0);
            img.Dispose();
            return ret;
        }

        public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key, bool useWIC)
        {
            if (disposed)
                return null;

            Image img = UnpackImage((string)key);
            List<Utility.Tuple<ColumnType, string, object>> details = new List<Utility.Tuple<ColumnType, string, object>>();

            // Get file info
            if (img != null)
            {
                //FileInfo info = new FileInfo(filename);
                //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateCreated, string.Empty, info.CreationTime));
                //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateAccessed, string.Empty, info.LastAccessTime));
                //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateModified, string.Empty, info.LastWriteTime));
                //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FileSize, string.Empty, info.Length));
                //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FilePath, string.Empty, info.DirectoryName ?? ""));
                //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FolderName, string.Empty, info.Directory.Name ?? ""));

                // Get metadata
                MetadataExtractor metadata = MetadataExtractor.FromImage(img);
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Dimensions, string.Empty, new Size(metadata.Width, metadata.Height)));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Resolution, string.Empty, new SizeF((float)metadata.DPIX, (float)metadata.DPIY)));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ImageDescription, string.Empty, metadata.ImageDescription ?? ""));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.EquipmentModel, string.Empty, metadata.EquipmentModel ?? ""));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateTaken, string.Empty, metadata.DateTaken));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Artist, string.Empty, metadata.Artist ?? ""));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Copyright, string.Empty, metadata.Copyright ?? ""));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ExposureTime, string.Empty, (float)metadata.ExposureTime));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FNumber, string.Empty, (float)metadata.FNumber));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ISOSpeed, string.Empty, (ushort)metadata.ISOSpeed));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.UserComment, string.Empty, metadata.Comment ?? ""));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Rating, string.Empty, (ushort)metadata.Rating));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Software, string.Empty, metadata.Software ?? ""));
                details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FocalLength, string.Empty, (float)metadata.FocalLength));

                img.Dispose();
            }

            return details.ToArray();
        }

    }


}
