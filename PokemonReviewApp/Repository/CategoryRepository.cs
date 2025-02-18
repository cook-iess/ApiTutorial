using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Data;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Repository
{
    public class CategoryRepository : ICatagoryRepository
    {
        private readonly DataContext _context;
        public CategoryRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> CreateCategory(Category category)
        {
            await _context.AddAsync(category);
            return await Save();
        }

        public async Task<bool> DeleteCategory(Category category)
        {
            _context.Remove(category);
            return await Save();
        }

        public async Task<ICollection<Category>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategory(int id)
        {
            return await _context.Categories.Where(e => e.Id == id).FirstOrDefaultAsync() ?? throw new InvalidOperationException("Category not found");
        }

        public async Task<ICollection<Pokemon>> GetPokemonByCategory(int categoryId)
        {
            return await _context.PokemonCategories.Where(e => e.CategoryId == categoryId).Select(c => c.Pokemon).ToListAsync();
        }

        public async Task<bool> Save()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCategory(Category category)
        {
            _context.Update(category);
            return await Save();
        }
    }
}
