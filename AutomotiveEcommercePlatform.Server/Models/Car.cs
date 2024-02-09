﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AutomotiveEcommercePlatform.Server.Models;

namespace DataBase_LastTesting.Models
{
    public class Car
    {
        public int Id { get; set;}
        [MaxLength(50)]
        public string BrandName { get; set; }
        [MaxLength(50)]
        public string ModelName { get; set;}
        public int ModelYear { get; set; }
        public decimal Price { get; set; }
        [MaxLength(15)]
        public string CarCategory { get; set; }
        public string CarImage { get; set; } // Go Back 
        public bool InStock { get; set; }
        public int? OrderId { get; set; }
        public Trader Trader { get; set; }
        public Order Order { get; set; }
        public List<CarReview> CarReview { get; set;}
        public ICollection<Cart> Cart { get; set; }

        public List<CartItem> CartItems { get; set; }
    }
}