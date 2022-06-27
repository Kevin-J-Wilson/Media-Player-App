/////////test test test
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace WpfTutorialSamples.Audio_and_Video
{
	public partial class MediaPlayerWPF : Window {
		private bool mediaPlayerIsPlaying = false;
		private bool userIsDraggingSlider = false;
		decimal mediaSpeed = 1;
		public MediaPlayerWPF() {
			InitializeComponent();

			//START TIMER ON MEDIA
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(0);
			timer.Tick += timer_Tick;
			timer.Start();
		}
		BitmapMaker _loadedImage;
		string _ImageFilePath;
		string _saveFile;
		int _height;
		int _width;
		string _idHeader;
		byte[] _p6Array; 
		string[] _p3Array;
		int _index = 0;
		int _pixelBytes;

		//PROGRESS BAR/TIMER
		private void timer_Tick(object sender, EventArgs eventArg) {
			if((mediaBox.Source != null) && (mediaBox.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
			{
				sliProgress.Minimum = 0;
				sliProgress.Maximum = mediaBox.NaturalDuration.TimeSpan.TotalSeconds;
				sliProgress.Value = mediaBox.Position.TotalSeconds;
			}
		}
		private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> evntArg) {
			lblProgressBar.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
		}
		private void sliProgress_DragStarted(object sender, DragStartedEventArgs eventArg) {
			userIsDraggingSlider = true;
		}
		private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs eventArg) {
			userIsDraggingSlider = false;
			mediaBox.Position = TimeSpan.FromSeconds(sliProgress.Value);
		}

		//OPEN MEDIA
		private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg) {
			eventArg.CanExecute = true;
		}
		private void Open_Executed(object sender, ExecutedRoutedEventArgs e) {
			bool fileSelected = false;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Media files (All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
				
				//add file name to open ppms
			_ImageFilePath = openFileDialog.FileName;

			//Check to see if the user selected a file
			if (openFileDialog.FileName != "")
			{
				fileSelected = true;
			}


			if (fileSelected == true && _ImageFilePath.Contains(".ppm")){
				imgImageBox.Visibility = Visibility.Visible;
				//read all line of file and store to a string array
				_p3Array = File.ReadAllLines(_ImageFilePath);

				ReadHeaders();

				if (_idHeader == "P3") {//for a P3 file

					SetP3Pixels();

					LoadImage();

				}//end if
				if (_idHeader == "P6") {//for a P6 file
					//read all bytes from the file and store to a byte array
					_p6Array = File.ReadAllBytes(_ImageFilePath);

					SetP6Pixels();

					LoadImage();

				}//end if
			} else {
				if (openFileDialog.FileName.Contains(".png")
					|| openFileDialog.FileName.Contains(".PNG")
					|| openFileDialog.FileName.Contains(".jpeg")
					|| openFileDialog.FileName.Contains(".JPEG")
					|| openFileDialog.FileName.Contains(".JPG")
					|| openFileDialog.FileName.Contains(".jpg")) {
					BitmapImage openedImage = new BitmapImage(new Uri(openFileDialog.FileName));
					imgImageBox.Visibility = Visibility.Visible;
					imgImageBox.Source = openedImage;
				} else if (openFileDialog.FileName.Contains(".wmv")
					|| openFileDialog.FileName.Contains(".WMV")
					|| openFileDialog.FileName.Contains(".GIF")
					|| openFileDialog.FileName.Contains(".gif")
					|| openFileDialog.FileName.Contains(".mp4")
					|| openFileDialog.FileName.Contains(".MP4")) {
					mediaBox.Source = new Uri(openFileDialog.FileName);
					imgImageBox.Visibility = Visibility.Collapsed; //THIS IS WHERE THE IMAGE BOX NEEDS TO MINIMIZE TO DO THE Picture in Picture 
				} else if (openFileDialog.FileName.Contains(".mp3")
					|| openFileDialog.FileName.Contains(".MP3")
					|| openFileDialog.FileName.Contains(".wav")
					|| openFileDialog.FileName.Contains(".WAV")){ 				 
					 mediaBox.Source = new Uri(openFileDialog.FileName);
				}else if(openFileDialog.FileName.Contains(".ogg")){
					string message = "Unfortuntely at this time, this application does not support .ogg audio files.\n\n" +
						"Please feel free to convert your .ogg files to one of our supported types, ex.. .mp3 .wav.\n\n" +
						"We aplolgize for the inconvience!";
					string header = "Error!";
					MessageBox.Show(message , header);
					
                }
				else {
					imgImageBox.Visibility = Visibility.Visible;
				}
			}//end if			

		}//end event


		//Help Function
		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
			string content = "This is a media player app.\n\n * To load a file, like a song or video." +
			"Click on the far left image it should open up your file directory, from there select the file you want to load/play.\n\n" +
			"* You can also adjust the speed and volume of the media playing.\n\n" +
			"* Unfortunately this application does not support .ogg files at this time. Please feel free to convert it one of our supported file types.";

			string header = "Instructions";

			MessageBox.Show(content, header);
		}

		//PLAY 
		private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg) {
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null);
		}
		private void Play_Executed(object sender, ExecutedRoutedEventArgs eventArg) {
			mediaBox.Play();
			mediaPlayerIsPlaying = true;
		}

		//PAUSE
		private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg) {
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null);
		}
		private void Pause_Executed(object sender, ExecutedRoutedEventArgs eventArg) {
			mediaBox.Pause();
		}

		//STOP
		private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg) {
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null);
		}
		private void Stop_Executed(object sender, ExecutedRoutedEventArgs eventArg) {
			mediaBox.Stop();
			mediaPlayerIsPlaying = false;
		}

		//REPEAT FUNCTION	
		private void btnRepeat_Click(object sender, RoutedEventArgs e)
		{
			//Reset the progress postion back to zero
			mediaBox.Position = TimeSpan.FromSeconds(00);

			//Set speed ratio back to 1
			mediaBox.SpeedRatio = 1.0;

			//Update speed variable
			mediaSpeed = (decimal)1.0;

			//Display new speed in speed label
			lblCurrentSpeed.Content = mediaSpeed;
		}//end repeat function
		

		//VOLUME
		private void Grid_MouseWheel(object sender, MouseWheelEventArgs eventArg) {
			mediaBox.Volume += (eventArg.Delta > 0) ? 0.05 : -0.05;
		}
		
		//Volume Down
		private void DecreaseVolume_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg) {
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null && mediaBox.Volume > 0);
		}
		private void DecreaseVolume_Executed(object sender, ExecutedRoutedEventArgs eventArg) {
			mediaBox.Volume -= .05;
			mediaPlayerIsPlaying = false;
		}
		
		//VOLUME UP
		private void IncreaseVolume_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg) {
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null && mediaBox.Volume < 1);
		}
		private void IncreaseVolume_Executed(object sender, ExecutedRoutedEventArgs eventArg) {
			mediaBox.Volume += .05;
			mediaPlayerIsPlaying = false;
		}


		//SPEED CONTROL

		private void FastForward_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg)
		{
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null && mediaSpeed < (decimal)2.5);
		}
		private void FastForward_Executed(object sender, ExecutedRoutedEventArgs eventArg)
		{
			mediaBox.SpeedRatio += 0.10;
			mediaSpeed += (decimal)0.10;
			lblCurrentSpeed.Content = mediaSpeed;
			
		}
		private void Rewind_CanExecute(object sender, CanExecuteRoutedEventArgs eventArg)
		{
			eventArg.CanExecute = (mediaBox != null) && (mediaBox.Source != null && mediaSpeed > (decimal)0.10);
			
		}
		private void Rewind_Executed(object sender, ExecutedRoutedEventArgs eventArg)
		{	
			mediaBox.SpeedRatio -= 0.10;
			mediaSpeed -= (decimal)0.10;
			if (mediaBox.SpeedRatio > 0.10)
            {
				lblCurrentSpeed.Content = mediaSpeed;
			}		
			
		}

		//IMAGE BOX DISPLAY CONTROL
		private void btnMinimize_Click(object sender, RoutedEventArgs e)
		{
			imgImageBox.VerticalAlignment = VerticalAlignment.Bottom;
			imgImageBox.HorizontalAlignment = HorizontalAlignment.Right;
			imgImageBox.Height = 100;
			imgImageBox.Width = 100;
		}
		private void btnMaximize_Click(object sender, RoutedEventArgs e)
		{

			imgImageBox.HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Top;
			imgImageBox.Height = mediaBox.Height;
			imgImageBox.Width = mediaBox.Width;
		}

		private void LoadImage() {
			//convert pixel data into writable bitmap
			WriteableBitmap wbmImage = _loadedImage.MakeBitmap();

			//set nearest neighbor rendering mode for image box
			RenderOptions.SetBitmapScalingMode(imgImageBox, BitmapScalingMode.NearestNeighbor);

			//set uniform stretching to scale image cleanly
			imgImageBox.Stretch = Stretch.Uniform;

			//set source for image box to the bitmap
			imgImageBox.Source = wbmImage;

		}//end LoadImage
		private void SetP6Pixels() {
			int index = _p6Array.Length - _pixelBytes;
			//set pixel data for the image
			for (int y = 0; y < _height; y++) {
				for (int x = 0; x < _width; x++) {
					_loadedImage.SetPixel(x, y, _p6Array[index], _p6Array[index + 1], _p6Array[index + 2]);
					index += 3;
				}//end for
			}//end for
		}//end SetP6Pixels
		private void SetP3Pixels() {
			_index = _p3Array.Length - _pixelBytes;
			//set pixel data for the image
			for (int y = 0; y < _height; y++) {
				for (int x = 0; x < _width; x++) {
					_loadedImage.SetPixel(x, y, byte.Parse(_p3Array[_index]), byte.Parse(_p3Array[_index + 1]), byte.Parse(_p3Array[_index + 2]));
					_index += 3;
				}//end for
			}//end for
		}//end SetP3Pixels
		private void ReadHeaders() {

			_index = 0;
			//read first line of file to get the type
			_idHeader = _p3Array[_index];
			_index += 1;

			//store second line of file which should be a comment
			string record = _p3Array[_index];

			while (record[0] == '#') {//read throught all the comments
				record = _p3Array[_index];
				_index += 1;
			}//end while

			//split the next line to get the width and height of the image
			string[] fields = record.Split(' ');
			_width = int.Parse(fields[0]);
			_height = int.Parse(fields[1]);

			//get number of bytes in the pixels minus the alpha value from each pixel
			_pixelBytes = _height * _width * 3;

			//skip next line which is the max rgb channel
			record = _p3Array[_index];
			_index += 1;

			//set up a new bitmap maker with the width and height read from the file
			_loadedImage = new BitmapMaker(_width, _height);
		}//end ReadHeaders

    }//END CLASS
}//END NAMESPACE