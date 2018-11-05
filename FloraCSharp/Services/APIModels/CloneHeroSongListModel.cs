using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FloraCSharp.Services.APIModels
{
    internal class CloneHeroSongListModel
    {
        [JsonProperty("songName")]
        public string SongName { get; set; }

        [JsonProperty("artistName")]
        public string ArtistName { get; set; }

        [JsonProperty("albumName")]
        public string AlbumName { get; set; }

        [JsonProperty("genreName")]
        public string GenreName { get; set; }

        [JsonProperty("charterName")]
        public string CharterName { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("Lyrics")]
        public int Lyrics { get; set; }

        [JsonProperty("bandDifficulty")]
        public int BandDifficulty { get; set; }

        [JsonProperty("guitarDifficulty")]
        public int GuitarDifficulty { get; set; }

        [JsonProperty("bassDifficulty")]
        public int BassDifficulty { get; set; }

        [JsonProperty("rhythmDifficulty")]
        public int RhythmDifficulty { get; set; }

        [JsonProperty("drumsDifficulty")]
        public int DrumsDifficulty { get; set; }

        [JsonProperty("keysDifficulty")]
        public int KeysDifficulty { get; set; }

        [JsonProperty("ghlGuitarDifficulty")]
        public int GH1GuitarDifficulty { get; set; }

        [JsonProperty("ghlBassDifficulty")]
        public int GH1BassDifficulty { get; set; }

        [JsonProperty("songLength")]
        public int SongLength { get; set; }
    }
}
