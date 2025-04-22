using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Context;
using proyecto.Dtos;
using proyecto.Interfaces;
using proyecto.Mappers;
using proyecto.Models;

namespace proyecto.Services
{
    public class FoodService : IFoodDAO
    {

        private readonly AppDbContext _context;

        public FoodService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Food>> GetFoodsAsync()
        {
            return await _context.Foods.ToListAsync();
        }

        public Task<List<Food>> GetFoodsAsyncUpgrade()
        {
            return _context.Foods.AsNoTracking().ToListAsync();
        }

        public async Task<Food> GetFoodByIdAsync(int id)
        {
            return await _context.Foods.FindAsync(id);
        }

        public async Task<bool> AddFoodAsync(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFoodAsync(Food food)
        {
            _context.Entry(food).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodExists(food.Id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteFoodAsync(Food food)
        {
            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }


        public bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.Id == id);
        }


        public IEnumerable<FoodDto> GetAllDto()
        {
            return _context.Foods.ToList().Select(s => s.ToFoodDto());
        }

    }
}
