using System;
using System.Collections.Generic;
using System.Text;

namespace MeanWords.Models
{
    class LyricsResponseModel
    {
        public string success { get; set; }
        public int length { get; set; }
        public LyricsModel result { get; set; }
    }
}
