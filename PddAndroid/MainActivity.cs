using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using PddApp;

namespace PddAndroid
{
	[Activity(MainLauncher = true
	          , Theme = "@android:style/Theme.NoTitleBar"
	          , Icon = "@drawable/icon")]
	public class HomeScreen : ListActivity
	{
		LearningQueue _queue = new LearningQueue();
		PddQuestion[][] _data;

		PddQuestion _currentItem;

		PddQuestion CurrentItem
		{
			get 
			{
				return _currentItem;
			}
			set
			{
//				labelQuestion.Text = (value.TicketNumber + 1) + "-" + (value.QuestionNumber + 1) + ". " + value.Question;
				
				var textViewQuestion = FindViewById<TextView> (Resource.Id.textViewQuestion);
				textViewQuestion.Text = (value.TicketNumber + 1) + "-" + (value.QuestionNumber + 1) + ". " + value.Question;

//				listBoxChoices.Items.Clear();
				//listBoxChoices.Items.Add(Enumerable.Range(0, 300).Select(i => " " + i.ToString("D3")).Aggregate((s, next) => s += next));
//				listBoxChoices.Items.AddRange(value.Choices);
//				ListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, value.Choices);
				ListAdapter = new ArrayAdapter<String>(this, Resource.Layout.ChoiceTextItem, value.Choices);


//				labelImage.Image = Image.FromFile(value.ImagePath);
				{
					var imageView1 = FindViewById<ImageView> (Resource.Id.imageView1);
				
					var imageName = value.ImagePath.Split ('\\') [1].Split ('.') [0];
					var imageId = Resources.GetIdentifier (imageName, "drawable", PackageName);
					imageView1.SetImageResource (imageId);
				}

				_currentItem = value;
			}

		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
//
//			RequestWindowFeature (WindowFeatures.Progress);
//			RequestWindowFeature (WindowFeatures.IndeterminateProgress);
//
//
//			SetProgressBarVisibility (true);
//			SetProgressBarIndeterminateVisibility (true);
//
//			SetProgress (5000);

			View header = LayoutInflater.Inflate(Resource.Layout.Header, null);
			ListView.AddHeaderView(header);

			View footer = LayoutInflater.Inflate(Resource.Layout.Footer, null);
			ListView.AddFooterView (footer);


			var buttonDontKnow = FindViewById<Button> (Resource.Id.buttonDontKnow);
			buttonDontKnow.Click += (sender, e) =>
			{
				Answer("");
			};
//					buttonDontKnow
			_data = LoadQuestions ();
//			LoadProgress ();

			NextQuestion ();
		}

		public PddQuestion[][] LoadQuestions()
		{
			PddQuestion[][] questions = null;
			
			using (var stream = Assets.Open("Questions.xml"))
			{
				var ser = new XmlSerializer(typeof(PddQuestion[][]));
				questions = (PddQuestion[][])ser.Deserialize(stream);
			}

			return questions;
		}
		
		void NextQuestion()
		{
			var item = _queue.GetQuestion();

			CurrentItem = _data[item.Ticket][item.Question];

//			statusLabelProgress.Text = _queue.PercentProgress + "%, " + _queue.TimeToFinish;
			{
				var progressBar = FindViewById<ProgressBar> (Resource.Id.progressBar);

				var p = _queue.PercentProgress;

				progressBar.Progress = (int)Math.Floor(p);
				progressBar.SecondaryProgress = (int) (100 * (p - Math.Floor (p)));

			}

			{
				var textViewProgress = FindViewById<TextView> (Resource.Id.textViewProgress);
				textViewProgress.Text = _queue.PercentProgress + "%, " + _queue.TimeToFinish.ToString (@"d'd 'h'h 'm'm'");
			}

		}

		void Answer(string choice)
		{
//			webBrowser1.DocumentText = _currentItem.Tip;
			var textViewHint = FindViewById<TextView> (Resource.Id.textViewHint);
			textViewHint.Text = Android.Text.Html.FromHtml(CurrentItem.Tip).ToString();
			var hintColor = Android.Graphics.Color.Green;

			LearningQueue.Answers answer;

			if (choice.Equals ("")) {
				answer = LearningQueue.Answers.FullyDontKnow;

				ShowHint ();

				hintColor = Android.Graphics.Color.Red;
			}
			else if (choice.Equals(_currentItem.CorrectAnswer))
			{
				answer = LearningQueue.Answers.Know;
			}
			else
			{
				answer = LearningQueue.Answers.DontKnow;

				ShowHint();
				hintColor = Android.Graphics.Color.Red;
			}

			textViewHint.SetTextColor(hintColor);

 			_queue.ProvideAnswer(answer);

//			SaveProgress ();

			NextQuestion();
		}

		void ShowHint()
		{
			AlertDialog alertDialog;
			alertDialog = new AlertDialog.Builder(this).Create();
//			alertDialog.SetTitle("Packing List");
			alertDialog.SetMessage(Android.Text.Html.FromHtml(CurrentItem.Tip));
			alertDialog.Show();
		}

		protected override void OnListItemClick(ListView l, View v, int position, long id)
		{
			if (id >= 0 && id < CurrentItem.Choices.Length)
			{
				Answer (CurrentItem.Choices [id]);
			}
		}
	}
}


