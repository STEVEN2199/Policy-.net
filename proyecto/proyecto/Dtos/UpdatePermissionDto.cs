﻿using System.ComponentModel.DataAnnotations;

namespace proyecto.Dtos
{
    public class UpdatePermissionDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
    }
}
