using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Ionic.Zip;

namespace IridiumFallInstaller
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region "Variables"
		const string ZIP_URI = "https://dl.iridiumfall.net/minecraft_install.zip";
		string mstrInstallPath = String.Empty;
		private static Thread tPatcher;

		//Private BAD_PROCESSES As String() = {"javaw.exe", "jcef_helper.exe.exe"}
		//Private BAD_FOLDERS As String() = {"Flan", "mods"}
		#endregion

		#region "Methods"
		public MainWindow()
		{
			InitializeComponent();

			//Set default directory
			mstrInstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\";
			lblDirectory.Content = mstrInstallPath;
		}

		private void ChangeDirectory(object sender, InputEventArgs e)
		{
			//TODO - Build my own FilePicker
			if (tPatcher == null || !tPatcher.IsAlive)
			{
				MessageBox.Show("TODO");
			}
		}

		public void StartPatching()
		{
			try
			{
				String strTempPath = Path.GetTempFileName();

				//Step 1 of 3 Downloading Files - Progress goes from 0 to 100
				Invoke(delegate () { lblStatus.Content = "Step 1 of 3 - Downloading files"; });

				HttpWebRequest oFileRequest = (HttpWebRequest)WebRequest.Create(ZIP_URI);

				using (HttpWebResponse oFileResponse = (HttpWebResponse)oFileRequest.GetResponse())
				{
					long nFileSize = oFileResponse.ContentLength;
					long nDownloadedSize = 0;

					using (Stream oRemoteStream = oFileResponse.GetResponseStream())
					{
						using (Stream oLocalStream = File.Create(strTempPath))
						{
							int nByteSize = 0;
							byte[] buffer = new byte[1024];

							while (oRemoteStream.CanRead)
							{
								nByteSize = oRemoteStream.Read(buffer, 0, buffer.Length);

								if (nByteSize == 0) break;

								nDownloadedSize += nByteSize;

								oLocalStream.Write(buffer, 0, nByteSize);

								Invoke(delegate () { SetProgress((int)(nDownloadedSize * 100 / nFileSize)); });
							}
						}
					}
				}

				//Step 2 of 3 Unzipping Files - Progress goes from 0 to 100
				Invoke(delegate () { lblStatus.Content = "Step 2 of 3 - Unzipping files"; });
				
				using (ZipFile zip = ZipFile.Read(strTempPath))
				{
					zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
					zip.ExtractAll(mstrInstallPath, ExtractExistingFileAction.OverwriteSilently);
				}

				//Step 3 of 3 Deleteing unnecessary files - Progress goes from 0 to 100
				Invoke(delegate () { lblStatus.Content = "Step 2 of 3 - Deleting unnecessary files"; });
				File.Delete(strTempPath);

				Invoke(delegate () { lblStatus.Content = "Finished"; });
			}
			catch (Exception ex)
			{
				if (!(ex is ThreadAbortException))
				{
					MessageBox.Show(String.Format("{0}\n\n{1}", ex.Message, (ex.InnerException != null) ? ex.InnerException.Message : ""));
				}
			}
			finally
			{
				Invoke(delegate () { btnPatchIt.Content = "Start patching"; });
			}
		}
				
		public void Invoke(Action action)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(action);
			}
			else
			{
				action();
			}
		}

		public void SetProgress(int nValue)
		{
			barProgress.Value = nValue;
			lblProgress.Content = barProgress.Value + " %";
		}
		#endregion
	
		#region "Handlers
		private void Title_LeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			if(tPatcher != null && tPatcher.IsAlive) {
				//TODO - Build own MsgBox
				//TODO - Abfragen ob er denn beenden darf wenn ja thread beenden
				return;
			}

			this.Close();
		}
		
		private void btnPatchIt_Click(object sender, RoutedEventArgs e)
		{
			if (tPatcher == null || !tPatcher.IsAlive)
			{
				tPatcher = new Thread(new ThreadStart(StartPatching));
				tPatcher.Start();

				btnPatchIt.Content = "Stop Patching";
			}
			else
			{
				tPatcher.Abort();

				btnPatchIt.Content = "Start patching";
				barProgress.Value = 0;
			}
		}

		void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
		{
			if (e.TotalBytesToTransfer > 0)
			{
				Invoke(delegate () { SetProgress(Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer)); });
			}
		}
		#endregion
	}
}
