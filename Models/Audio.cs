using System;
using System.Collections.Generic;
using System.Text;

namespace HAS.Yoga.Functions
{
    public class Audio
    {
        public string Title { get; set; }
        public string FileName { get; set; }
        public Uri Uri { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public List<string> Tags { get; set; }
    }
}
