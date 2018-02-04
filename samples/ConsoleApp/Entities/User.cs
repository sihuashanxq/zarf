using System;

namespace ConsoleApp.Entities
{
    public class User
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public string Name { get; set; }

        public DateTime CreateDay { get; set; }

        public string Tel { get; set; }

        public override string ToString()
        {
            return
                "Id:" + Id + 
                "\tAge=" + Age + 
                "\tName=" + Name + 
                "\tTel=" + Tel + 
                "\tCreateDay=" + CreateDay.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
