﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MajesticHotel_HotelAPI.Models.Dto.Rooms
{
    public class RoomCreateDTO
    {
        public int RoomClassId { get; set; }

        public int HotelId { get; set; }
        public bool IsAvailable { get; set; }

    }
}
