using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FootballClubsServer
{
    [Serializable]
    public class FootballClub
    {
        public static List<FootballClub> clubs;

        private int id;
        private string name;
        private string imagePath;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (File.Exists(value))
                {
                    imagePath = value;
                }
            }
        }

        public FootballClub()
        {
            
        }

        public FootballClub(int id, string name, string imagePath)
        {
            this.Id = id;
            this.Name = name;
            this.ImagePath = imagePath;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
