using Microsoft.AspNetCore.Mvc;
using proyecto.Dtos;
using proyecto.Models;
using System.Threading.Tasks;

namespace proyecto.Interfaces
{
    public interface IFoodDAO
    {
        Task<IEnumerable<Food>> GetFoodsAsync();
        Task<List<Food>> GetFoodsAsyncUpgrade();
        Task<Food> GetFoodByIdAsync(int id);
        Task<bool> AddFoodAsync(Food food);
        Task<bool> UpdateFoodAsync(Food food);
        Task<bool> DeleteFoodAsync(Food food);
        IEnumerable<FoodDto> GetAllDto();
        bool FoodExists(int id);
    }
}
