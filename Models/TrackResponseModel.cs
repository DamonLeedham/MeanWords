using System;
using System.Collections.Generic;
using System.Text;

namespace MeanWords.Models
{
    class TrackResponseModel
    {
        public string success { get; set; }
        public int length { get; set; }
        public TrackModel[] result { get; set; }
    }
}
