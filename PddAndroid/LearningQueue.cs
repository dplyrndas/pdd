using Android.OS;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PddApp
{
    public class LearningQueue
    {
        DateTime _startDate;
        double _startProgress;

        public LearningQueue()
        {
			Load ();
			LoadHistory ();
            

            _startDate = DateTime.Now;
            _startProgress = PercentProgress;
        }

        bool AllLearned
        {
            get
            {
                return _items.All(i => i.IsLearned);
            }
        }

        public PddItem GetQuestion()
        {
            if (!AllLearned)
            {
                for (; _items.First().IsLearned; )
                {
                    var item = _items[0];
                    _items.RemoveAt(0);
                    _items.Add(item);
                }
            }

            return _items.First();
        }

        static int LearningFunction(int factor)
        {
            return (int)Math.Pow(4, factor);
        }

        public enum Answers
        {
            FullyDontKnow, DontKnow, Know, FullyKnow
        }

        public void Repeat()
        {
            _items.ForEach(i => i.LearningFactor = PddItem.MAX_FACTOR - 1);
        }

        public void ProvideAnswer(Answers answer)
        {
            _settings.History.Add(PercentProgress);

            var currentItem = _items[0];


            switch (answer)
            {
                case Answers.FullyDontKnow:
                    currentItem.LearningFactor = 0;
                    break;
                case Answers.DontKnow:
                    currentItem.LearningFactor--;
                    break;
                case Answers.Know:
                    currentItem.LearningFactor++;
                    break;
                case Answers.FullyKnow:
                    currentItem.LearningFactor += 3;
                    break;
            }

            _items.Remove(currentItem);

            var newPosition = currentItem.IsLearned ? _items.Count : LearningFunction(currentItem.LearningFactor);

            _items.Insert(newPosition, currentItem);


            Save();
        }

        public double PercentProgress
        {
            get
            {
                var maxProgress = _items.Count * PddItem.MAX_FACTOR;
                var currentProgress = _items.Sum(i => i.LearningFactor);

                return currentProgress * 100.0 / maxProgress;
            }
        }

        public TimeSpan TimeToFinish
        {
            get
            {
                var elapsedSeconds = (DateTime.Now - _startDate).TotalSeconds;
                var gainedProgress = PercentProgress - _startProgress;
                var remainingProgress = 100.0 - PercentProgress;

                var estimatedSeconds = remainingProgress / gainedProgress * elapsedSeconds;

                if (!double.IsInfinity(estimatedSeconds))
                    return TimeSpan.FromSeconds(estimatedSeconds);

                return TimeSpan.MaxValue;
            }
        }

        void Save()
        {
			try{
//	            using (var stream = new StreamWriter(_progressFile))
//	            {
//	                var ser = new XmlSerializer(typeof(List<PddItem>));
//	                ser.Serialize(stream, _items);
//	            }
				
				bool mExternalStorageAvailable = false;
				bool mExternalStorageWriteable = false;
				String state = Android.OS.Environment.ExternalStorageState;

				if (Android.OS.Environment.MediaMounted == state) {
					// We can read and write the media
					mExternalStorageAvailable = mExternalStorageWriteable = true;
				} else if (Android.OS.Environment.MediaMountedReadOnly == state) {
					// We can only read the media
					mExternalStorageAvailable = true;
					mExternalStorageWriteable = false;
				} else {
					// Something else is wrong. It may be one of many other states, but all we need
					//  to know is we can neither read nor write
					mExternalStorageAvailable = mExternalStorageWriteable = false;
				}

				string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
				string filePath = Path.Combine(path, "Progress.xml");
				using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
					using (var stream = new StreamWriter(file))
					{
						var ser = new XmlSerializer(typeof(List<PddItem>));
						ser.Serialize(stream, _items);

						stream.Close();
					}
			}
			catch (Exception e) {
			}

            SaveHistory();
        }

        void Load()
        {
			try
			{
//	            using (var stream = new StreamReader(_progressFile))
//	            {
//	                var ser = new XmlSerializer(typeof(List<PddItem>));
//	                _items = (List<PddItem>)ser.Deserialize(stream);
//	            }

				
				bool mExternalStorageAvailable = false;
				bool mExternalStorageWriteable = false;
				String state = Android.OS.Environment.ExternalStorageState;

				if (Android.OS.Environment.MediaMounted == state) {
					// We can read and write the media
					mExternalStorageAvailable = mExternalStorageWriteable = true;
				} else if (Android.OS.Environment.MediaMountedReadOnly == state) {
					// We can only read the media
					mExternalStorageAvailable = true;
					mExternalStorageWriteable = false;
				} else {
					// Something else is wrong. It may be one of many other states, but all we need
					//  to know is we can neither read nor write
					mExternalStorageAvailable = mExternalStorageWriteable = false;
				}
			
				string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
				string filePath = Path.Combine(path, "Progress.xml");
//				using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
					using (var stream = new StreamReader(filePath))
					{
						var ser = new XmlSerializer(typeof(List<PddItem>));
						_items = (List<PddItem>)ser.Deserialize(stream);

						stream.Close();
					}
			}
			catch{
			}

			if (_items == null)
			{
				_items = PddItem.DefaultValues();
//				Repeat();
			}
        }

        void SaveHistory()
        {
			try
			{
	            using (var stream = new StreamWriter(_historyFile))
	            {
	                var ser = new XmlSerializer(typeof(Settings));
	                ser.Serialize(stream, _settings);
	            }
			}
			catch {
			}
        }

        void LoadHistory()
        {
            try
            {
                using (var stream = new StreamReader(_historyFile))
                {
                    var ser = new XmlSerializer(typeof(Settings));
                    _settings = (Settings)ser.Deserialize(stream);
                }
            }
            catch
            {
                
            }

            if(_settings == null)
                _settings = new Settings { History = new List<double>() };
        }

        public class Settings
        {
            public List<double> History { get; set; }
            public List<PddItem> Items { get; set; }
        }


        public List<PddItem> _items;
        Settings _settings;

        string _progressFile = "Egorka.xml";
        string _historyFile = "Egorka_history.xml";
    }

    public class PddItem
    {
        public const int MAX_FACTOR = 4;
        public int Ticket { get; set; }
        public int Question { get; set; }

        int _factor;
        public int LearningFactor
        {
            get
            {
                return _factor;
            }
            set
            {
                if (value < 0)
                    _factor = 0;
                else if (value <= MAX_FACTOR)
                    _factor = value;
                else
                    _factor = MAX_FACTOR;
            }
        }

        public bool IsLearned
        {
            get
            {
                return _factor >= MAX_FACTOR;
            }
        }

        public static List<PddItem> DefaultValues()
        {
            List<PddItem> results = new List<PddItem>(800);

            for (int t = 0; t < 40; ++t)
            {
                for (int q = 0; q < 20; ++q)
                {
                    results.Add(new PddItem { Ticket = t, Question = q });
                }
            }

            return results;
        }

        public override string ToString()
        {
            return "T=" + Ticket + ", Q=" + Question + ", F=" + LearningFactor;
        }
    }
}
