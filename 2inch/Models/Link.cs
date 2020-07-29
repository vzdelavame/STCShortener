using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _2inch.Models
{
    public class Link
    {
        public int id { get ; set; }
        public string createdBy { get; set; }
        public string longLink { get; set; }
        public string shortLink { get; set; }
        public int clicked { get; set; }
        public string creationTime { get; set; }

        public Link(int ID, string CreatedBy, string LongLink, string ShortLink, int Clicked, string CreationTime)
        {
            this.id = ID;
            this.createdBy = CreatedBy;
            this.longLink = LongLink;
            this.shortLink = ShortLink;
            this.clicked = Clicked;
            this.creationTime = CreationTime;
        }

        public Link()
        {

        }
    }
}
