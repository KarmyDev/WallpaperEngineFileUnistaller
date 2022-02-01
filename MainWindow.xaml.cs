using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WallpaperEngineFileUnistaller
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string workingDirectoryPath;

		private string workingWallpaperDirectoryPath;

		public MainWindow()
		{
			InitializeComponent();
			WallpaperEngine_WallpaperBrowser.DisplayMemberPath = "Name";
			WallpaperEngine_WallpaperBrowser.SelectionChanged += WallpaperEngine_WallpaperBrowser_SelectionChanged;
		}

		private void WallpaperEngine_WallpaperBrowser_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				WallpaperItem wItem = (WallpaperItem)WallpaperEngine_WallpaperBrowser.SelectedItem;

				if (wItem != null)
				{

					string[] dataFromWallpapaer = Directory.GetFiles(wItem.DirectoryPath);

					foreach (string wallpaperFile in dataFromWallpapaer)
					{
						FileInfo file = new FileInfo(wallpaperFile);

						workingWallpaperDirectoryPath = file.Directory.FullName;

						if (file.Name.Split(".")[0] == "preview")
						{
							if (!string.IsNullOrWhiteSpace(wallpaperFile))
							{
								try
								{
									using (var stream = File.Open(wallpaperFile, FileMode.Open))
									{
										using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream))
										{
											Bitmap bmp = (Bitmap)image;

											using (var memory = new MemoryStream())
											{
												bmp.Save(memory, ImageFormat.Png);
												memory.Position = 0;

												var bitmapImage = new BitmapImage();
												bitmapImage.BeginInit();
												bitmapImage.StreamSource = memory;
												bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
												bitmapImage.EndInit();
												bitmapImage.Freeze();

												WallpaperEngine_WallpaperPreview.Source = bitmapImage;

											}
										}
									}

								}
								catch (Exception) { }

								try
								{
									string fileJson = File.ReadAllText(System.IO.Path.Combine(file.DirectoryName, "project.json"));
									WallpaperEngine_JsonBox.Text = fileJson;
								}
								catch (Exception exception)
								{
									WallpaperEngine_JsonBox.Text = "Couldn't parse this wallpaper project.json\n\nException: " + exception.Message;
								}

								break;
							}
							else
							{
								WallpaperEngine_WallpaperPreview.Source = null;
								try
								{
									SetImageIfError();
								}
								catch (Exception)
								{ }
							}

						}
						else
						{
							WallpaperEngine_WallpaperPreview.Source = null;
							try
							{
								SetImageIfError();
							}
							catch (Exception)
							{ }
						}
					}
				}
			}
			catch (Exception) {}
		}

		private void SetImageIfError()
		{
			WallpaperEngine_WallpaperPreview.Source = GenerateRandomImage();
		}

		private void WallpaperEngine_SelectWorkFolder(object sender, RoutedEventArgs e)
		{
			var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			if (dialog.ShowDialog(this).GetValueOrDefault())
			{
				workingDirectoryPath = dialog.SelectedPath;
				ParseFilesToBrowser();
			}
		}

		private void ParseFilesToBrowser()
		{
			try
			{
				string[] foldersInWorkshopContent = Directory.GetDirectories(System.IO.Path.Combine(workingDirectoryPath, "content"));

				WallpaperEngine_WallpaperBrowser.Items.Clear();

				foreach (string folderFromWorkshopContent in foldersInWorkshopContent)
				{
					string[] foldersInWorkshopContentDirectory = Directory.GetDirectories(folderFromWorkshopContent);

					foreach (string wallpaperFolder in foldersInWorkshopContentDirectory)
					{
						string displayName = new DirectoryInfo(wallpaperFolder).Name;
						try
						{
							string json = File.ReadAllText(System.IO.Path.Combine(wallpaperFolder, "project.json"));
							dynamic data = JsonConvert.DeserializeObject(json);

							displayName += $": {data.title}";
						}
						catch (Exception exception) { displayName += $": \t\t\t\t(File project.json not found)"; }

						WallpaperEngine_WallpaperBrowser.Items.Add(new WallpaperItem(displayName, wallpaperFolder));
					}

				}

			}
			catch (Exception)
			{
			}
		}

		class WallpaperItem 
		{
			public string Name { get; set; }
			public string DirectoryPath { get; set; }

			public WallpaperItem(string name, string path)
			{
				Name = name;
				DirectoryPath = path;
			}
		}

		private void WallpaperEngine_OpenWallpaperInExplorer(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(workingWallpaperDirectoryPath)) Process.Start("explorer.exe", workingWallpaperDirectoryPath);
		}

		private BitmapImage GenerateRandomImage()
		{
			int width = 200, height = 200;

			//bitmap
			Bitmap bmp = new Bitmap(width, height);

			//random number
			Random rand = new Random();

			//create random pixels
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					//generate random ARGB value
					int a = rand.Next(256);
					int r = rand.Next(256);
					int g = rand.Next(256);
					int b = rand.Next(256);

					//set ARGB value
					bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(a, r, g, b));
				}
			}

			using (var memory = new MemoryStream())
			{
				bmp.Save(memory, ImageFormat.Png);
				memory.Position = 0;

				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				bitmapImage.Freeze();

				return bitmapImage;

			}
		}

		private void WallpaperEngine_DeleteWallpaper(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(workingWallpaperDirectoryPath)) 
			{
				DirectoryInfo dir = new DirectoryInfo(workingWallpaperDirectoryPath);
				WallpaperEngine_WallpaperPreview.Source = GenerateRandomImage();
				WallpaperEngine_JsonBox.Text = string.Empty;
				dir.Delete(true);
				ParseFilesToBrowser();
			}
		}
	}
}
