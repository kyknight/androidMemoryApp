using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Views;
using Java.Lang;

namespace androidMyMemoryApp
{
	[Activity(Label = "My Memory App", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity, View.IOnTouchListener
	{

		#region vars
		Button resetFourButton;
		Button resetSixButton;
		LinearLayout gameViewLinearLayout;
		List<System.String> imgsArr = new List<System.String> ( );


		int tileWidth;
		GridLayout gameGridLayout;
		int gridSize = 4;
		int screenWidth;
		List<ImageView> tilesArr = new List<ImageView>();
		List<Point> coordsArr = new List<Point>();


		int curTime = 0;
		TextView timerTextView;


		bool compareState = false;
		ImageView imgViewOne;
		ImageView imgViewTwo;
		bool touchAllowed = true;


		#endregion




		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			for (int i = 1; i <= 18; i++)
			{
				imgsArr.Add("img_" + i.ToString());
			}


			gameViewLinearLayout = FindViewById<LinearLayout>(Resource.Id.gameLinearLayoutId);

			resetFourButton = FindViewById<Button>(Resource.Id.resetFourButtonId);
			resetSixButton = FindViewById<Button>(Resource.Id.resetSixButtonId);

			resetFourButton.Click += ResetFourButton_Click;
			resetSixButton.Click += ResetSixButton_Click;


			// reset at the beginning
			gridSize = 4;
			resetMethod();

			//start time count method
			timerTextView = FindViewById<TextView>(Resource.Id.timerTextViewId);
			timeUpdate();
		}


		private void setGameView()
		{
			// // /// SECTION: Make Grid Layout
			gameGridLayout = new GridLayout(this);
			gameGridLayout.ColumnCount = gridSize;
			gameGridLayout.RowCount = gridSize;

			screenWidth = Resources.DisplayMetrics.WidthPixels;
			LinearLayout.LayoutParams gameGridLayoutParams = new LinearLayout.LayoutParams(screenWidth, screenWidth);

			gameGridLayout.LayoutParameters = gameGridLayoutParams;
			gameGridLayout.SetBackgroundColor(Color.SkyBlue);
			gameViewLinearLayout.AddView(gameGridLayout);
			// // // END SECTION: Make Grid Layout

		}


		private void tileMakerMethod()
		{
			// know the grid size ... gridSize
			// know the tile width and height
			//know where to place tile in grid layout
			//know which image to display on tiles

			tileWidth = screenWidth / gridSize;
			System.String imageName;

			int imgNumber = 0;

			for (int h = 0; h < gridSize; h++)
			{
				for (int v = 0; v < gridSize; v++)
				{
					ImageView imgTile = new ImageView(this);

					Point thisCoord = new Point(h, v);
					coordsArr.Add(thisCoord);

					GridLayout.Spec rowSpec = GridLayout.InvokeSpec(thisCoord.X);
					GridLayout.Spec colSpec = GridLayout.InvokeSpec(thisCoord.Y);

					GridLayout.LayoutParams tileParams = new GridLayout.LayoutParams(rowSpec, colSpec);

					tileParams.Width = tileWidth;
					tileParams.Height = tileWidth;

					if (imgNumber == gridSize * gridSize / 2)
						imgNumber = 0;


					imageName = imgsArr[imgNumber] as string;
					int resId = Resources.GetIdentifier(imageName, "drawable", PackageName);
					imgTile.SetImageResource(resId);

					imgTile.LayoutParameters = tileParams;




					imgTile.SetOnTouchListener(this);


					tilesArr.Add(imgTile);	

					gameGridLayout.AddView(imgTile);

					imgNumber++;
				}
			}
			// have an array of all tiles
			//ORDERED from 0 to gridsize X gridSize
			// and have an array of all coords.

		}


		private void randomizeMethod()
		{
			// 16 tiles.
			//16 locations.
			//go through tiles in order
			//assign a random coord to them.

			Random myRandom = new Random();

			foreach (ImageView any in tilesArr)
			{
				int randomIndex = myRandom.Next(0, coordsArr.Count);

				Point newRandCoord = coordsArr[randomIndex];

				GridLayout.Spec randomRowSpec = GridLayout.InvokeSpec(newRandCoord.X);
				GridLayout.Spec randomColSpec = GridLayout.InvokeSpec(newRandCoord.Y);

				GridLayout.LayoutParams randGridParams = new GridLayout.LayoutParams(randomRowSpec, randomColSpec);

				randGridParams.Width = tileWidth;
				randGridParams.Height = tileWidth;

				any.LayoutParameters = randGridParams;


				coordsArr.RemoveAt(randomIndex);
			}

		}

		private void resetMethod()
		{
			curTime = 0;
			compareState = false;
			touchAllowed = true;

			if (tilesArr.Count > 0)
				tilesArr.Clear();

			if (coordsArr.Count > 0)//just making sure the array is clear
				coordsArr.Clear();

			if (gameViewLinearLayout.ChildCount > 0)
				gameViewLinearLayout.RemoveView(gameGridLayout);

			setGameView();
			tileMakerMethod();
			randomizeMethod();

			foreach (ImageView any in tilesArr)
			{
				int noImgResId = Resources.GetIdentifier("no_img", "drawable", PackageName);
				any.SetImageResource(noImgResId);
			}

		}


		private async void timeUpdate()
		{
			while (true)
			{
				await Task.Delay(1000);
				curTime++;

				int timeMinutes = curTime / 60;
				int timeSeconds = curTime % 60;

				timerTextView.Text = timeMinutes.ToString() + "\' : " + timeSeconds.ToString() + "\"";
			}
		}


		void ResetFourButton_Click(object sender, System.EventArgs e)
		{
			gridSize = 4;
			resetMethod();
		}

		void ResetSixButton_Click(object sender, System.EventArgs e)
		{
			gridSize = 6;
			resetMethod();
		}

		public bool OnTouch(View v, MotionEvent e)
		{
			if (tilesArr.Contains((ImageView)v) && e.Action == MotionEventActions.Up && touchAllowed)
			{
				touchAllowed = false;


				ImageView thisTile = (ImageView)v;

				int indexOfTile = tilesArr.IndexOf(thisTile);
				int indexOfImage;

				if (indexOfTile >= gridSize * gridSize / 2)
					indexOfImage = indexOfTile - gridSize * gridSize / 2;
				else
					indexOfImage = indexOfTile;


				System.String nameOfImage = imgsArr[indexOfImage] as string;
				int resourceID = Resources.GetIdentifier(nameOfImage, "drawable", PackageName);
				thisTile.SetImageResource(resourceID);


				if (compareState)
				{
					// second touch here
					imgViewTwo = thisTile;
					int dist = gridSize * gridSize / 2;

					int thisDist = System.Math.Abs(tilesArr.IndexOf(imgViewOne) - tilesArr.IndexOf(imgViewTwo));

					Handler compareHandler = new Handler();
					Action myCompareAction = () =>
					{
						if (thisDist == dist)
						{
							// identical
							imgViewOne.Animate()
									  .SetDuration(500)
									  .Alpha(0)
									  .WithEndAction(new Runnable(() =>
										{
											ImageView foundImgView = new ImageView(this);
											GridLayout.LayoutParams foundParams = (GridLayout.LayoutParams)imgViewOne.LayoutParameters;

											int foundResId = Resources.GetIdentifier("found", "drawable", PackageName);
											foundImgView.SetImageResource(foundResId);

											foundImgView.LayoutParameters = foundParams;
											gameGridLayout.AddView(foundImgView);

											gameGridLayout.RemoveView(imgViewOne);
										}));

							imgViewTwo.Animate()
									  .SetDuration(500)
									  .Alpha(0)
									  .WithEndAction(new Runnable(() =>
										{
											ImageView foundImgView = new ImageView(this);
											GridLayout.LayoutParams foundParams = (GridLayout.LayoutParams)imgViewTwo.LayoutParameters;

											int foundResId = Resources.GetIdentifier("found", "drawable", PackageName);
											foundImgView.SetImageResource(foundResId);

											foundImgView.LayoutParameters = foundParams;
											gameGridLayout.AddView(foundImgView);

											gameGridLayout.RemoveView(imgViewTwo);

											touchAllowed = true;
										}));

							imgViewOne.Alpha = 0;
							imgViewTwo.Alpha = 0;
						}
						else
						{
							//different
							imgViewOne.SetImageResource(Resources.GetIdentifier("no_img", "drawable", PackageName));
							imgViewTwo.SetImageResource(Resources.GetIdentifier("no_img", "drawable", PackageName));

							touchAllowed = true;
						}
					};

					compareHandler.PostDelayed(myCompareAction, 1000);

					compareState = false;

				}
				else
				{
					//first touch here.
					imgViewOne = thisTile;

					compareState = true;
					touchAllowed = true;
				}
			}

			return true;
		}
	}
}

