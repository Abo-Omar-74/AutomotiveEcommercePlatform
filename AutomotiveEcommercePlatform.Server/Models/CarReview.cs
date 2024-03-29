﻿using Newtonsoft.Json;

namespace DataBase_LastTesting.Models
{
    public class CarReview
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public int CarId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public Car Car { get; set; }
    }
}
