using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.IO;
using CheckListManager.Instances.Logic.Exceptions;


namespace CheckListManager.Instances.Logic.ImageUtilities
{
	class ImageAdapter
	{
		public static byte[] ScaleImage(byte[] imageBytes, int maxWidth, int maxHeight)
		{
			try
			{
				int newWidth;
				int newHeight;
				byte[] returnArray;
				Image image;

				//Get image from bytes.
				image = (Bitmap)(new ImageConverter().ConvertFrom(imageBytes));

				//Check EXIF Orientation and fix it
				foreach (var prop in image.PropertyItems)
				{
					if (prop.Id == 0x0112)
					{
						int rotateValue = image.GetPropertyItem(prop.Id).Value[0];
						RotateFlipType flipType = GetRotateFlipType(rotateValue);
						image.RotateFlip(flipType);
						break;
					}
				}

				if (image.Width > maxWidth || image.Height > maxHeight)
				{
					var ratioX = (double)maxWidth / image.Width;
					var ratioY = (double)maxHeight / image.Height;
					var ratio = Math.Min(ratioX, ratioY);

					newWidth = (int)(image.Width * ratio);
					newHeight = (int)(image.Height * ratio);
				}
				else
				{
					newWidth = image.Width;
					newHeight = image.Height;
				}
				var newImage = new Bitmap(newWidth, newHeight);

				//set the new resolution, 72 is usually good enough for displaying images on monitors
				newImage.SetResolution(72, 72);
				//or use the original resolution
				//newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

				//drawn the new resized image
				using (var graphics = Graphics.FromImage(newImage))
				{
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

					graphics.DrawImage(image, 0, 0, newWidth, newHeight);
				}
				image = newImage;

				using (MemoryStream ms = new MemoryStream())
				{
					EncoderParameters encoderParameters = new EncoderParameters(1);
					encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
					image.Save(ms, GetEncoderInfo("image/jpeg"), encoderParameters);

					returnArray = ms.ToArray();
				}

				image.Dispose();
				newImage.Dispose();

				return returnArray;
			}
			catch
			{
				throw new InstanceException("Error in image processing");
			}
		}

		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			for (int j = 0; j < encoders.Length; ++j)
			{
				if (encoders[j].MimeType.ToLower() == mimeType.ToLower())
					return encoders[j];
			}
			return null;
		}

		//Get properly rotation to fix image rotation due to EXIF Metatag
		private static RotateFlipType GetRotateFlipType(int rotateValue)
		{
			RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone;

			switch (rotateValue)
			{
				case 1:
					flipType = RotateFlipType.RotateNoneFlipNone;
					break;
				case 2:
					flipType = RotateFlipType.RotateNoneFlipX;
					break;
				case 3:
					flipType = RotateFlipType.Rotate180FlipNone;
					break;
				case 4:
					flipType = RotateFlipType.Rotate180FlipX;
					break;
				case 5:
					flipType = RotateFlipType.Rotate90FlipX;
					break;
				case 6:
					flipType = RotateFlipType.Rotate90FlipNone;
					break;
				case 7:
					flipType = RotateFlipType.Rotate270FlipX;
					break;
				case 8:
					flipType = RotateFlipType.Rotate270FlipNone;
					break;
				default:
					flipType = RotateFlipType.RotateNoneFlipNone;
					break;
			}

			return flipType;
		}
	}
}
