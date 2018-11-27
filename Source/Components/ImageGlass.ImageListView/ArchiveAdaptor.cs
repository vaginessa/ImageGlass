/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017-2019 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

This file created by Kevin Routley.
*/
using SevenZip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ImageGlass.ImageListView
{
    /// <summary>
    /// ImageListView support for thumbnails and metadata from archive files.
    /// </summary>
    public class ArchiveAdaptor : ImageListViewItemAdaptors.FileSystemAdaptor
    {
        private SevenZipExtractor _extr;
        private string _zippath;

        /// <summary>
        /// Create an archive image fetcher.
        /// </summary>
        /// <param name="zippath">file path to archive file</param>
        public ArchiveAdaptor(string zippath)
        {
            _zippath = zippath;
        }

        private ArchiveFileInfo? FetchImageData(string key)
        {
            try
            {
                // TODO KBR 20181127 the extractor appears not to be thread safe?
                using (SevenZipExtractor extr = new SevenZipExtractor(_zippath))
                {
                    var foo = extr.ArchiveFileData;
                    foreach (var afd in foo)
                    {
                        if (afd.FileName == key)
                            return afd;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private Image UnpackImage(string key)
        {
            try
            {
                // TODO KBR 20181127 the extractor appears not to be thread safe?
                using (SevenZipExtractor extr = new SevenZipExtractor(_zippath))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        extr.ExtractFile(key, mem);

                        var ms = new MemoryStream(mem.GetBuffer());  // TODO KBR 20181119 dispose exception if don't copy stream??
                        return Image.FromStream(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract an image (especially thumbnails) from within an archive.
        /// </summary>
        /// <param name="key">the key to find the virtual list view item</param>
        /// <param name="size">target size for the image</param>
        /// <param name="useEmbeddedThumbnails"></param>
        /// <param name="useExifOrientation"></param>
        /// <param name="useWIC">ignored</param>
        /// <returns>the extracted image</returns>
        public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC)
        {
            if (disposed)
                return null;

            using (Image img = UnpackImage((string)key))
            {
                if (img == null)
                    return null;
                Image ret = ThumbnailExtractor.GetThumbnailBmp(img, size, 0);
                return ret;
            }
        }

        /// <summary>
        /// Provide metadata for an image within an archive
        /// </summary>
        /// <param name="key">the key to find the virtual list view item</param>
        /// <param name="useWIC">ignored</param>
        /// <returns></returns>
        public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key, bool useWIC)
        {
            if (disposed)
                return null;

            using (Image img = UnpackImage((string)key))
            {
                List<Utility.Tuple<ColumnType, string, object>> details = new List<Utility.Tuple<ColumnType, string, object>>();

                // Get file info
                if (img != null)
                {
                    // Fetch relevant file data from the archive
                    ArchiveFileInfo? filedata = FetchImageData((string)key);
                    if (filedata.HasValue)
                    {
                        details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateCreated, string.Empty, filedata.Value.CreationTime));
                        details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateAccessed, string.Empty, filedata.Value.LastAccessTime));
                        details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateModified, string.Empty, filedata.Value.LastWriteTime));
                        details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FileSize, string.Empty, filedata.Value.Size));
                    }

                    // Get image metadata
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
                }
                return details.ToArray();
            }
        }

    }


}
